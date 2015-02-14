using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class IrcChannel
    {
        public IrcChannel(string Channel)
        {
            this.Name = Channel;
            this.Users = new List<string>();
        }

        public string Name;
        public string Topic;
        public List<string> Users;
    }
}
