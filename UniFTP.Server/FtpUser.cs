using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniFTP.Server
{
    [Serializable]
    public class FtpUser
    {
        internal static FtpUser Anonymous = new FtpUser("anonymous");
        public FtpUser(string name, string groupName = null, int conn = 4096, string pass = "", string md5 = "")
        {
            UserName = name;
            MaxConnection = conn;
            Password = pass;
            PasswordMD5 = md5;
            GroupName = groupName ?? "anonymous";
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// MD5
        /// </summary>
        public string PasswordMD5 { get; set; }

        /// <summary>
        /// GroupName
        /// </summary>
        public string GroupName { get; internal set; }

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int MaxConnection { get; set; }

        //[XmlAttribute("twofactorsecret")]
        //public string TwoFactorSecret { get; set; }

        public bool IsAnonymous { get; set; }
        
        public FtpUserGroup UserGroup { get; internal set; }

        public static FtpUser Validate(FtpServer server, string username, string password)
        {
            if (username==null)
            {
                username = "anonymous";
            }
            username = username.ToLower();
            if (password==null)
            {
                password = "";
            }
            if (server.Users.ContainsKey(username))
            {
                var user = server.Users[username];
                if (server.UserGroups.ContainsKey(user.GroupName))
                {
                    var group = server.UserGroups[user.GroupName];
                    user.UserGroup = group;
                    if (group.UserGroupName == "anonymous")
                    {
                        user.IsAnonymous = true;
                    }
                    if (!@group.Forbidden)
                    {
                        switch (@group.Auth)
                        {
                            case AuthType.None:
                                user.IsAnonymous = true;
                                return user;
                                break;
                            case AuthType.TwoFactor:
                            case AuthType.SSL:
                            case AuthType.Password:
                                if (!String.IsNullOrEmpty(user.Password) && user.Password == password)
                                {
                                    return user;
                                }
                                return null;
                                break;
                            case AuthType.MD5:
                                if (!String.IsNullOrEmpty(user.PasswordMD5) && user.PasswordMD5 == password)
                                {
                                    return user;
                                }
                                return null;
                                break;
                        }
                    }
                }
            }
            return null;
        }
    }


    //public static FtpUser Validate(string username, string password, string twoFactorCode)
    //{
    //    FtpUser user = (from u in _users where u.UserName == username && u.Password == password select u).SingleOrDefault();

    //    if (TwoFactor.TimeBasedOneTimePassword.IsValid(user.TwoFactorSecret, twoFactorCode))
    //    {
    //        return user;
    //    }

    //    return null;
    //}
 
}
