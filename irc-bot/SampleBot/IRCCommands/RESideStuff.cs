using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrcBotFramework;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using REsideUtility;

namespace Lebot
{
    
    partial class PS2StatBot
    {

        Branch CurrentBranchOLD = Branch.Live;

        private Dictionary<Branch, RevDb> RevDBs = new Dictionary<Branch, RevDb>();


        public string OpenRevisionOLD(IrcCommand command)
        {
            if (RevDBs[CurrentBranchOLD].RevisionOpen) return "Cannot open revision while " + RevDBs[CurrentBranchOLD].openRev + " is Open";
            string name = command.Parameters[0];

            RevDBs[CurrentBranchOLD].OpenRevision(name);

            return "Opened " + name;
        }

        public string CloseRevisionOLD(IrcCommand command)
        {
            RevDBs[CurrentBranchOLD].CloseRevision();
            RevDb.SaveDatabase(RevDBs[CurrentBranchOLD], Constants.BranchFileNames[CurrentBranchOLD]);
            return "Done";
        }

        public string LookupFileOld(IrcCommand command)
        {
            string filename = command.Parameters[0];
            if (!RevDBs[CurrentBranchOLD].RevTable.ContainsKey(filename)) return  "No file by that name (try .find <filename>)";

            foreach (KeyValuePair<string, FileRevisionData> entry in RevDBs[CurrentBranchOLD].RevTable[filename].revisions)
            {
                SendMessage(command.Destination, string.Format("Version: {0}, Action {1}", entry.Key, entry.Value.action.ToString()));
            }

            return "Done";
        }


        public string FindFileOLD(IrcCommand command)
        {
            string name = command.Parameters[0];
            string searchstring = RevDBs[CurrentBranchOLD].FindFile(name);

            return "Done: " + searchstring.Haste();

        }

        public string CalculateRevisionOLD(IrcCommand command)
        {
            if (!RevDBs[CurrentBranchOLD].RevisionOpen) return "No Revision Open";

            using (StreamReader reader = new StreamReader(command.Parameters[0]))
            {                
                while (!reader.EndOfStream)
                {
                    string[] revdata = reader.ReadLine().Split('\t');
                    RevDBs[CurrentBranchOLD].CalculateRevision(revdata[0], ulong.Parse(revdata[2]), int.Parse(revdata[1]));
                   
                }
                RevDBs[CurrentBranchOLD].CheckForRemovedFile();

            }

            return "Done";
        }

        public string DumpChangesInRevOLD(IrcCommand command)
        {
            string rev = command.Parameters[0];
            Revision r = RevDBs[CurrentBranchOLD].GetRevisionByName(rev);
            if (r == null) return "No revision by that name";

            
            if (!System.IO.File.Exists("output/RevisionReports/" + r.name + "Changes.txt"))
            {
                string data = RevDBs[CurrentBranchOLD].GetFilesForRevision(r);
                using (StreamWriter wr = new StreamWriter("output/RevisionReports/" + r.name + "Changes.txt"))
                {
                    wr.Write(data);
                    wr.Flush();
                    wr.Close();
                }
            }

            return "Done: " + string.Format("http://www.testoutfit.info/lebot/RevisionReports/{0}", r.name + "Changes.txt");
        }

        public string ReportChangesInRevOLD(IrcCommand command)
        {
            string rev = command.Parameters[0];
            Revision r = RevDBs[CurrentBranchOLD].GetRevisionByName(rev);
            if (r == null) return "No revision by that name";

            string data = RevDBs[CurrentBranchOLD].GetFilesForRevision(r);

            return "Done: " + data.Haste();
        }

        public string SwitchBranchOLD(IrcCommand command)
        {
            string name = command.Parameters[0];
            Branch b;
            bool success = Enum.TryParse(name, out b);

            //Either branch does not exist, or we've selected a private branch in a public channel
            if (!success || (!Constants.PublicBranch[b] && command.Destination != "#reside_priv"))
            {
                Branch[] branches = null;
                if (command.Destination == "#reside_priv")
                {
                    branches = AllBranches.ToArray();
                }
                else
                {
                    branches = AllBranches.Where(x => Constants.PublicBranch[x]).ToArray();
                }

                string bString = "{ ";
                foreach (Branch branch in branches)
                {
                    bString += branch.ToString() + ", ";
                }
                bString = bString.Remove(bString.Length - 2) + " }";
                return "Branch not found: " + name + ". Possible Branches: " + bString;
            }

            if (b == CurrentBranchOLD) return "Already on branch: " + CurrentBranchOLD.ToString();

            CurrentBranchOLD = b;

            return "Switched to " + CurrentBranchOLD.ToString();
        }



        public string DeleteRevision(IrcCommand command)
        {

            Revision rev = RevDBs[CurrentBranchOLD].GetRevisionByName(command.Parameters[0]);
            if (rev == null) return "No Revision " + command.Parameters[0];
            RevDBs[CurrentBranchOLD].DeleteRevision(rev);

            return "Done";

        }
        public string CleanOrphandFiles(IrcCommand command)
        {
            int num = RevDBs[CurrentBranchOLD].CleanOrphanedFiles();
            return "Done";
        }

        public string DumpRevs(IrcCommand command)
        {
            string data = "";
            foreach(Revision rev in RevDBs[CurrentBranchOLD].AllRevisions)
            {
                data += rev.name + "\n";
            }

            return "Done: " + data.Haste();
        }



        List<Branch> AllBranches = new List<Branch>();
        Branch CurBranch;

        private void LoadBot()
        {
            foreach (Branch b in Enum.GetValues(typeof(Branch)))
            {
                RevDBs[b] = RevDb.LoadDatabase(Constants.BranchFileNames[b]);
            }


            AllBranches.AddRange((Branch[])Enum.GetValues(typeof(Branch)));
            CurBranch = AllBranches.DefaultIfEmpty(AllBranches.First())
                        .Where(b => b == Branch.Live)
                        .First();           
        }


       
        public void AutoAnalyze(string branch, string data, string revName)
        {
            Branch b = (Branch)Enum.Parse(typeof(Branch), branch);
            RevDb db = RevDBs[b];

            db.OpenRevision(revName);
            foreach(string s in data.Split('\n'))
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

            RevDb.SaveDatabase(RevDBs[CurrentBranchOLD], Constants.BranchFileNames[CurrentBranchOLD]);

        }
        
     
    }
}
