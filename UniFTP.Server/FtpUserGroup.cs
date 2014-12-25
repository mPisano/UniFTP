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
        public static readonly FtpUserGroup Anonymous = new FtpUserGroup("anonymous",AuthType.None);
        public FtpUserGroup(string name, AuthType auth, string dir = null)
        {
            UserGroupName = name;
            Auth = auth;
            HomeDir = dir;
            Rules = new Dictionary<string, FilePermission>();
            Links = new Dictionary<string, string>();
        }

        /// <summary>
        /// 用户组名
        /// </summary>
        public string UserGroupName { get; set; }

        /// <summary>
        /// 主目录
        /// </summary>
        public string HomeDir { get;  set; }

        /// <summary>
        /// 禁用
        /// </summary>
        public bool Forbidden { get; set; }

        /// <summary>
        /// 认证方式
        /// </summary>
        public AuthType Auth  { get; set; }

        /// <summary>
        /// 权限规则
        /// </summary>
        public Dictionary<string, FilePermission> Rules { get; set; }

        /// <summary>
        /// 文件链接
        /// </summary>
        public Dictionary<string, string> Links { get; set; }

        /// <summary>
        /// 超时
        /// </summary>
        public long TimeOut { get; set; }
    }
}
