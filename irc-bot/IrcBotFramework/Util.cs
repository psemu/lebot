using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace IrcBotFramework
{
    static class Util
    {
        private static readonly Regex HasteKeyRegex = new Regex(@"{""key"":""(?<key>[a-z].*)""}", RegexOptions.Compiled);


        public static string Haste(this string message)
        {

            string hastebin = @"http://hastebin.com/";

            using (WebClient client = new WebClient())
            {

                string response = client.UploadString(hastebin + "documents", message);
                var match = HasteKeyRegex.Match(response);
                if (!match.Success)
                {
                    return "Error: " + response;
                }


                return hastebin + match.Groups["key"];
            }

        }


    }
}
