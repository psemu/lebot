#define OLD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using IrcBotFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lebot
{
    public partial class PS2StatBot : IrcBot
    {
        public PS2StatBot(string ServerAddress, IrcUser User) : base(ServerAddress, User)
        {
            LoadBot();
            StartTimers();
            InitUserDB();

#if OLD
            RegisterCommand("openrev", OpenRevisionOLD);
            RegisterCommand("lookup", LookupFileOld);
            RegisterCommand("rawadd", CalculateRevisionOLD);
            RegisterCommand("closerev", CloseRevisionOLD);
            RegisterCommand("report_chng", DumpChangesInRevOLD);
            RegisterCommand("find", FindFileOLD);
            RegisterCommand("changebranch", SwitchBranchOLD);
#else
            RegisterCommand("openrev", OpenRevision);
            RegisterCommand("lookup", LookupFile);
            RegisterCommand("rawadd", RawAdd);
            RegisterCommand("closerev", CloseRevision);
            RegisterCommand("report_chng", ReportChangesInRev);
            RegisterCommand("find", Find);
            RegisterCommand("changebranch", SwitchBranch);
#endif

            // .ping
            RegisterCommand("ping", command => "get off my dick");
            RegisterCommand("lastquery", commad => lastquery);
            RegisterCommand("stats", QueryStats);            
            //RegisterCommand("mark", MarkFile);          
            RegisterCommand("help", command => "https://github.com/RoyAwesome/ps2ls/wiki/lebot");            
            RegisterCommand("ps2ls", command => "https://github.com/RoyAwesome/ps2ls");
            RegisterCommand("deleterevision", DeleteRevision);
            RegisterCommand("cleanorphanedfiles", CleanOrphandFiles);
            
            RegisterCommand("lastupdate", LastUpdate);
            RegisterCommand("register", RegisterUser);
            RegisterCommand("sendmessage", SendMessage);
            RegisterCommand("togglesysmessages", ToggleSysMessageRecieve);
            
            RegisterCommand("trackmanifest", TrackManifestFile);
            RegisterCommand("tracklist", AllTrackedFiles);

            RegisterCommand("diffmanifests", DiffManifests);

            RegisterCommand("downloadstatus", DownloadStatus);
            RegisterCommand("forcedownload", ForceDownload);

            RegisterCommand("forceanalyze", AnalyzeChanges);

            RegisterCommand("dumprevisions", DumpRevs);
        }
        
    }
}
