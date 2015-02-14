using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace REsideUtility
{
    static class Util
    {


        private static readonly Regex HasteKeyRegex = new Regex(@"{""key"":""(?<key>[a-z].*)""}", RegexOptions.Compiled);


        public static string Haste(this string message)
        {

            string hastebin = @"http://hastebin.com/";

            using (WebClient client = new WebClient())
            {
                string response;
                try
                {
                    response = client.UploadString(hastebin + "documents", message);
                }
                catch (System.Net.WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            return "Error: string was too big for hastebin!";
                        }
                        else if(((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                        {
                            return "Error: Page not found";
                        }
                    }
                    throw e;
                }


                var match = HasteKeyRegex.Match(response);
                if (!match.Success)
                {
                    return "Error: " + response;
                }


                return hastebin + match.Groups["key"];
            }

        }


        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, IComparable> projectionToComparable)
        {
            using (var e = source.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    throw new InvalidOperationException("Sequence is empty.");
                }
                TSource max = e.Current;
                IComparable maxProjection = projectionToComparable(e.Current);
                while (e.MoveNext())
                {
                    IComparable currentProjection = projectionToComparable(e.Current);
                    if (currentProjection.CompareTo(maxProjection) > 0)
                    {
                        max = e.Current;
                        maxProjection = currentProjection;
                    }
                }
                return max;
            }
        }

        public static string MySqlEscape(this string usString)
        {
            if (usString == null)
            {
                return null;
            }
            // SQL Encoding for MySQL Recommended here:
            // http://au.php.net/manual/en/function.mysql-real-escape-string.php
            // it escapes \r, \n, \x00, \x1a, baskslash, single quotes, and double quotes
            return Regex.Replace(usString, @"[\r\n\x00\x1a\\'""]", @"\$0");
        }

        const ConsoleColor DebugColor = ConsoleColor.Cyan;

        public static void ConsoleLog(string s)
        {
#if DEV
            
            Console.ForegroundColor = DebugColor;
            Console.WriteLine(s);
            Console.ResetColor();
#endif
        }
    }
}
