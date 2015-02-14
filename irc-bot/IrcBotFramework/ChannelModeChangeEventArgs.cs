using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class ChannelModeChangeEventArgs : EventArgs
    {
        public IrcChannel Channel;
        public IrcUser Source;
        public string ModeChange;
    }
}
