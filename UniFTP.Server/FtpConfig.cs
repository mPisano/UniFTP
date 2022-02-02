using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniFTP.Server
{
    public enum CounterType
    {
        None,
        System,
        BuiltIn,
    }
    ///<summary>
    ///FTP configuration
    ///</summary>
    [Serializable]
    public class FtpConfig
    {
        public FtpConfig(string homeDir = null, string name = "UniFTP", bool allowAnonymous = true, string owner = "UniFTP", string ownerGroup = "UniFTP", string[] welcome = null, string[] loginWelcome = null, string[] logoutWelcome = null, CounterType counter = CounterType.BuiltIn)
        {
            HomeDir = homeDir;
            //if (homeDir == null)
            //{
            //    string workDir = Path.Combine(Environment.CurrentDirectory, name, "Root");
            //    Directory.CreateDirectory(workDir);
            //    HomeDir = workDir;
            //}
            ServerName = name;
            AllowAnonymous = allowAnonymous;
            Owner = owner ?? "UniFTP";
            OwnerGroup = ownerGroup ?? "UniFTP";
            Welcome = welcome;
            LogInWelcome = loginWelcome;
            LogOutWelcome = logoutWelcome;
            CounterType = counter;
        }

        ///<summary>
        ///Server name
        ///<para>The name cannot contain path characters</para>
        ///</summary>
        public string ServerName { get; set; }

        ///<summary>
        ///Allow anonymity
        ///</summary>
        public bool AllowAnonymous { get; set; }

        ///<summary>
        ///Server home directory
        ///</summary>
        public string HomeDir { get; set; }

        ///<summary>
        ///The file owner
        ///</summary>
        public string Owner { get; set; }

        ///<summary>
        ///File all groups
        ///</summary>
        public string OwnerGroup { get; set; }

        ///<summary>
        ///Welcome message
        ///</summary>
        public string[] Welcome { get; set; }

        ///<summary>
        ///Welcome message after login
        ///</summary>
        public string[] LogInWelcome { get; set; }

        ///<summary>
        ///Exit prompt
        ///</summary>
        public string[] LogOutWelcome { get; set; }

        ///<summary>
        ///File rules
        ///</summary>
        public Dictionary<string, string> Rules { get; set; }

        ///<summary>
        ///SSL certification path
        ///</summary>
        public string CertificatePath { get; set; }

        ///<summary>
        ///SSL password
        ///</summary>
        internal string CertificatePassword { get; set; }

        ///<summary>
        ///Performance counters
        ///</summary>
        internal CounterType CounterType { get; set; }
    }
}
