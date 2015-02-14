using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using IrcBotFramework;

namespace Lebot
{
    partial class PS2StatBot
    {
        const string QueryPath = @"http://census.soe.com/";
        const string GetPath = @"get/";
        const string PS2Live = @"ps2/";
        const string PS2Beta = @"ps2-beta/";
        const string sid = @"s:ircstatbot/";

        string lastquery;

        static Dictionary<int, string> worlds = new Dictionary<int, string>(){
            {1 , "Connery"},
            {2 , "Genudine"},
            {3 , "Helios"},
            {4 , "Palos"},
            {5 , "Torremar"},
            {6 , "Voight"},
            {7 , "Benson"},
            {8 , "Everett"},
            {9 , "Woodman"},
            {10 , "Miller"},
            {11 , "Ceres"},
            {12 , "Lithcorp"},
            {13 , "Cobalt"},
            {14 , "Mallory"},
            {15 , "Rust Mesa"},
            {16 , "Snowshear"},
            {17 , "Mattherson"},
            {18 , "Waterson"},
            {19 , "Jaeger"},
            {20 , "SolTech"},
            {21 , "DeepCore"},
            {22 , "AuraxiCom"},
            {23 , "Snake Ravine"},
            {24 , "Apex"},
            {25 , "Briggs"},
            {26 , "Morgannis"},
            {27 , "Saerro"},
            {28 , "Vaemar"},
            {29 , "Jagged Lance"},
            {30 , "Alkali"},
            {31 , "Stillwater"},
            {32 , "Black Ridge"}
        };




        WebClient client = new WebClient();
        private JObject GetUserJsonData(string name)
        {
            string path = QueryPath + sid + GetPath + PS2Beta + "character/?name.first_lower=" + name + "&c:show=name,type&c:resolve=world,online_status,outfit&c:has=!deleted";
            lastquery = path;
            string data = client.DownloadString(path);
            JObject o = JObject.Parse(data);


            return o;

        }

        public string QueryStats(IrcCommand command)
        {
            JObject o = GetUserJsonData(command.Parameters[0].ToLower());

            JToken charlist = o["character_list"];
            if (!charlist.HasValues) return "Character Not Found";

            StringBuilder builder = new StringBuilder();



            JToken token = charlist.First["outfit"];

            if (charlist.First["outfit"] != null)
            {
                builder.Append("[");
                builder.Append((string)charlist.First["outfit"]["alias"]);
                builder.Append("] ");
            }
            builder.Append((string)charlist.First["name"]["first"]);
            builder.Append(" is part of ");
            int id = 0;
            bool success = int.TryParse((string)charlist.First["world_id"], out id);
            if (!success)
            {
                return "World ID is not a number.  Character probably banned, deleted, or duplicate name in the api";
            }
            builder.Append(worlds[int.Parse((string)charlist.First["world_id"])]);

            builder.Append("'s ");

            String faction = (string)charlist.First["type"]["faction"];
            switch (faction)
            {
                case "vs":
                    faction = "Vanu Sovereignty";
                    break;
                case "tr":
                    faction = "Terran Republic";
                    break;
                case "nc":
                    faction = "New Conglomerate";
                    break;
            }

            builder.Append(faction);

            //string name = (string)o["character_list"]["name"]["first"];



            return builder.ToString();
        }


    }
}
