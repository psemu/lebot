using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class UserQuitEventArgs : EventArgs
    {
        public IrcUser User;
        public string Reason;

        public UserQuitEventArgs(IrcUser User, string Reason)
        {
            this.User = User;
            this.Reason = Reason;
        }
    }
}
