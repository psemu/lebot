using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBotFramework;
using System.IO;
using System.Net;
using System.Timers;
using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;
using Newtonsoft.Json.Linq;
using System.Threading;
using Lebot.PS2FileApi;
using REsideUtility;

namespace Lebot
{

    
    class Program
    {
        
        static StreamWriter logWriter;
        public static PS2StatBot bot;

        static void Main(string[] args)
        {
#if TEST

            string analysis = PS2Analyzer.AnalyzePS2(@"E:\SteamGames\SteamApps\common\PlanetSide 2");

            string revName = "test";

            RevDb db = new RevDb();
            db.OpenRevision(revName);
            foreach (string s in analysis.Split('\n'))
            {
                if (s == "") continue;
                string[] revdata = s.Split('\t');
                db.CalculateRevision(revdata[0], ulong.Parse(revdata[2]), int.Parse(revdata[1]));
            }
            db.CheckForRemovedFile();
            db.CloseRevision();

            Revision r = db.GetRevisionByName(revName);
            string d = db.GetFilesForRevision(r);
            using (StreamWriter wr = new StreamWriter("output/RevisionReports/" + r.name + "Changes.txt"))
            {
                wr.Write(d);
                wr.Flush();
                wr.Close();
            }

#else
                      
#if DEV
            string name = "lebot_dev";
#else
            string name = "lebot";
#endif
            logWriter = new StreamWriter("log.txt");  
          
            bot = new PS2StatBot("irc.planetside-universe.com", new IrcUser(name, name+"1"));
            bot.ConnectionComplete += new EventHandler(bot_ConnectionComplete);
            bot.RawMessageRecieved += new EventHandler<RawMessageEventArgs>(bot_RawMessage);
            bot.RawMessageSent += new EventHandler<RawMessageEventArgs>(bot_RawMessage);
            bot.Run();
            
           
        
            while (true) ;
#endif
            
        }

       

        

        static void bot_ConnectionComplete(object sender, EventArgs e)
        {
           
            bot.JoinChannel("#ps-universe");
#if DEV            
            bot.PartChannel("#ps-universe", "");
#else            
            bot.JoinChannel("#api");
#endif
            bot.JoinChannel("#REside");

            bot.JoinChannel("#REside_priv", "password removed") //Intential compile error here.  Change this if you want to use it yourself

            bot.t_Elapsed(null, null);
           
        }

        static void bot_RawMessage(object sender, RawMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
            logWriter.WriteLine(e.Message);
            logWriter.Flush();
        }
    }
}
