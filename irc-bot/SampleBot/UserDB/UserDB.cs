using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;

namespace Lebot
{
    [Flags]
    enum Permissions
    {
        None = 0x0,
        RevDB = 0x1,



        Full = 0xFF,

    }

    [Serializable]
    class User
    {
        public string DisplayName;
        public Permissions PermissionSet;
        public string[] hostmasks;
        public byte[] passwordHash;
        public DateTime LastOnline;
        [NonSerialized]
        public bool Online;
    }

    [Serializable]
    struct UserMessage
    {
        public User Recipient;
        public DateTime TimeSent;
        public string Message;
    }


    [Serializable]
    class UserDB
    {
        public static User NoUser = new User()
        {
            DisplayName = "None",
            hostmasks = new string[] { "" },
            Online = false,
        };

        const string Filename = "Userdb.txt";

        
        public List<User> UserList = new List<User>();

        public List<UserMessage> UserMessages = new List<UserMessage>();

        public List<User> SysMessageRecievers = new List<User>();

        public void RegisterUser(string username, string password, string hostmask)
        {
            SHA256 p = SHA256Managed.Create();
            byte[] phash = p.ComputeHash(Encoding.UTF8.GetBytes(password));

            User newUser = new User()
            {
                DisplayName = username,
                PermissionSet = Permissions.None,
                hostmasks = new string[] { hostmask },
                passwordHash = phash,
                LastOnline = DateTime.Now,
                Online = true,
            };

            UserList.Add(newUser);

        }

        public User GetUserByName(string Displayname)
        {
            return UserList.DefaultIfEmpty(null).FirstOrDefault(u => u.DisplayName == Displayname);            
        }

        public User GetUserByNameHost(string name, string hostmask)
        {
            return UserList.DefaultIfEmpty(UserDB.NoUser).FirstOrDefault(u => u.DisplayName == name && u.hostmasks.Contains(hostmask));    
        }

        public bool SendMessage(string username, string message)
        {
            User u = GetUserByName(username);
            if (u.Equals(NoUser)) return false;

            return SendMessage(u, message);
        }

        bool SendMessage(User u, string message)
        {
            UserMessage m = new UserMessage()
            {
                Recipient = u,
                TimeSent = DateTime.Now,
                Message = message,
            };

            UserMessages.Add(m);
            return true;
        }

        public UserMessage[] GetMessagesSinceTime(string username, DateTime time)
        {
            return GetMessagesSinceTime(GetUserByName(username), time);
        }

        public UserMessage[] GetMessagesSinceTime(User user, DateTime time)
        {
            UserMessage[] message = UserMessages.Where((m) => user.Equals(m.Recipient) && m.TimeSent <= time).ToArray();
            UserMessages.RemoveAll(u => message.Contains(u));
            return message;
        }


        public bool UserExists(string username, string hostmask)
        {
            return UserList.Exists(u => u.DisplayName == username || u.hostmasks.Contains(hostmask));
        }


       

        public void ToggleSysMessages(User user)
        {
            if (SysMessageRecievers.Contains(user)) SysMessageRecievers.Remove(user);
            else SysMessageRecievers.Add(user);
        }

        public void Save()
        {
            JsonSerializer ser = new JsonSerializer();
            ser.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            

            using (StreamWriter sw = new StreamWriter(Filename))
            using (JsonWriter wr = new JsonTextWriter(sw))
            {
                wr.Formatting = Formatting.Indented;
                ser.Serialize(wr, this);
            }
        }

        public static UserDB Load()
        {
            if (!File.Exists(Filename))
            {
                UserDB b = new UserDB();
                b.Save();
            }
            UserDB db;
            JsonSerializer ser = new JsonSerializer();
            using (StreamReader r = new StreamReader(Filename))
            using (JsonTextReader reader = new JsonTextReader(r))
            {
                db = ser.Deserialize<UserDB>(reader);
            }
            return db;
        }

        
    }
}
