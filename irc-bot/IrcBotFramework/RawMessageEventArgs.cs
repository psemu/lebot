using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class RawMessageEventArgs : EventArgs
    {
        public string Message;
        public RawMessageEventArgs(string Message)
        {
            this.Message = Message;
        }
    }
}
