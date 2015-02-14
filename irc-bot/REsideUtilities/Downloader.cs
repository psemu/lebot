using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using REsideUtility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REsideUtility
{
    public delegate void FullDownloadJobComplete(string jobstart);


    class DownloadJob
    {
        public string name;
        public string url;
        public string output;
        public bool decompress;
        public string status;
        public string jobtag;

    }

    public class Downloader
    {

        static ConcurrentQueue<DownloadJob> DownloadQueue = new ConcurrentQueue<DownloadJob>();
        static List<DownloadJob> DownloadingQueue = new List<DownloadJob>();
        public static ConcurrentDictionary<string, FullDownloadJobComplete> JobCallbacks = new ConcurrentDictionary<string, FullDownloadJobComplete>();

        static Thread DownloadWorker = null;



        public static void Download(string filename, string url, string outputfilename, string job)
        {
            DoDownloadJob(new DownloadJob()
            {
                url = url,
                output = outputfilename,
                decompress = false,
                name = filename,
                status = filename + " Has Not Started",
                jobtag = job,
            });
        }

        public static void DownloadAndDecompress(string filename, string url, string outputfilename, string job)
        {
            DoDownloadJob(new DownloadJob()
            {
                url = url,
                output = outputfilename,
                decompress = true,
                name = filename,
                status = filename + " Has Not Started",
                jobtag = job
            });



        }

        private static void DoDownloadJob(DownloadJob job)
        {
            if (DownloadWorker == null)
            {
                DownloadWorker = new Thread(new ThreadStart(DownloadThreadWork));
                DownloadWorker.Start();
            }

            DownloadQueue.Enqueue(job);
        }



        public static int DownloadJobsToComplete()
        {
            return DownloadQueue.Count;
        }

        public static string[] ReportDownloadStatus()
        {
            List<string> s = new List<string>();
            s.Add("Files In Queue: " + DownloadQueue.Count);
            Parallel.ForEach(DownloadingQueue, j =>
            {
                if (j == null) return;
                s.Add(j.status);
            });

            return s.ToArray();
        }


        public static JObject GetJobjectFromManifest(string Manifest)
        {

            WebClient c = new WebClient();
            string d = c.DownloadString(Manifest + ".txt");

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(d);

            string jsonText = JsonConvert.SerializeXmlNode(doc, Formatting.Indented);
            return JObject.Parse(jsonText);
        }

        const int MaxFilesToDownload = 1;
        static int FilesBeingDownloaded = 0;

        public static List<string> JobErrors = new List<string>();

        static object CountLock = new object();
        public static void DownloadThreadWork()
        {
            WebClient cl = new WebClient();

            cl.Headers.Add("user-agent", "Quicksilver Player/1.0.3.183 (Windows; PlanetSide 2)");

            cl.DownloadProgressChanged += new DownloadProgressChangedEventHandler(cl_DownloadProgressChanged);
            cl.DownloadDataCompleted += new DownloadDataCompletedEventHandler(cl_DownloadDataCompleted);


            while (true)
            {

                lock (CountLock)
                {
                    if (FilesBeingDownloaded < MaxFilesToDownload)
                    {
                        DownloadJob job;
                        if (DownloadQueue.TryDequeue(out job))
                        {

                            try
                            {
                                cl.DownloadDataAsync(new Uri(job.url), job);
                            }
                            catch (WebException e)
                            {

                            }

                            FilesBeingDownloaded++;
                        }
                        DownloadingQueue.Add(job);
                    }
                }

                Thread.Sleep(100);
            }

        }

        static void cl_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            DownloadJob job = (DownloadJob)e.UserState;
            if (e.Cancelled)
            {
                job.status = job.name + " Cancelled";
                Console.WriteLine("Job was Cancelled!");

                return;
            }
            if (e.Error != null)
            {
                job.status = job.name + " ERROR";
                Console.WriteLine("Error: " + e.Error);

                lock (CountLock)
                {

                    JobErrors.Add(string.Format("File {0} at {1} had error {2}", job.name, job.url, e.Error.Message));

                    DownloadingQueue.Remove(job);

                    FilesBeingDownloaded--;

                }

                return;
            }


            byte[] data = e.Result;
            if (job.decompress)
            {
                job.status = job.name + " is Decompressing";
                data = SevenZip.Compression.LZMA.SevenZipHelper.Decompress(data);
            }

            using (BinaryWriter wr = new BinaryWriter(File.Open(job.output + job.name, FileMode.OpenOrCreate)))
            {
                job.status = job.name + " is Writing";
                if (data == null)
                {

                    job.status = job.name + " NULL DATA";
                    return;
                }

                wr.Write(data);
            }
            job.status = job.name + " is Waiting on lock";

            lock (CountLock)
            {
                int jobsLeft = DownloadQueue.Where(s => s.jobtag == job.jobtag).Count();

                if (jobsLeft == 0)
                {
                    JobCallbacks[job.jobtag](job.jobtag);
                    FullDownloadJobComplete d;
                    JobCallbacks.TryRemove(job.jobtag, out d);

                }

                FilesBeingDownloaded--;

                DownloadingQueue.Remove(job);
            }

            job.status = job.name + " Done";

            Console.WriteLine(job.name + " is Done");
        }

        static void cl_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadJob j = (DownloadJob)e.UserState;
            j.status = string.Format("Downloading {0}: {1}% ({2}/{3})", j.name, e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);

        }
    }
}
