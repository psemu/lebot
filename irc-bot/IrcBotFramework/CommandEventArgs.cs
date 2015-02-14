using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class CommandEventArgs : EventArgs
    {
        public IrcCommand Command { get; set; }

        public CommandEventArgs(IrcCommand command)
        {
            Command = command;
        }
    }
}
