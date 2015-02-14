using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IrcBotFramework
{
    public class IrcMessage
    {
        /// <summary>
        /// Parses a raw message string received from the server. See RFC 1459, 2.3.1 for details on raw message syntax.
        /// </summary>
        /// <param name="RawMessage"></param>
        public IrcMessage(string RawMessage)
        {
            this.RawMessage = RawMessage;
            var matches = Regex.Match(RawMessage,
                @"(?::(?<Prefix>[^ ]+) +)?(?<Command>[^ :]+)" + 
                @"(?<payload>(?<middle>(?: +[^ :]+)*)(?<coda> +:(?<trailing>.*)?)?)");

            Prefix = matches.Groups["Prefix"].Value;
            Command = matches.Groups["Command"].Value;
            Payload = matches.Groups["payload"].Value.Trim();

            var parameters = new List<string>();
            parameters.AddRange(matches.Groups["middle"].Value
                .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            parameters.Add(matches.Groups["trailing"].Value);

            Parameters = parameters.ToArray();
        }

        public string RawMessage, Prefix, Command, Payload;
        public string[] Parameters;
    }
}
