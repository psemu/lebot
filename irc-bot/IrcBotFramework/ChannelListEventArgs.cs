using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class ChannelListEventArgs : EventArgs
    {
        public IrcChannel Channel;
        public ChannelListEventArgs(IrcChannel channel)
        {
            this.Channel = channel;
        }
    }
}
