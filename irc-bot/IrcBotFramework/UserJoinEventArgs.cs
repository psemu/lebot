using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class UserJoinEventArgs : EventArgs
    {
        public IrcUser User;
        public IrcChannel Channel;

        public UserJoinEventArgs(IrcUser User, IrcChannel Channel)
        {
            this.User = User;
            this.Channel = Channel;
        }
    }
}
