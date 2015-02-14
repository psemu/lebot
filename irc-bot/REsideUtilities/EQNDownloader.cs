using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;




namespace REsideUtility
{
   
    public static class EQNDownloader
    {
     
       

        static void EmptyJobCompleteCallback(string time)
        {
            return;
        }

        public static void DownloadPS2(JObject thisManifest, JObject lastManifest, string outputfolder)
        {
            DownloadPS2Job(thisManifest, lastManifest, outputfolder, EmptyJobCompleteCallback);
        }

        public static string DownloadPS2Job(JObject thisManifest, JObject lastManifest, string outputfolder, FullDownloadJobComplete jobComplete)
        {
            string jobStart = DateTime.Now.ToString();
            DownloadPS2Job(thisManifest, lastManifest, outputfolder, jobComplete, jobStart);
            return jobStart;
        }

        public static void DownloadPS2Job(JObject thisManifest, JObject lastManifest, string outputfolder, FullDownloadJobComplete jobComplete, string jobName)
        {
         
            

            if (!Directory.Exists("output/")) Directory.CreateDirectory("output/");
            if (!Directory.Exists("output/PS2Install")) Directory.CreateDirectory("output/PS2Install");
            if (!Directory.Exists("output/PS2Install/" + outputfolder)) Directory.CreateDirectory("output/PS2Install/" + outputfolder);

            string of = "output/PS2Install/" + outputfolder + "/";


            JToken folders = thisManifest["digest"]["folder"];

            string downloadURL = (string)thisManifest["digest"]["@defaultServerFolder"];

            Downloader.JobCallbacks[jobName] = jobComplete;

            if (lastManifest == null) UpdateFilesInFolder(downloadURL,"", of, folders, null, jobName);
            else
            {
                JToken oldFolders = lastManifest["digest"]["folder"];
                UpdateFilesInFolder(downloadURL, "", of, folders, oldFolders, jobName);
            }
        }

        private static void UpdateFilesInFolder(string downloadurl,string remoteFolder, string outFolder, JToken thisFolder, JToken lastFolder, string job)
        {

            if (!Directory.Exists(outFolder)) Directory.CreateDirectory(outFolder);
            //Sometimes there is a null child
            if (thisFolder == null) return;
            //If the folder is an array of files (not a folder structure), go through each of the folders and parse them
            if (thisFolder.Type == JTokenType.Array)
            {
                foreach (var token in thisFolder)
                {
                    UpdateFilesInFolder(downloadurl, remoteFolder, outFolder, token, lastFolder, job);
                }
                return;
            }


            JToken t = null;
            if (thisFolder["file"] != null)
            {
                if (thisFolder["file"].Type == JTokenType.Array)
                {
                    foreach (var file in thisFolder["file"])
                    {
                        if (lastFolder != null)
                        {
                            t = lastFolder.SelectToken(file.Path.Replace("digest.folder.", ""));

                        }
                        UpdateFile(downloadurl, remoteFolder, outFolder, file, t, job);
                    }

                }
                else
                {
                    if (lastFolder != null)
                    {
                        t = lastFolder.SelectToken(thisFolder.Path.Replace("digest.folder.", ""));
                        t = t["file"];

                    }
                    UpdateFile(downloadurl, remoteFolder, outFolder, thisFolder["file"], t, job);
                }
            }

            if (thisFolder["folder"] != null)
            {
                if (thisFolder["folder"].Type == JTokenType.Array)
                {
                    foreach (JToken jsonfolder in thisFolder["folder"])
                    {
                        if (jsonfolder["@name"] == null)
                        {
                            UpdateFilesInFolder(downloadurl,remoteFolder, outFolder, jsonfolder, lastFolder, job);
                        }
                        else
                        {
                            string name = (string)jsonfolder["@name"];
                            UpdateFilesInFolder(downloadurl, remoteFolder + name + "/", outFolder + name + "/", jsonfolder, lastFolder, job);
                        }
                    }
                }
                else
                {
                    thisFolder = thisFolder["folder"];
                    if (thisFolder["@name"] == null)
                    {
                        UpdateFilesInFolder(downloadurl, remoteFolder, outFolder, thisFolder["folder"], lastFolder, job);
                    }
                    else
                    {

                        string name = (string)thisFolder["@name"];
                        UpdateFilesInFolder(downloadurl, remoteFolder + name + "/", outFolder + name + "/", thisFolder["folder"], lastFolder, job);
                    }
                }
            }

        }

        private static void UpdateFile(string downloadurl, string remoteFolder, string outfolder, JToken file, JToken oldFile, string job)
        {
            if (file["@delete"] != null) return;
             string filename = (string)file["@name"];


            //If oldFile is null, that means this is a new file.
            if (oldFile != null)
            {
                string oldFilename = (string)oldFile["@name"];

                if (oldFilename == filename)
                {
                    return;
                }
            }

            

            string downloadURL = downloadurl + "/" + remoteFolder + filename + ".zs";

            if (file["@uncompressedSize"] == null || file["@compressedSize"] == null)
            {
                return; //If there is no size the file doesn't exist on the server
            }
            else
            {
                int uncompressedsize = (int)file["@uncompressedSize"];
                int compressedsize = (int)file["@compressedSize"];

                if (uncompressedsize > compressedsize)
                {
                    Downloader.DownloadAndDecompress(filename, downloadURL, outfolder, job);
                }
                else
                {
                    Downloader.Download(filename, downloadURL, outfolder, job);
                }
            }

            Console.WriteLine("Downloading: " + filename);


        }



    }
}
