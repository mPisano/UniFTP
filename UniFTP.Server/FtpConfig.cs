using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniFTP.Server
{
    /// <summary>
    /// FTP配置
    /// </summary>
    [Serializable]
    public class FtpConfig
    {
        public FtpConfig(string homeDir = null, string name = "UniFTP", bool allowAnonymous = true, string owner = "UniFTP",string ownerGroup="UniFTP",string[] welcome = null,string[] loginWelcome = null,string[] logoutWelcome = null)
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
            Owner = owner??"UniFTP";
            OwnerGroup = ownerGroup ?? "UniFTP";
            Welcome = welcome;
            LogInWelcome = loginWelcome;
            LogOutWelcome = logoutWelcome;
        }

        /// <summary>
        /// 服务器名
        /// <para>名称不能包含路径字符</para>
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 允许匿名
        /// </summary>
        public bool AllowAnonymous { get; set; }

        /// <summary>
        /// 服务器主目录
        /// </summary>
        public string HomeDir { get; set; }

        /// <summary>
        /// 文件所有者
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// 文件所有组
        /// </summary>
        public string OwnerGroup { get; set; }

        /// <summary>
        /// 欢迎语
        /// </summary>
        public string[] Welcome { get; set; }

        /// <summary>
        /// 登录后欢迎语
        /// </summary>
        public string[] LogInWelcome { get; set; }

        /// <summary>
        /// 退出提示语
        /// </summary>
        public string[] LogOutWelcome { get; set; }

        /// <summary>
        /// 文件规则
        /// </summary>
        public Dictionary<string, string> Rules { get; set; }

        /// <summary>
        /// SSL证书路径
        /// </summary>
        public string CertificatePath { get; set; }

        /// <summary>
        /// SSL密码
        /// </summary>
        internal string CertificatePassword { get; set; }
    }
}
