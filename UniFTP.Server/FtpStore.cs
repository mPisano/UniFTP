using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{

    public static class FtpStore
    {
        private static BinaryFormatter serializer = new BinaryFormatter();

        public static FtpConfig LoadConfig(string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    return serializer.Deserialize(fs) as FtpConfig ?? null;
                }
            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        public static Dictionary<string, FtpUser> LoadUsers(string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    return serializer.Deserialize(fs) as Dictionary<string, FtpUser> ?? null;
                }
            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        public static Dictionary<string, FtpUserGroup> LoadUserGroups(string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    return serializer.Deserialize(fs) as Dictionary<string, FtpUserGroup> ?? null;
                }
            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        private static bool Load(FtpServer server, string file)
        {
            string sname = VPath.RemoveInvalidPathChars(server.Config.ServerName);
            if (File.Exists(Path.Combine(sname, "Config", file)))
            {
                using (FileStream fs = new FileStream(Path.Combine(sname, "Config", file), FileMode.Open))
                {
                    try
                    {
                        switch (file)
                        {
                            case "users.cfg":
                                server.Users = serializer.Deserialize(fs) as Dictionary<string, FtpUser> ?? server.Users;
                                break;
                            case "usergroups.cfg":
                                server.UserGroups = serializer.Deserialize(fs) as Dictionary<string, FtpUserGroup> ??
                                                    server.UserGroups;
                                break;
                            case "config.cfg":
                                server.Config = serializer.Deserialize(fs) as FtpConfig ?? server.Config;
                                break;
                            default:
                                fs.Close();
                                return false;
                        }
                    }
                    catch (SerializationException)
                    {

                    }
                    //server.Users = serializer.Deserialize(fs) as Dictionary<string, FtpUser> ?? server.Users;
                    fs.Close();
                    return true;
                }
            }
            return false;
        }

        private static bool Save(FtpServer server, string file)
        {
            string sname = VPath.RemoveInvalidPathChars(server.Config.ServerName);
            string dir = Path.Combine(sname, "Config");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream fs = new FileStream(Path.Combine(sname, "Config", file), FileMode.Create))
            {
                switch (file)
                {
                    case "users.cfg":
                        serializer.Serialize(fs, server.Users);
                        break;
                    case "usergroups.cfg":
                        serializer.Serialize(fs, server.UserGroups);
                        break;
                    case "config.cfg":
                        serializer.Serialize(fs, server.Config);
                        break;
                    default:
                        fs.Close();
                        return false;
                }
                //serializer.Serialize(fs, server.Users);
                fs.Close();
            }
            return true;
        }



        public static void Delete(FtpServer server, string file)
        {
            string sname = VPath.RemoveInvalidPathChars(server.Config.ServerName);
            string dir = Path.Combine(sname, "Config");

            if (!Directory.Exists(dir))
            {
                return;
            }
            else
            {
                if (file!=null)
                {
                    File.Delete(Path.Combine(dir, file));
                }
                else
                {
                    File.Delete(Path.Combine(dir, "users.cfg"));
                    File.Delete(Path.Combine(dir, "usergroups.cfg"));
                    File.Delete(Path.Combine(dir, "config.cfg"));
                }
            }
            return;
        }

        public static bool LoadUsers(FtpServer server)
        {
            return Load(server, "users.cfg");
        }

        public static void SaveUsers(FtpServer server)
        {
            Save(server, "users.cfg");
        }

        public static bool LoadUserGroups(FtpServer server)
        {
            return Load(server, "usergroups.cfg");
        }

        public static void SaveUserGroups(FtpServer server)
        {
            Save(server, "usergroups.cfg");
        }

        public static bool LoadConfig(FtpServer server)
        {
            return Load(server, "config.cfg");
        }

        public static void SaveConfig(FtpServer server)
        {
            Save(server, "config.cfg");
        }
    }
}
