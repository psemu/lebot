using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class NickChangedEventArgs : EventArgs
    {
        public NickChangedEventArgs(IrcUser user, string newNick)
        {
            User = user;
            NewNick = newNick;
        }

        public IrcUser User { get; set; }
        public string NewNick { get; set; }
    }
}
