using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class NoticeEventArgs : EventArgs
    {
        public IrcNotice Notice;

        public NoticeEventArgs(IrcNotice Notice)
        {
            this.Notice = Notice;
        }
    }
}
