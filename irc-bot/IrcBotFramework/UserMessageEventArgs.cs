using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class UserMessageEventArgs : EventArgs
    {
        public IrcUserMessage UserMessage;

        public UserMessageEventArgs(IrcUserMessage UserMessage)
        {
            this.UserMessage = UserMessage;
        }
    }
}
