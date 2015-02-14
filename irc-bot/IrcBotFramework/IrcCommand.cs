using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IrcBotFramework
{
    public class IrcCommand
    {
        public IrcCommand(IrcUser Source, string Message, string Prefix)
        {
            this.Source = Source;
            this.Prefix = Prefix;
            if (IndexOfWhitespace(Message) == -1)
                this.Message = "";
            else
                this.Message = Message.Substring(IndexOfWhitespace(Message) + 1).Trim();
            string[] parts = SplitOnWhitespace(Message);
            this.Command = parts[0].Substring(1);
            if (parts.Length > 1)
                this.Parameters = parts.Skip(1).ToArray();
            else
                this.Parameters = new string[0];
        }

        public IrcUser Source;
        public string Message, Command, Prefix;
        public string[] Parameters;
        public string Destination;

        private int IndexOfWhitespace(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i]))
                    return i;
            }
            return -1;
        }

        private string[] SplitOnWhitespace(string value)
        {
            string[] result = new string[1];
            result[0] = "";
            bool inString = false, inChar = false;
            foreach (char c in value)
            {
                bool foundChar = false;
                if (char.IsWhiteSpace(c))
                {
                    foundChar = true;
                    result = result.Concat(new string[] { "" }).ToArray();
                }
                if (!foundChar)
                    result[result.Length - 1] += c;
            }
            return result;
        }
    }
}
