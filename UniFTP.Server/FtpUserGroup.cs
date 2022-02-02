using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
    [Serializable]
    public enum AuthType
    {
        None = 1,
        Password = 2,
        MD5 = 4,
        SSL = 8,
        TwoFactor = 16
    }

    [Serializable]
    public class FtpUserGroup
    {
        public static readonly FtpUserGroup Anonymous = new FtpUserGroup("anonymous", AuthType.None);
        public FtpUserGroup(string name, AuthType auth, string dir = null)
        {
            UserGroupName = name;
            Auth = auth;
            HomeDir = dir;
            Rules = new Dictionary<string, FilePermission>();
            Links = new Dictionary<string, string>();
            //AutoMakeDirectory = false;
        }

        ///<summary>
        ///The user group name
        ///</summary>
        public string UserGroupName { get; set; }

        ///<summary>
        ///Home Directory
        ///</summary>
        public string HomeDir { get; set; }

        ///<summary>
        ///disable
        ///</summary>
        public bool Forbidden { get; set; }

        ///<summary>
        ///Authentication method
        ///</summary>
        public AuthType Auth { get; set; }

        ///<summary>
        ///Permission rules
        ///</summary>
        public Dictionary<string, FilePermission> Rules { get; set; }

        /// <summary>
        /// File links
        /// </summary>
        public Dictionary<string, string> Links { get; set; }

        /// <summary>
        /// Timeout
        /// </summary>
        public long TimeOut { get; set; }

        //public bool AutoMakeDirectory { get; set; }
    }
}
