using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class IrcUserMessage
    {
        public IrcUserMessage(IrcMessage Message)
        {
            this.Target = Message.Payload.Remove(Message.Payload.IndexOf(' '));
            this.Message = Message.Payload.Substring(Message.Payload.IndexOf(':') + 1);
            string[] parts = Message.Prefix.Split('!', '@');
            this.Source = new IrcUser(parts[0], parts[1]);
            this.Source.Hostname = parts[2];
            if (this.Target.StartsWith("#"))
                ChannelMessage = true;
            else
                this.Target = this.Source.Nick;
        }

        public IrcUser Source;
        public string Message;
        public string Target;
        public bool ChannelMessage;
    }
}
