using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using SharpServer;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
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
        internal Dictionary<string, FtpUserGroup> UserGroups = new Dictionary<string, FtpUserGroup>();
        /// <summary>
        /// 用户
        /// <para>键必须为小写</para>
        /// </summary>
        internal Dictionary<string, FtpUser> Users = new Dictionary<string, FtpUser>(); 
        private DateTime _startTime;
        private Timer _timer;

        /// <summary>
        /// 读取配置信息
        /// </summary>
        public void LoadConfigs()
        {
            FtpStore.LoadConfig(this);
            FtpStore.LoadUserGroups(this);
            FtpStore.LoadUsers(this);
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

        public bool AddUserGroup(string groupname, AuthType auth,string homeDir = null)
        {
            if (UserGroups.ContainsKey(groupname.ToLower()))
            {
                return false;
            }
            FtpUserGroup g = new FtpUserGroup(groupname,auth,homeDir);

            UserGroups.Add(groupname.ToLower(),g);
            return true;
        }

        public bool AddUser(string username, string password, string groupname,int maxconn = 4096)
        {
           
            if (Users.ContainsKey(username.ToLower()))
            {
                return false;
            }

            FtpUser u;

            if (!UserGroups.ContainsKey(groupname.ToLower()))
            {
                u = new FtpUser(username,"anonymous",maxconn,password);
            }
            else
            {
                u = new FtpUser(username,groupname.ToLower(),maxconn,password,password);
            }
            Users.Add(username.ToLower(),u);
            return true;
        }

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
        /// <param name="port"></param>
        /// <param name="config"></param>
        /// <param name="logHeader"></param>
        public FtpServer(int port = 21, FtpConfig config = null, string logHeader = null)
            : this(IPAddress.Any, port, logHeader)
        {
            Config = config;
        }

        public FtpServer(IPAddress ipAddress, int port, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        {
        }

        public FtpServer(IPEndPoint[] localEndPoints, string logHeader = null)
            : base(localEndPoints, logHeader)
        {
            UserGroups.Clear();
            UserGroups.Add("anonymous",FtpUserGroup.Anonymous);
            if (Config.AllowAnonymous)
            {
                Users.Add("anonymous", FtpUser.Anonymous);
            }
            foreach (var endPoint in localEndPoints)
            {
                FtpPerformanceCounters.Initialize(endPoint.Port);
            }
        }

        protected override void OnConnectAttempt()
        {
            FtpPerformanceCounters.IncrementTotalConnectionAttempts();

            base.OnConnectAttempt();
        }

        protected override void OnStart()
        {
            _startTime = DateTime.Now;

            _timer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);

            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);

            _timer.Start();
        }

        protected override void OnStop()
        {
            if (_timer != null)
                _timer.Stop();
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
            FtpPerformanceCounters.SetFtpServiceUptime(DateTime.Now - _startTime);
        }
    }
}
