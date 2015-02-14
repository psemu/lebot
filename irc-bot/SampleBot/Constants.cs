using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lebot
{

    

    enum Branch
    {
        Live,
        Test,
        EQNLandmark,      
        The9,
        The9_Admin,
    }


    class Constants
    {
        public const int UpdateInterval = 10;

        public const string LauncherUserAgent = "Quicksilver Player/1.0.3.183 (Windows; PlanetSide 2)";

        public static Dictionary<Branch, string> Manifests = new Dictionary<Branch, string> {
            { Branch.Live, "http://manifest.patch.station.sony.com/patch/sha/manifest/planetside2/planetside2-live/live/planetside2-live.sha.soe"},
            { Branch.Test, "http://manifest.patch.station.sony.com/patch/sha/manifest/planetside2/planetside2-test/live/planetside2-test.sha.soe"},
            { Branch.EQNLandmark, "http://manifest.patch.station.sony.com/patch/eqnext/test/digest/play/test64-cdn.soe" },           
            { Branch.The9, "http://patch.ps2.the9.com/patch/sha/manifest/ps2/play/live.sha.th9"},
            { Branch.The9_Admin, ""},
        };

        public static Dictionary<Branch, string> Directories = new Dictionary<Branch, string> {
#if DEV
            { Branch.Live, "Live/"},
            { Branch.Test, "E:\\Planetside2PTR" },
            { Branch.EQNLandmark, "EQNLandmark/" },   
#else
            { Branch.Live, "output/PS2Install/Live/" },
            { Branch.Test, "output/PS2Install/Test/" },
            { Branch.EQNLandmark, "output/PS2Install/EQNLandmark/" },    
#endif
            { Branch.The9, "ChinaLive-Repo/" },
            { Branch.The9_Admin, "ChinaAdmin-Repo/" },
        };

        public static Dictionary<Branch, string> DisplayName = new Dictionary<Branch, string>
        {
            { Branch.Live, "Live" },
            { Branch.Test, "Test" },
            { Branch.EQNLandmark, "EQNLandmark" },            
            { Branch.The9, "China_Live" },
            { Branch.The9_Admin, "China_Admin" },
        };

        public static Dictionary<Branch, string> BranchFileNames = new Dictionary<Branch, string>
        {
            { Branch.Live, "output/RevisionData/livedb.txt" },
            { Branch.Test, "output/RevisionData/testdb.txt" },
            { Branch.EQNLandmark, "output/RevisionData/EQNLandmark.txt" },          
            { Branch.The9, "output/RevisionData/PS2_The9Live.txt" },
            { Branch.The9_Admin, "PS2_The9Admin.txt" },
        };

        public static Dictionary<Branch, string[]> NotifyChannels = new Dictionary<Branch, string[]>()
        {
            { Branch.Live,  new string[] { "#REside", "#ps-universe", "#reside_priv" } },
            { Branch.Test, new string[] { "#REside", "#ps-universe", "#reside_priv" } },
            { Branch.EQNLandmark, new string[] { "#REside", "#ps-universe", "#reside_priv" } },           
            { Branch.The9, new string[] { "#REside", "#reside_priv" } },
            { Branch.The9_Admin, new string[] { "#reside_priv" } },
        };

        public static Dictionary<Branch, bool> PublicBranch = new Dictionary<Branch, bool>()
        {
            { Branch.Live,  true },
            { Branch.Test, true },
            { Branch.EQNLandmark, true },         
            { Branch.The9, true },
            { Branch.The9_Admin, false },
        };
        
    }
}
