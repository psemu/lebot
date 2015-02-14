using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class UserPartEventArgs : EventArgs
    {
        public IrcUser User;
        public IrcChannel Channel;
        public string Reason;

        public UserPartEventArgs(IrcUser User, IrcChannel Channel, string Reason)
        {
            this.User = User;
            this.Channel = Channel;
            this.Reason = Reason;
        }
    }
}
