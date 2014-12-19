using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using SharpServer;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
    /// <summary>
    /// 日志事件
    /// </summary>
    /// <param name="sender">日志实体</param>
    public delegate void LogEventHandler(object sender);

    /// <summary>
    /// FTP服务器
    /// </summary>
    public class FtpServer : Server<FtpClientConnection>
    {
        private FtpConfig _config = new FtpConfig();
        public FtpConfig Config {
            get { return _config; }
            set { _config = value; }
        }
        /// <summary>
        /// 用户组
        /// <para>键必须为小写</para>
        /// </summary>
        public Dictionary<string, FtpUserGroup> UserGroups = new Dictionary<string, FtpUserGroup>();
        /// <summary>
        /// 用户
        /// <para>键必须为小写</para>
        /// </summary>
        public Dictionary<string, FtpUser> Users = new Dictionary<string, FtpUser>(); 
        private DateTime _startTime;
        private Timer _timer;
        //private bool _enableIPv6 = false;
        internal X509Certificate2 ServerCertificate;
        public FtpPerformanceCounter ServerPerformanceCounter { get; set; }
        
        public event LogEventHandler OnLog;

        #region Public Methods
        /// <summary>
        /// 读取配置信息
        /// </summary>
        public void LoadConfigs()
        {
            try
            {
                FtpStore.LoadConfig(this);
                if (File.Exists(Config.CertificatePath))    //有X509证书
                {
                    ImportCertificate(Config.CertificatePath, Config.CertificatePassword);
                }
                FtpStore.LoadUserGroups(this);
                FtpStore.LoadUsers(this);
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR when loading configs...");
                //throw;
            }

        }

        /// <summary>
        /// 导入X509证书
        /// </summary>
        /// <param name="path"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ImportCertificate(string path,string password)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                ServerCertificate = string.IsNullOrEmpty(password)?new X509Certificate2(path) : new X509Certificate2(path,password);
                Config.CertificatePath = path;
                Config.CertificatePassword = password;
                return true;
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
        }

        public void SaveConfigs()
        {
            FtpStore.SaveConfig(this);
            FtpStore.SaveUserGroups(this);
            FtpStore.SaveUsers(this);
        }

        public void DeleteGroupRule(string groupname, string vPath)
        {
            if (UserGroups.ContainsKey(groupname.ToLower()))
            {
                var g = UserGroups[groupname.ToLower()];
                string pre = vPath.Trim();
                if (g.Rules.ContainsKey(pre))
                {
                    g.Rules.Remove(pre);
                }
            }
        }

        /// <summary>
        /// 添加组权限规则
        /// </summary>
        /// <param name="groupname">组名</param>
        /// <param name="vPath">虚拟路径</param>
        /// <param name="permission">权限，9位UNIX权限</param>
        /// <returns></returns>
        public bool AddGroupRule(string groupname,string vPath, string permission)
        {
            FilePermission f;
            try
            {
                f = new FilePermission(permission);
            }
            catch (FormatException)
            {
                return false;
            }

            if (!UserGroups.ContainsKey(groupname.ToLower())) return true;
            var g = UserGroups[groupname.ToLower()];
            string pre = vPath.Trim();
            if (g.Rules.ContainsKey(pre))
            {
                g.Rules[pre] = f;
            }
            else
            {
                g.Rules.Add(pre, f);
            }
            return true;
        }

        /// <summary>
        /// 删除用户组
        /// <para>只有</para>
        /// </summary>
        /// <param name="groupname"></param>
        /// <returns></returns>
        public bool DeleteUserGroup(string groupname)
        {
            List<FtpUser> u =
                new List<FtpUser>(
                    Users.Values.Where(
                        user => String.Equals(user.GroupName, groupname, StringComparison.CurrentCultureIgnoreCase)));
            u.ForEach(t => { t.GroupName = "anonymous"; });
            //if (u != null)
            //{
            //    return false;
            //}
            if (UserGroups.ContainsKey(groupname.ToLower()))
            {
                UserGroups.Remove(groupname.ToLower());
            }
            return true;
        }

        public void AddUserGroup(string groupname, AuthType auth,string homeDir = null,bool forbidden = false)
        {
            FtpUserGroup g = new FtpUserGroup(groupname, auth, homeDir);
            g.Forbidden = forbidden;
            
            if (UserGroups.ContainsKey(groupname.ToLower()))
            {
                UserGroups[groupname.ToLower()] = g;
            }
            else
            {
                UserGroups.Add(groupname.ToLower(), g);
            }
            return;
        }

        public void DeleteUser(string username)
        {
            if (Users.ContainsKey(username.ToLower()))
            {
                Users.Remove(username.ToLower());
            }
        }

        public void AddUser(string username, string password, string groupname,int maxconn = 4096)
        {
            FtpUser u;

            if (!UserGroups.ContainsKey(groupname.ToLower()))
            {
                u = new FtpUser(username, "anonymous", maxconn, password);
            }
            else
            {
                u = new FtpUser(username, groupname.ToLower(), maxconn, password, password);
            }

            if (Users.ContainsKey(username.ToLower()))
            {
                Users[username.ToLower()] = u;
            }
            else
            {
                Users.Add(username.ToLower(), u);
            }
            return;
        }

        public void DeleteLink(string groupname, string path)
        {
            path = path.Trim();
            if (UserGroups.ContainsKey(groupname.ToLower()))
            {
                var group = UserGroups[groupname.ToLower()];
                if (group.Links.ContainsKey(path))
                {
                    group.Links.Remove(path);
                }
            }
        }

        public bool AddLink(string groupname, string path, string vParentPath)
        {
            path = path.Trim();
            if (Directory.Exists(path) || File.Exists(path))
            {
                if (UserGroups.ContainsKey(groupname.ToLower()))
                {
                    var group = UserGroups[groupname.ToLower()];

                    if (!group.Links.ContainsKey(path))
                    {
                        group.Links.Add(path,vParentPath);
                    }
                    else
                    {
                        group.Links[path] = vParentPath;
                    }
                    return true;
                }
            }
            return false;
        }

        #endregion

        public FtpServer(string logHeader = null)
            : this(IPAddress.Any, 21, logHeader)
        {
        }

        public FtpServer(int port, string logHeader = null)
            : this(IPAddress.Any, port, logHeader)
        {
        }

        /// <summary>
        /// FTP服务器
        /// </summary>
        /// <param name="port">监听端口号</param>
        /// <param name="config">FTP配置（可为空）</param>
        /// <param name="enableIPv6">启用IPv6</param>
        /// <param name="logHeader">日志头，也用作性能计数器的实例划分，请传入服务器名</param>
        public FtpServer(int port = 21, FtpConfig config = null,bool enableIPv6 = false, string logHeader = null)
            : this(IPAddress.Any, port, enableIPv6, logHeader)
        {
            Config = config;
            if (logHeader == null && config != null)
            {
                logHeader = config.ServerName;
            }
        }

        public FtpServer(IPAddress ipAddress, int port,string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        {
        }

        public FtpServer(IPAddress ipAddress, int port,bool enableIPv6, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port),enableIPv6?new IPEndPoint(IPAddress.IPv6Any,port) : null }, logHeader)
        {
        }

        public FtpServer(IPEndPoint[] localEndPoints,string logHeader = null)
            : base(localEndPoints, logHeader)
        {
            if (File.Exists("UniFTP.Server.log4net"))
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("UniFTP.Server.log4net"));
            }
            UserGroups.Clear();
            UserGroups.Add("anonymous",FtpUserGroup.Anonymous);
            if (Config.AllowAnonymous)
            {
                Users.Add("anonymous", FtpUser.Anonymous);
            }
            OnLog += sender => {};
            //foreach (var endPoint in localEndPoints)
            //{
            //    //_performanceCounter.Initialize(endPoint.Port);
            //}
            if (logHeader == null)
            {
                logHeader = "UniFTP";
            }
            ServerPerformanceCounter = new FtpPerformanceCounter(logHeader);
        }


        protected override void OnConnectAttempt()
        {
            ServerPerformanceCounter.IncrementTotalConnectionAttempts();

            base.OnConnectAttempt();
        }

        protected override void OnStart()
        {
            _startTime = DateTime.Now;

            _timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);

            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);

            _timer.Start();

            FtpLogEntry logEntry = new FtpLogEntry()
            {
                Date = DateTime.Now,
                Info = LogInfo.ServerStart.ToString()
            };
            
            OnLog(logEntry);
        }

        protected override void OnStop()
        {
            if (_timer != null)
                _timer.Stop();

            FtpLogEntry logEntry = new FtpLogEntry()
            {
                Date = DateTime.Now,
                Info = LogInfo.ServerStop.ToString()
            };

            OnLog(logEntry);
        }

        protected override void Dispose(bool disposing)
        {
            PassiveListenerPool.ReleaseAll();

            if (_timer != null)
                _timer.Dispose();

            base.Dispose(disposing);
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ServerPerformanceCounter.SetFtpServiceUptime(DateTime.Now - _startTime);
        }

        internal void SendLog(object sender)
        {
            OnLog(sender);
        }
    }
}
