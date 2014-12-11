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
        None = 0,
        Password = 1,
        MD5 = 2,
        SSL = 4,
        UniZip = 8
    }

    [Serializable]
    public class FtpUserGroup
    {
        internal static readonly FtpUserGroup Anonymous = new FtpUserGroup("anonymous",AuthType.None);
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
        public string HomeDir { get; internal set; }

        /// <summary>
        /// 禁用
        /// </summary>
        public bool Forbidden { get; set; }

        /// <summary>
        /// 认证方式
        /// </summary>
        public AuthType Auth  { get; set; }

        //BUG:规则如何引入？Dictionary x 2?以/结尾的认定为目录权限？
        //BUG:匿名权限认为是最低权限
        //BUG:root权限限定最高权限

        /// <summary>
        /// 权限规则
        /// </summary>
        public Dictionary<string, FilePermission> Rules { get; set; }

        /// <summary>
        /// 文件链接
        /// </summary>
        public Dictionary<string, string> Links { get; set; }
    }
}
