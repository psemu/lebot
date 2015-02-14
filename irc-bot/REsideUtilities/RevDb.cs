using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;


namespace REsideUtility
{
    public enum RevisionAction
    {
        ADD,
        REMOVE,
        MODIFY,
    }

    [Serializable]
    public class RevDb
    {
        static Revision None = new Revision()
        {
            name = "None",
            creation = DateTime.MinValue

        };

        public List<Revision> AllRevisions = new List<Revision>();

        public Dictionary<string, FileEntry> RevTable = new Dictionary<string, FileEntry>();

        public DateTime LastPatch = DateTime.MinValue;

        [NonSerialized]
        public Revision openRev = null;


        public bool RevisionOpen
        {
            get { return openRev != null; }
        }

        public void OpenRevision(string name)
        {
            Revision rev = AllRevisions.Where(c => c.name == name).DefaultIfEmpty(None).FirstOrDefault();
            if (rev.CompareTo(None) == 0) CreateRevision(name);
            else openRev = rev;
            foreach (FileEntry e in RevTable.Values)
            {
                e.touched = false;
            }
        }

        public void CloseRevision()
        {
            openRev = null;
        }

        private void CreateRevision(string revname)
        {
            Revision n = new Revision()
            {
                name = revname,
                creation = DateTime.Now,
            };
            AllRevisions.Add(n);
            openRev = n;
        }

        public void CalculateRevision(string name, ulong crc, int size)
        {

            if (!RevTable.ContainsKey(name)) AddFile(name, crc, size);
            else ModifyFile(name, crc, size);
        }

        public void CheckForRemovedFile()
        {
            foreach (FileEntry file in RevTable.Values.Where(e => !e.touched))
            {
                Revision latest = openRev;
                string closest = "";
                try
                {
                    closest = file.revisions.Keys.Where(t => GetRevisionByName(t).creation < latest.creation).OrderByDescending(t => GetRevisionByName(t).creation).First();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.ToString());
                }
                if(file.revisions[closest].action != RevisionAction.REMOVE) RemoveFile(file.name);
            }
        }

        private void AddFile(string name, ulong crc, int size)
        {
            //Mark interesting files
            string[] itTags = openRev.GetITTags();
            foreach (string s in itTags)
            {

                if (name.Contains(s, StringComparison.OrdinalIgnoreCase)) openRev.MarkInterestingFile(name);
            }

            FileEntry rev = new FileEntry()
            {
                name = name,
                revisions = new Dictionary<string, FileRevisionData>()
                {
                    {openRev.name, new FileRevisionData() 
                            {
                                action = RevisionAction.ADD,
                                size = size,
                                crc = crc,
                            } },
                },
            };

            RevTable[name] = rev;
            RevTable[name].touched = true;
        }

        private void ModifyFile(string name, ulong crc, int size)
        {
            try
            {
                FileEntry r = RevTable[name];
                Revision latest = (openRev == null) ? LatestRev() : openRev;
                string closest = "";
                try
                {
                    closest = r.revisions.Keys.Where(t => GetRevisionByName(t).creation < latest.creation).OrderByDescending(t => GetRevisionByName(t).creation).First();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.ToString());
                }
                //CRC or size are different. 
                if (r.revisions[closest].crc != crc || r.revisions[closest].size != size)
                {
                    //Mark interesting files
                    string[] itTags = openRev.GetITTags();
                    foreach (string s in itTags)
                    {
                        if (name.Contains(s, StringComparison.OrdinalIgnoreCase)) openRev.MarkInterestingFile(name);
                    }


                    FileRevisionData data = new FileRevisionData()
                    {
                        action = RevisionAction.MODIFY,
                        crc = crc,
                        size = size,
                    };
                    r.revisions[openRev.name] = data;
                    
                }
                RevTable[name].touched = true;
            }
            catch (KeyNotFoundException e)
            {
                throw new Exception(e.Message + " , " + name);
            }
        }

        private void RemoveFile(string name)
        {
            FileEntry r = RevTable[name];
            FileRevisionData data = new FileRevisionData()
            {
                action = RevisionAction.REMOVE,
                crc = 0,
                size = 0,
            };
            r.revisions[openRev.name] = data;
        }

        private Revision LatestRev()
        {
            AllRevisions.Sort();
            return AllRevisions.First();
        }

        public Revision GetRevisionByName(string name)
        {
            return AllRevisions.Where(t => t.name == name).DefaultIfEmpty(null).First();
        }

        public string GetFilesForRevision(Revision rev)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Revision Report for " + rev.name);
            builder.AppendLine();

            int countmod = 0;

            foreach (string s in RevTable.Where(x => x.Value.revisions.ContainsKey(rev.name) && x.Value.revisions[rev.name].action == RevisionAction.MODIFY).Select(x => x.Key).OrderBy(x=> x))
            {
               
                builder.AppendLine(string.Format("{0} \t\t {1}", RevTable[s].revisions[rev.name].action.ToString(), s));
                
                countmod++;
            }

            int countadd = 0;
            foreach (string s in RevTable.Where(x => x.Value.revisions.ContainsKey(rev.name) && x.Value.revisions[rev.name].action == RevisionAction.ADD).Select(x => x.Key).OrderBy(x => x))
            {
               
                builder.AppendLine(string.Format("{0} \t\t {1}", RevTable[s].revisions[rev.name].action.ToString(), s));
               
                countadd++;
            }

            int countremove = 0;
            foreach (string s in RevTable.Where(x => x.Value.revisions.ContainsKey(rev.name) && x.Value.revisions[rev.name].action == RevisionAction.REMOVE).Select(x => x.Key).OrderBy(x => x))
            {

                builder.AppendLine(string.Format("{0} \t\t {1}", RevTable[s].revisions[rev.name].action.ToString(), s));

                countremove++;
            }

            builder.AppendLine();
            builder.AppendLine("Total Files Changed: " + (countadd + countmod + countremove));
            builder.AppendLine("Total Files Modified: " + countmod);
            builder.AppendLine("Total Files Added: " + countadd);
            builder.AppendLine("Total Files Removed: " + countremove);
            return builder.ToString().Trim();
        }

        public string FindFile(string partialName)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string name in RevTable.Keys)
            {
                if (name.Contains(partialName, StringComparison.OrdinalIgnoreCase)) builder.AppendLine(name);

            }

            return builder.ToString().Trim();
        }

        public void RecalcInterestingFiles(Revision rev)
        {
           
            rev.InterestingFiles.Clear();

            string[] itTags = rev.GetITTags();
            foreach (string s in RevTable.Keys)
            {
                if (RevTable[s].revisions.ContainsKey(rev.name))
                {

                    foreach (string tag in itTags)
                    {
                        if (s.Contains(tag, StringComparison.OrdinalIgnoreCase)) rev.MarkInterestingFile(s);
                    }
                }
            }
        }

        public bool DeleteRevision(Revision rev)
        {
            if (!AllRevisions.Contains(rev)) return false;
            AllRevisions.Remove(rev);
            

            foreach (FileEntry entry in RevTable.Values)
            {
                if(entry.revisions.ContainsKey(rev.ToString()))
                {
                    entry.revisions.Remove(rev.ToString());
                }
            }
            CleanOrphanedFiles();

            return true;
        }

        public static bool SaveDatabase(RevDb db, string name)
        {
            JsonSerializer ser = new JsonSerializer();
            ser.PreserveReferencesHandling = PreserveReferencesHandling.None;
            Directory.CreateDirectory("output/RevisionData/");

            using (StreamWriter sw = new StreamWriter(name))
            using (JsonWriter wr = new JsonTextWriter(sw))
            {
                ser.Serialize(wr, db);
            }
            return true;
        }

        public static RevDb LoadDatabase(string name)
        {
            if (!File.Exists(name))
            {
                RevDb rdb = new RevDb();
                RevDb.SaveDatabase(rdb, name);
                
            }
            RevDb db;
            JsonSerializer ser = new JsonSerializer();
            using (StreamReader r = new StreamReader(name))
            using (JsonTextReader reader = new JsonTextReader(r))
            {
                db = ser.Deserialize<RevDb>(reader);
            }
            return db;
        }

        public int CleanOrphanedFiles()
        {
            List<string> markedForDeletion = new List<string>();
           
            foreach (KeyValuePair<string, FileEntry> entry in RevTable)
            {
                if (entry.Value.revisions.Count == 0)
                    markedForDeletion.Add(entry.Key);
            }
            foreach (string e in markedForDeletion)
            {
                RevTable.Remove(e);
            }
            return markedForDeletion.Count;
        }

    }
}
