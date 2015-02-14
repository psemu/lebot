using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace REsideUtility
{
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class Revision : IComparable
    {
        public string name;
        public DateTime creation;

        public List<string> InterestingFiles = new List<string>();

        public Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"rd", ""},
            {"ca", ""},
            {"it", ""},
        };

        public string[] GetITTags()
        {
            if (data["it"] == "") return new string[] { };
            return data["it"].Split(';');
        }



        public void MarkInterestingFile(string filename)
        {
            InterestingFiles.Add(filename);
        }

        public string ReportInterestingFiles()
        {
            if (InterestingFiles.Count == 0) return "none";
            StringBuilder builder = new StringBuilder();

            foreach (string f in InterestingFiles)
            {
                builder.AppendLine(f);
            }

            return builder.ToString().Trim();

        }

        public void ModifyProperty(string key, string value)
        {
            data[key] = value;
        }

        public string GetProperties()
        {
            return "Release Date: " + data["rd"] + '\n' + "Content Analysis: " + data["ca"] + '\n' + "IT Tags: " + data["it"] + '\n' + "Interesting File Count: " + InterestingFiles.Count;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Revision)) throw new Exception();
            return ((Revision)obj).creation.CompareTo(creation);
        }

        public override string ToString()
        {
            return name;
        }
    }


    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class FileEntry
    {
        public string name;

        public Dictionary<string, FileRevisionData> revisions;

        [NonSerialized]
        public bool touched = false;


    }



    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class FileRevisionData
    {
        public RevisionAction action;
        public int size;
        public ulong crc;

        public override string ToString()
        {
            if (action == RevisionAction.ADD) return "ADD";
            if (action == RevisionAction.REMOVE) return "DEL";
            return string.Format("{0} - size: {1} crc: {2}", action, size, crc); ;
        }
    }
}
