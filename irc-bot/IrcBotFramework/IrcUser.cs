using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class IrcUser
    {
        public IrcUser(string Host)
        {
            string[] mask = Host.Split('@', '!');
            if(mask.Length < 3)
            {
                this.Nick = "";
                this.User = "";
                this.Hostname = Host;
            }
            else
            {
                this.Nick = mask[0];
                this.User = mask[1];
                this.Hostname = mask[2];
            }

        }

        public IrcUser(string Nick, string User)
        {
            this.Nick = Nick;
            this.User = User;
            this.RealName = User;
        }

        public IrcUser(string Nick, string User, string Password) : this(Nick, User)
        {
            this.Password = Password;
        }

        public IrcUser(string Nick, string User, string Password, string RealName) : this(Nick, User, Password)
        {
            this.RealName = RealName;
        }

        public string Nick, User, Password, RealName;
        private string _hostname;
        public string Hostname
        {
            get
            {
                return _hostname;
            }
            internal set
            {
                _hostname = value;
            }
        }

        public string Hostmask
        {
            get
            {
                return Nick + "!" + User + "@" + Hostname;
            }
        }
    }
}
