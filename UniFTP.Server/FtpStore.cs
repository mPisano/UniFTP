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
        private static readonly BinaryFormatter serializer = new BinaryFormatter();

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

        ///<summary>
        ///Save as
        ///</summary>
        ///<param name="obj"></param>
        ///<param name="file"></param>
        ///<returns></returns>
        public static bool SaveAs(object obj, string file)
        {
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Create))
                {
                    serializer.Serialize(fs, obj);
                    fs.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
        }

        private static bool Load(FtpServer server, string file, string directory = null)
        {
            string sname = VPath.RemoveInvalidPathChars(server.Config.ServerName);
            string p = directory == null ? Path.Combine(sname, "Config", file) : Path.Combine(VPath.RemoveInvalidPathChars(directory), sname, "Config", file);
            if (File.Exists(p))
            {
                using (FileStream fs = new FileStream(p, FileMode.Open))
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

        private static bool Save(FtpServer server, string file, string directory = null)
        {
            string sname = VPath.RemoveInvalidPathChars(server.Config.ServerName);
            string dir = directory == null ? Path.Combine(sname, "Config") : Path.Combine(VPath.RemoveInvalidPathChars(directory), sname, "Config");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream fs = new FileStream(Path.Combine(dir, file), FileMode.Create))
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



        public static void Delete(FtpServer server, string file = null, string directory = null)
        {
            string sname = VPath.RemoveInvalidPathChars(server.Config.ServerName);
            //string dir = Path.Combine(sname, "Config");
            string dir = directory == null ? Path.Combine(sname, "Config") : Path.Combine(VPath.RemoveInvalidPathChars(directory), sname, "Config");
            if (!Directory.Exists(dir))
            {
                return;
            }
            else
            {
                if (file != null)
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

        public static bool LoadUsers(FtpServer server, string dir = null)
        {
            return Load(server, "users.cfg", dir);
        }

        public static void SaveUsers(FtpServer server, string dir = null)
        {
            Save(server, "users.cfg", dir);
        }

        public static bool LoadUserGroups(FtpServer server, string dir = null)
        {
            return Load(server, "usergroups.cfg", dir);
        }

        public static void SaveUserGroups(FtpServer server, string dir = null)
        {
            Save(server, "usergroups.cfg", dir);
        }

        public static bool LoadConfig(FtpServer server, string dir = null)
        {
            return Load(server, "config.cfg", dir);
        }

        public static void SaveConfig(FtpServer server, string dir = null)
        {
            Save(server, "config.cfg", dir);
        }
    }
}
