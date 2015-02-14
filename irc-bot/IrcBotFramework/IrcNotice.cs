using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class IrcNotice
    {
        public IrcNotice(IrcMessage message)
        {
            this.Source = message.Prefix;
            this.Message = message.Payload.Substring(message.Payload.IndexOf(':') + 1);
        }

        public string Source;
        public string Message;
    }
}
