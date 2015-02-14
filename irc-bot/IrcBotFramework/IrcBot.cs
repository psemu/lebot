using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace IrcBotFramework
{
    public abstract class IrcBot
    {
        private byte[] ReadBuffer;
        private int ReadBufferIndex;
        private DateTime LastPingRecieved = DateTime.MaxValue, LastPongRecieved = DateTime.MaxValue;
        private string ServerPingAddress;
        private Timer PingTimer;

        public Socket IrcConnection;
        public string ControlCharacter;
        public string ServerHostname;
        public int ServerPort;
        public Encoding Encoding;
        public IrcUser User;
        public List<IrcChannel> Channels;
        public ChanServ ChanServ;
        public DateTime LastRawMessageSent;

        public string ServerAddress
        {
            get
            {
                return ServerHostname + ":" + ServerPort;
            }
            set
            {
                string[] parts = value.Split(':');
                this.ServerHostname = parts[0];
                if (parts.Length > 1)
                    this.ServerPort = int.Parse(parts[1]);
                else
                    this.ServerPort = 6667;
            }
        }

        public event EventHandler ConnectionComplete;
        public event EventHandler<UserMessageEventArgs> UserMessageRecieved;
        public event EventHandler<NoticeEventArgs> NoticeRecieved;
        public event EventHandler<UserJoinEventArgs> UserJoinedChannel;
        public event EventHandler<UserPartEventArgs> UserPartedChannel;
        public event EventHandler<UserQuitEventArgs> UserQuit;
        public event EventHandler<ChannelListEventArgs> ChannelListRecieved;
        public event EventHandler<ChannelModeChangeEventArgs> ChannelModeChanged;
        public event EventHandler<RawMessageEventArgs> RawMessageRecieved, RawMessageSent;
        public event EventHandler<CommandEventArgs> PreCommand, PostCommand;
        /// <summary>
        /// Fired when any nick changes, not just your own.
        /// </summary>
        public event EventHandler<NickChangedEventArgs> NickChanged;
        public event EventHandler ServerQuit;

        public IrcBot(string ServerAddress, IrcUser user, string ControlCharacter, Encoding encoding)
        {
            this.ServerAddress = ServerAddress;
            this.ControlCharacter = ControlCharacter;
            this.RegisteredCommands = new Dictionary<string, CommandHandler>();
            this.Encoding = encoding;
            this.User = user;
            this.Channels = new List<IrcChannel>();
            this.ChanServ = new ChanServ(this);
            this.LastRawMessageSent = DateTime.MinValue;
        }

        public IrcBot(string ServerAddress, IrcUser User) :
            this(ServerAddress, User, ".", Encoding.UTF8)
        {
            
        }

        public void Run()
        {
            IrcConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ReadBuffer = new byte[1024];
            ReadBufferIndex = 0;
            IrcConnection.BeginConnect(ServerHostname, ServerPort, ConnectComplete, null);
            PingTimer = new Timer(o =>
                {
                    if ((DateTime.Now - LastPingRecieved).TotalSeconds < 30)
                        SendRawMessage("PING " + ServerPingAddress);
                }, null, 30000, 30000);
        }

        private void ConnectComplete(IAsyncResult result)
        {
            IrcConnection.EndConnect(result);
            IrcConnection.BeginReceive(ReadBuffer, ReadBufferIndex, ReadBuffer.Length, SocketFlags.None, RecieveComplete, null);
            if (User.Password != null)
                SendRawMessage("PASS " + User.Password);
            SendRawMessage("NICK " + User.Nick);
            SendRawMessage("USER " + User.User + " 0.0.0.0 server :" + User.RealName);
        }

        private void RecieveComplete(IAsyncResult result)
        {
            int length = IrcConnection.EndReceive(result);
            List<string> messages = new List<string>();
            byte[] data = ReadBuffer.Take(length + ReadBufferIndex).ToArray();
            ReadBufferIndex = 0;

            while (data.Length > 0)
            {
                int messageLength = Array.IndexOf(data, (byte)'\n');
                if (messageLength == -1) // incomplete message
                {
                    Array.Copy(data, 0, ReadBuffer, 0, data.Length);
                    ReadBufferIndex = data.Length;
                    break;
                }
                string message = Encoding.GetString(data, 0, messageLength).Trim('\r');
                data = data.Skip(messageLength + 1).ToArray();
                messages.Add(message);
            }
            foreach (var message in messages)
            {
                if (RawMessageRecieved != null)
                    RawMessageRecieved(this, new RawMessageEventArgs(message));
                HandleMessage(new IrcMessage(message));
            }
            IrcConnection.BeginReceive(ReadBuffer, ReadBufferIndex, ReadBuffer.Length - ReadBufferIndex, SocketFlags.None, RecieveComplete, null);
        }

        private void HandleMessage(IrcMessage message)
        {
            // scope in switch...case is stupid in C#
            string raw, channelName, reason;
            IrcChannel channel;
            switch (message.Command)
            {
                case "001":
                    if (ConnectionComplete != null)
                        ConnectionComplete(this, null);
                    break;
                case "PRIVMSG":
                    IrcCommand command = null;
                    IrcUserMessage userMessage = new IrcUserMessage(message);
                    if (UserMessageRecieved != null)
                        UserMessageRecieved(this, new UserMessageEventArgs(userMessage));
                    string messagePrepend = "", destination = userMessage.Target;
                    string sourceNick = userMessage.Source.Nick;
                    if (userMessage.Message.Contains("@"))
                    {
                        sourceNick = userMessage.Message.Substring(userMessage.Message.IndexOf("@") + 1).Trim();
                        userMessage.Message = userMessage.Message.Remove(userMessage.Message.IndexOf("@"));
                    }
                    if (message.Payload.StartsWith("#")) // channel message
                        messagePrepend = sourceNick + ": \u000f";
                    if (userMessage.Message.StartsWith(ControlCharacter))
                    {
                        command = new IrcCommand(userMessage.Source, userMessage.Message.Trim(), messagePrepend);
                        command.Destination = destination;
                        if (RegisteredCommands.ContainsKey(command.Command.ToLower()))
                        {
                            Thread tempThread = new Thread(() =>
                            {
                                try
                                {
                                    if (PreCommand != null)
                                    {
                                        var eventArgs = new CommandEventArgs(command);
                                        PreCommand(this, eventArgs);
                                        command = eventArgs.Command;
                                    }
                                    string response = RegisteredCommands[command.Command.ToLower()](command);
                                    if (response != null)
                                        SendMessage(destination, messagePrepend + response);
                                    if (PostCommand != null)
                                        PostCommand(this, new CommandEventArgs(command));
                                }
                                catch (Exception e) { 
                                    SendMessage(destination, messagePrepend + "Error: " + e.ToString().Haste());
                                    
                                }
                            });
                            tempThread.Start();
                        }
                    }
                    break;
                case "PING":
                    SendRawMessage("PONG " + message.Payload);
                    ServerPingAddress = message.Payload;
                    LastPingRecieved = DateTime.Now;
                    break;
                case "PONG":
                    LastPongRecieved = DateTime.Now;
                    break;
                case "332": // Channel topic
                    raw = message.Payload.Substring(message.Payload.IndexOf(' ') + 1);
                    channelName = raw.Remove(raw.IndexOf(' ') );
                    raw = raw.Substring(raw.IndexOf(' ') + 1).Substring(1);
                    channel = GetChannel(channelName);
                    channel.Topic = raw;
                    break;
                case "353":
                    raw = message.Payload.Substring(message.Payload.IndexOf(' ') + 3);
                    channelName = raw.Remove(raw.IndexOf(' '));
                    raw = raw.Substring(raw.IndexOf(':') + 1);
                    string[] names = raw.Split(' ');
                    channel = GetChannel(channelName);
                    foreach (var user in names)
                    {
                        if (user.StartsWith("+"))
                            channel.Users.Add(user.Substring(1));
                        else if (user.StartsWith("@"))
                            channel.Users.Add(user.Substring(1));
                        else
                            channel.Users.Add(user);
                    }
                    break;
                case "366": // End of names
                    raw = message.Payload.Substring(message.Payload.IndexOf(' ') + 1);
                    channelName = raw.Remove(raw.IndexOf(' '));
                    if (ChannelListRecieved != null)
                        ChannelListRecieved(this, new ChannelListEventArgs(GetChannel(channelName)));
                    break;
                case "MODE":
                    if (message.Payload.StartsWith("#"))
                    {
                        ChannelModeChangeEventArgs eventArgs = new ChannelModeChangeEventArgs();
                        eventArgs.Source = new IrcUser(message.Prefix);
                        eventArgs.Channel = GetChannel(message.Payload.Remove(message.Payload.IndexOf(' ')));
                        eventArgs.ModeChange = message.Payload.Substring(message.Payload.IndexOf(' ') + 1);
                        if (ChannelModeChanged != null)
                            ChannelModeChanged(this, eventArgs);
                    }
                    break;
                case "NOTICE":
                    if (NoticeRecieved != null)
                        NoticeRecieved(this, new NoticeEventArgs(new IrcNotice(message)));
                    break;
                case "PART":
                    // #IAmABotAMA :\"hello world\"
                    channelName = message.Parameters[0];
                    channel = GetChannel(channelName);
                    reason = message.Payload.Substring(message.Payload.IndexOf(':') + 1);
                    if (new IrcUser(message.Prefix).Nick != User.Nick)
                    {
                        channel.Users.Remove(new IrcUser(message.Prefix).Nick);
                        if (UserPartedChannel != null)
                            UserPartedChannel(this, new UserPartEventArgs(new IrcUser(message.Prefix), channel, reason));
                    }
                    break;
                case "JOIN":
                    channelName = message.Parameters[0];
                    channel = GetChannel(channelName);
                    channel.Users.Add(new IrcUser(message.Prefix).Nick);
                    if (UserJoinedChannel != null)
                        UserJoinedChannel(this, new UserJoinEventArgs(new IrcUser(message.Prefix), channel));
                    break;
                case "QUIT":
                    reason = message.Payload.Substring(message.Payload.IndexOf(':') + 1);
                    if (new IrcUser(message.Prefix).Nick != User.Nick)
                    {
                        foreach (var c in Channels)
                        {
                            if (c.Users.Contains(new IrcUser(message.Prefix).Nick))
                                c.Users.Remove(new IrcUser(message.Prefix).Nick);
                        }
                        if (UserQuit != null)
                            UserQuit(this, new UserQuitEventArgs(new IrcUser(message.Prefix), reason));
                    }
                    else
                    {
                        if (ServerQuit != null)
                            ServerQuit(this, null);
                        // Reconnect
                        if (IrcConnection.Connected)
                            IrcConnection.Disconnect(false);
                        IrcConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        ReadBuffer = new byte[1024];
                        ReadBufferIndex = 0;
                        IrcConnection.BeginConnect(ServerHostname, ServerPort, ConnectComplete, null);
                    }
                    break;
                case "311": // WHOIS response
                    string[] whois = message.Payload.Split(' ');
                    var result = new IrcUser(whois[1], whois[2].Substring(1));
                    result.Hostname = whois[3];
                    if (WhoIsCallback != null)
                    {
                        WhoIsCallback(result);
                        WhoIsCallback = null;
                    }
                    break;
                case "TOPIC":
                    if (TopicCallback != null)
                    {
                        TopicCallback(message.Payload.Substring(message.Payload.IndexOf(":") + 1));
                        TopicCallback = null;
                    }
                    break;
                case "NICK":
                    if (NickChanged != null)
                        NickChanged(this, new NickChangedEventArgs(new IrcUser(message.Prefix), message.Payload));
                    break;
            }
        }

        private Action<IrcUser> WhoIsCallback; // TODO: Make this better
        public void WhoIs(string Nick, Action<IrcUser> Callback)
        {
            WhoIsCallback = Callback;
            this.SendRawMessage("WHOIS " + Nick);
        }

        public void Invite(string Channel, string User)
        {
            this.SendRawMessage("INVITE " + User + " " + Channel);
        }

        public void Topic(string Channel, string NewTopic)
        {
            this.SendRawMessage("TOPIC " + Channel + " :" + NewTopic);
        }

        private Action<string> TopicCallback; // TODO: Make this better
        public void Topic(string Channel, Action<string> TopicCallback)
        {
            this.TopicCallback = TopicCallback;
            this.SendRawMessage("TOPIC " + Channel);
        }

        public void Kick(string channel, string user, string reason)
        {
            this.SendRawMessage("KICK " + channel + " " + user + " :" + reason);
        }

        public IrcChannel GetChannel(string name)
        {
            return Channels.Where(c => c.Name == name).DefaultIfEmpty(Channels.First()).FirstOrDefault();
        }

        public void ChangeMode(string Destination, string Mode)
        {
            SendRawMessage("MODE " + Destination + " " + Mode);
        }

        public void SendMessage(string Destination, string Message)
        {
            SendRawMessage("PRIVMSG " + Destination + " :" + Message);
        }

        public void SendNotice(string Destination, string Message)
        {
            SendRawMessage("NOTICE " + Destination + " :" + Message);
        }

        public void SendAction(string Destination, string Action)
        {
            SendRawMessage("PRIVMSG " + Destination + " :\u0001ACTION " + Action + "\u0001");
        }

        public void SendRawMessage(string Message)
        {
            SendRawMessage(Message, null);
        }

        public void SendRawMessage(string Message, AsyncCallback Callback)
        {
            while (DateTime.Now < LastRawMessageSent.AddMilliseconds(500))
                Thread.Sleep(100);
            LastRawMessageSent = DateTime.Now;
            if (this.RawMessageSent != null)
                RawMessageSent(this, new RawMessageEventArgs(Message));
            byte[] data = Encoding.GetBytes(Message + "\r\n");
            IrcConnection.BeginSend(data, 0, data.Length, SocketFlags.None, Callback, null);
        }

        public void JoinChannel(string Channel)
        {
            if (Channels.Where(c => c.Name == Channel).Count() != 0)
                return;
            Channels.Add(new IrcChannel(Channel));
            SendRawMessage("JOIN " + Channel);
        }

        public void JoinChannel(string Channel, string Keyword)
        {
            if (Channels.Where(c => c.Name == Channel).Count() != 0)
                return;
            Channels.Add(new IrcChannel(Channel));
            SendRawMessage("JOIN " + Channel + " " + Keyword);
        }

        public void PartChannel(string Channel, string Message)
        {
            if (Channels.Where(c => c.Name == Channel).Count() == 0)
                return;
            Channels.Remove(Channels.Where(c => c.Name == Channel).First());
            SendRawMessage("PART " + Channel);
        }

        private Dictionary<string, CommandHandler> RegisteredCommands;
        /// <summary>
        /// Return the response string, or null to not respond.
        /// </summary>
        public delegate string CommandHandler(IrcCommand Command);
        protected void RegisterCommand(string Command, CommandHandler Handler)
        {
            RegisteredCommands.Add(Command, Handler);
        }
    }
}
