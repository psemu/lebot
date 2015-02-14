using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBotFramework;
using Newtonsoft.Json;
using System.IO;
using REsideUtility;
using System.Timers;
using System.Net;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;
using Newtonsoft.Json.Linq;

namespace Lebot
{
    partial class PS2StatBot
    {
       

        public string[] DumpManifestToFile(string manifest, string folder, string name )
        {
            if (!Directory.Exists("output/Manifests")) Directory.CreateDirectory("output/Manifests");
            if (!Directory.Exists("output/"+folder)) Directory.CreateDirectory("output/" +folder);


            string filename = folder + name;

            string localFilename = "output/" + filename;

            List<string> allmanifestFiles = new List<string>();
            allmanifestFiles.Add(filename);
            if (!System.IO.File.Exists(localFilename))
            {
                
                WebClient c = new WebClient();
                string d = c.DownloadString(manifest + ".txt");

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(d);

                string jsonText = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);

                using(StreamWriter wr = new StreamWriter(localFilename))
                {
                    wr.Write(jsonText);
                }

            }

            return allmanifestFiles.ToArray(); ;
        }
    
        public string DiffManifests(IrcCommand command)
        {
           

            if (command.Parameters.Length > 2) return ".diffmanifests <manifest1> <manifest2>.";
            string directory = "output/";
            if(!Directory.Exists(directory + "Diffs/")) Directory.CreateDirectory(directory + "Diffs/");

            string outputfilename = command.Source.Nick + "diff";
            while (System.IO.File.Exists(directory + "Diffs/" + outputfilename + ".html"))
            {
                outputfilename += "_";
            }

            string f1 = command.Parameters[0];
            string f2 = command.Parameters[1];

            string output = Util.DiffTwoFiles(directory + "Manifests/" + f1, directory + "Manifests/" + f2);

            using (StreamWriter wr = new StreamWriter(directory + "Diffs/" + outputfilename + ".html"))
            {
                wr.Write(output);
            }

            Timer t = new Timer(1000 * 60 * 5);
            t.Elapsed += new ElapsedEventHandler((o, e) =>
            {
                System.IO.File.Delete(directory + "Diffs/" + outputfilename + ".html");
                SendMessage("#reside_priv", outputfilename + ".html has expired");
            });

            return "Done: " + string.Format("http://www.testoutfit.info/lebot/Diffs/{0}.html", outputfilename);
        }



        public string DownloadStatus(IrcCommand command)
        {
            //if (command.Destination != "#reside_priv") return "NOPERM";
            int jobs = Downloader.DownloadJobsToComplete();
            if (jobs == 0) return "No Downloads In Progress.";

            string[] Statusii = Downloader.ReportDownloadStatus();
            if (Statusii.Length > 5) return "Done: " + string.Join("\n", Statusii).Haste();

            foreach (string s in Statusii)
            {
                SendMessage(command.Destination, s);
            }
            return "Done.";
        }

        public string ForceDownload(IrcCommand command)
        {
            if (command.Destination != "#reside_priv") return "NOPERM";
            if (command.Parameters.Length != 2) return ".forcedownload <manifest> <output>";

            string manifest = command.Parameters[0];
            string output = command.Parameters[1];
            JObject ob = Downloader.GetJobjectFromManifest(manifest);

            PS2Downloader.DownloadPS2Job(ob, null, output, td =>
                {
                    SendMessage("#reside_priv", "Finished Downloading");
                });


            return "Queued";
        }

    }
}
