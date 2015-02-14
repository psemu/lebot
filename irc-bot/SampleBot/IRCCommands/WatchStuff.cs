using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Net;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;
using Newtonsoft.Json.Linq;
using REsideUtility;
using Lebot.PS2FileApi;

namespace Lebot
{
    [Serializable]
    public class TrackData
    {
        public string Name;
        public string Manifest;
        public DateTime LastUpdated;
        public string TrackedBy;

        public string[] Events = new string[] { "NotifyPrivate", "DumpManifestToFile", "DateManifest" };
       
    }

    delegate void NotifyDelegate(TrackData data);

    partial class PS2StatBot
    {
        List<TrackData> ManifestTrackList = new List<TrackData>();

        const string TrackedManifestFile = "TrackedManifests.txt";

        const int CheckMinutes = 30;


        Dictionary<string, NotifyDelegate> EventTypes = new Dictionary<string, NotifyDelegate>();

       
        public void StartTimers()
        {
            Timer t = new Timer(1000 * 60 * Constants.UpdateInterval);

            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Enabled = true;

            if (!System.IO.File.Exists(TrackedManifestFile))
            {
                using (JsonTextWriter wr = new JsonTextWriter(new System.IO.StreamWriter(TrackedManifestFile)))
                {
                    JsonSerializer ser = new JsonSerializer();
                    wr.Formatting = Formatting.Indented;
                    ser.Serialize(wr, ManifestTrackList);
                }

            }

            using (JsonTextReader r = new JsonTextReader(new System.IO.StreamReader(TrackedManifestFile)))
            {
                JsonSerializer ser = new JsonSerializer();
                ManifestTrackList = ser.Deserialize<List<TrackData>>(r);
            }

            EventTypes["NotifyPublic"] = (data) =>
            {
                string message = "SOE Patched " + data.Name +" on " + data.LastUpdated.ToString();
                SendMessage("#ps-universe", message);
                SendMessage("#REside", message);
                SystemMessage(message);
            };

            EventTypes["NotifyPrivate"] = (data) =>
            {
                string message = " Tracked Manifest " + data.Name + " was modified on " + data.LastUpdated.ToString();
                SendMessage("#reside_priv", message);
            };

            EventTypes["DumpManifestToFile"] = (data) =>
            {
                string[] filename = DumpManifestToFile(data.Manifest, "Manifests/Tracked/" + data.Name + "/", string.Format("{0}{1}.txt", data.Manifest.Substring(data.Manifest.LastIndexOf("/") + 1), DateTime.Now.ToString("yyyy-MM-dd")));

                foreach (string s in filename)
                {
                    this.SendMessage("#reside_priv", string.Format("http://testoutfit.info/lebot/{0}", s));
                }

            };

            EventTypes["DateManifest"] = (data) =>
            {
                using (System.IO.StreamWriter wr = new System.IO.StreamWriter(System.IO.File.Open("output/Manifests/Tracked/" + data.Name + "UpdateDates.txt", System.IO.FileMode.Append)))
                {
                    wr.WriteLine(data.LastUpdated.ToString());
                    this.SendMessage("#reside_priv", string.Format("Updated http://testoutfit.info/lebot/Manifests/Tracked/{0}", data.Name + "UpdateDates.txt"));
                }

            };

            EventTypes["SeeTheFuture"] = (data) =>
            {
                string message = "";
                if (data.Name == "livenext") message = "I predict a Live patch is going to happen in the near future";
                if (data.Name == "testnext") message = "I predict a Test patch is going to happen in the near future";
                SendMessage("#ps-universe", message);
                SendMessage("#REside", message);
            };
            
            EventTypes["QueueLiveDownload"] = (data) =>
            {
                DateTime now = DateTime.Now;

                
              
                SendMessage("#reside_priv", "Live has started downloading");

                string[] manifests = new string[]
                    {                        
                        "http://manifest.patch.station.sony.com/patch/sha/manifest/planetside2/planetside2-live/live/planetside2-live.sha.soe",
                        "http://manifest.patch.station.sony.com/patch/sha/manifest/planetside2/planetside2-livecommon/live/planetside2-livecommon.sha.soe",
                      
                    };

                foreach (string manifest in manifests)
                {

                    JObject thisManifest = Downloader.GetJobjectFromManifest(manifest);


                    PS2Downloader.DownloadPS2Job(thisManifest, null, "Live", dateTime =>
                        {
                            SendMessage("#reside_priv", "Live has finished Downloading");
                            Branch b = Branch.Live;

                            string analysis = PS2Analyzer.AnalyzePS2(Constants.Directories[b]);

                            string branchName = "Live-" + now.ToString("yyyy-MM-dd");
                            AutoAnalyze("Live", analysis, branchName);

                            SendMessage("#reside", "Live Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                            SendMessage("#ps-universe", "Live Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                            SendMessage("#reside_priv", "Live Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));

                        }, "LiveDownload");
                }
                
            };

            EventTypes["QueueTestDownload"] = (data) =>
            {
                DateTime now = DateTime.Now;

            
                             
                SendMessage("#reside_priv", "Test has started downloading");

                  string[] manifests = new string[]
                    {                        
                        "http://manifest.patch.station.sony.com/patch/sha/manifest/planetside2/planetside2-test/live/planetside2-test.sha.soe",
                        "http://manifest.patch.station.sony.com/patch/sha/manifest/planetside2/planetside2-testcommon/live/planetside2-testcommon.sha.soe",
                      
                    };

                  foreach (string manifest in manifests)
                  {
                      JObject thisManifest = Downloader.GetJobjectFromManifest(manifest);


                      PS2Downloader.DownloadPS2Job(thisManifest, null, "Test", dateTime =>
                      {
                          SendMessage("#reside_priv", "Test has finished Downloading");
                          Branch b = Branch.Test;

                          string analysis = PS2Analyzer.AnalyzePS2(Constants.Directories[b]);

                          string branchName = "Test-" + now.ToString("yyyy-MM-dd");
                          AutoAnalyze("Test", analysis, branchName);

                          SendMessage("#reside", "Test Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                          SendMessage("#ps-universe", "Test Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                          SendMessage("#reside_priv", "Test Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));

                      }, "TestDownload");

                  }

            };

            EventTypes["QueueEQNLDownload"] = (data) =>
                {
                    DateTime now = DateTime.Now;

                    SendMessage("#reside_priv", "EQNLandmark has started downloading");

                    string[] manifests = new string[]
                    {                        
                        "http://manifest.patch.station.sony.com/patch/eqnext/test/digest/play/test64-cdn.soe",
                        "http://manifest.patch.station.sony.com/patch/eqnext/test/digest/play/shared-cdn.soe",
                        "http://manifest.patch.station.sony.com/patch/eqnext/test/digest/common/shared-cdn.soe",
                        "http://manifest.patch.station.sony.com/patch/eqnext/test/digest/common/test-cdn.soe",
                    };


                    foreach(string manifest in manifests)
                    {
                        JObject thisManifest = Downloader.GetJobjectFromManifest(manifest);

                        EQNDownloader.DownloadPS2Job(thisManifest, null, "EQNLandmark", dateTime =>
                        {
                            SendMessage("#reside_priv", "EQNLandmark has finished Downloading");

                            Branch b = Branch.EQNLandmark;

                            string analysis = PS2Analyzer.AnalyzePS2(Constants.Directories[b]);

                            string branchName = "EQNLandmark-" + now.ToString("yyyy-MM-dd");
                            AutoAnalyze("EQNLandmark", analysis, branchName);

                            SendMessage("#reside", "EQNLandmark Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                            SendMessage("#ps-universe", "EQNLandmark Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                            SendMessage("#reside_priv", "EQNLandmark Changes: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", branchName + "Changes.txt"));
                            
                        }, "EQNLDownload");

                    }
                   
                };


        }

        public void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Request Sent!");
            
            try
            {
                
                foreach (TrackData data in ManifestTrackList)
                {
                    try
                    {
                       
                        DetectChange(data);
                        
                    }
                    catch (Exception ex)
                    {
                        SendMessage("#reside_priv", "Error while querying tracked data: " + data.Name + " error: " + ex.ToString().Haste());
                    }
                }

            }
            catch (Exception ex)
            {
                this.SendMessage("#REside_priv", "Error while querying last patch: " + ex.ToString().Haste());
            }
           
           
        }

        public string AllTrackedFiles(IrcBotFramework.IrcCommand command)
        {
            if (command.Destination != "reside_priv") return "You cannot use this command";

            foreach (TrackData dat in ManifestTrackList)
            {
                SendMessage(command.Destination, string.Format("{0} is tracking {1} last updated {2} url: {3}", dat.TrackedBy, dat.Name, dat.LastUpdated, dat.Manifest));
            }

            return "Done";
        }

        public string TrackManifestFile(IrcBotFramework.IrcCommand command)
        {
            if (command.Destination != "#reside_priv") return "You cannot use this command";

            if (command.Parameters.Length != 2) return ".trackmanifest <name> <manifesturl>";

            string name = command.Parameters[0];
            if (ManifestTrackList.Where(x => x.Name == name).DefaultIfEmpty(null).FirstOrDefault() != null)
            {
                return "Already tracking that!";
            }

            string url = command.Parameters[1];
            string owner = command.Source.Nick;

            TrackData dat = new TrackData()
            {
                Name = name,
                Manifest = url,
                TrackedBy = owner,
                LastUpdated = DateTime.MinValue,
            };

            ManifestTrackList.Add(dat);

            using (JsonTextWriter wr = new JsonTextWriter(new System.IO.StreamWriter(TrackedManifestFile)))
            {
                JsonSerializer ser = new JsonSerializer();
                wr.Formatting = Formatting.Indented;
                ser.Serialize(wr, ManifestTrackList);
            }


            return "Done";
        }

        public string LastUpdate(IrcBotFramework.IrcCommand command)
        {
            if(command.Parameters.Length == 1 && command.Parameters[0] == "all" && command.Destination == "#reside_priv")
            {
                foreach(TrackData d in ManifestTrackList.OrderByDescending(d => d.LastUpdated))
                {
                     SendMessage(command.Destination, d.Name + " was last updated on: " + d.LastUpdated.ToString());
                }
                return "Done.";
            }

            string[] PublicTracked = { "Live", "Test", "China_Live", "EQNLandmark" };

            foreach (TrackData d in ManifestTrackList.Where(d => PublicTracked.Contains(d.Name)).OrderByDescending(d => d.LastUpdated))
            {
                SendMessage(command.Destination, d.Name + " was last updated on: " + d.LastUpdated.ToString());
            }


            return "Done.";
        }


        public string AnalyzeChanges(IrcBotFramework.IrcCommand command)
        {
            string branch = command.Parameters[0];

            Branch b = (Branch)Enum.Parse(typeof(Branch), branch);

            string analysis = PS2Analyzer.AnalyzePS2(Constants.Directories[b]);

            string revName = branch + DateTime.Now.ToString("yyyy-MM-dd");
            AutoAnalyze(branch, analysis, revName);




            return "Done: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", revName + "Changes.txt");
        }


        void DetectChange(TrackData d)
        {
            HttpWebRequest request = WebRequest.Create(d.Manifest) as HttpWebRequest;
            request.Method = "HEAD";
            request.UserAgent = Constants.LauncherUserAgent;

            HttpWebResponse r = request.GetResponse() as HttpWebResponse;
            
            if (r.LastModified > d.LastUpdated)
            {
                d.LastUpdated = r.LastModified;

                string ev = "";
                try
                {
                    //Do the events
                    foreach (string e in d.Events)
                    {
                        EventTypes[e](d);
                        ev = e;
                    }

                }
                catch(Exception e)
                {
                    SendMessage("#reside_priv", "Error while doing event " + ev + " error: " + e.ToString().Haste());
                }
             
             
                using (JsonTextWriter wr = new JsonTextWriter(new System.IO.StreamWriter(TrackedManifestFile)))
                {
                    JsonSerializer ser = new JsonSerializer();
                    wr.Formatting = Formatting.Indented;
                    ser.Serialize(wr, ManifestTrackList);
                }
            }

        }

        

    }
}
