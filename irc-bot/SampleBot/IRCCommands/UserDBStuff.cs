using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBotFramework;

namespace Lebot
{
    partial class PS2StatBot
    {

        UserDB userdb = new UserDB();

        public void InitUserDB()
        {
            userdb = UserDB.Load();

            this.UserJoinedChannel += new EventHandler<UserJoinEventArgs>(UserDB_UserJoined);
            this.UserPartedChannel += new EventHandler<UserPartEventArgs>(UserDB_UserPart);
            this.ChannelListRecieved += new EventHandler<ChannelListEventArgs>(PS2StatBot_ChannelListRecieved);

        }

        void PS2StatBot_ChannelListRecieved(object sender, ChannelListEventArgs e)
        {
            foreach (string user in e.Channel.Users)
            {
                User u = userdb.GetUserByName(user);
                if (u == null) continue;
                u.Online = true;

                UserMessage[] messages = userdb.GetMessagesSinceTime(u, DateTime.Now);
                if (messages.Length > 0)
                {
                    foreach (UserMessage m in messages)
                    {
                        this.SendMessage(user, m.Message);
                    }
                    SaveDB();
                }
            }
        }

        public void SaveDB()
        {
            userdb.Save();
        }

        public string RegisterUser(IrcCommand command)
        {
            userdb.RegisterUser(command.Source.Nick, command.Parameters[0], command.Source.Hostmask);
            SaveDB();
            
            return "Done.  Your password is stored with SHA256";
        }

        public string SendMessage(IrcCommand command)
        {
            string username = command.Parameters[0];
            string message = command.Message.Remove(0, username.Length + 1);
            message = "From " + command.Source.Nick + " on " + DateTime.Now.ToString() + ": " + message;

            return SendMemo(username, message) ? "Message Sent" : "User does not exist";

        }

        public bool SendMemo(string username, string message)
        {
            User user = userdb.GetUserByName(username);
            if (user != null && user.Online)
            {
                SendMessage(username, message);
                return true;
            }           

            bool m = userdb.SendMessage(username, message);
            if (m) SaveDB();

            return m;

           
        }

        public void SystemMessage(string message)
        {
            foreach (User user in userdb.SysMessageRecievers)
            {
                SendMemo(user.DisplayName, "From lebot on " + DateTime.Now + ": " + message);
            }
        }

        public string ToggleSysMessageRecieve(IrcCommand command)
        {
            User user = userdb.GetUserByNameHost(command.Source.Nick, command.Source.Hostmask);
            if (user == null) return "You are not registered.  Send me a query with '.register <password>' so I know who you are";

            userdb.ToggleSysMessages(user);
            SaveDB();
            return "Done";
        }

        void UserDB_UserJoined(object sender, UserJoinEventArgs e)
        {
            User user = userdb.GetUserByName(e.User.Nick);
            if (user == null) return;
            user.Online = true;

            UserMessage[] messages = userdb.GetMessagesSinceTime(user, DateTime.Now);
            if (messages.Length > 0)
            {
                foreach(UserMessage m in messages)
                {
                    this.SendMessage(e.User.Nick, m.Message);
                }
                SaveDB();
            }
          

        }

        void UserDB_UserPart(object sender, UserPartEventArgs e)
        {
            User user = userdb.GetUserByName(e.User.Nick);
            if (user == null) return;

            user.Online = false;
            user.LastOnline = DateTime.Now;

            SaveDB();
        }


    }
}
