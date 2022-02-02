using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using System.Xml;
using SharpServer;
using UniFTP.Server.Performance;
using UniFTP.Server.Virtual;

namespace UniFTP.Server
{
    ///<summary>
    ///Log events
    ///</summary>
    ///<param name="sender" > log entity</param>
    public delegate void LogEventHandler(object sender);

    ///<summary>
    ///FTP server
    ///</summary>
    public class FtpServer : Server<FtpClientConnection>
    {
        private FtpConfig _config = new FtpConfig();
        public FtpConfig Config
        {
            get { return _config; }
            set
            {
                _config = value;
                InitServer(_config != null ? _config.ServerName : "UniFTP");
            }
        }
        ///<summary>
        ///User groups
        ///<para>The key must be lowercase</para>
        ///</summary>
        public Dictionary<string, FtpUserGroup> UserGroups = new Dictionary<string, FtpUserGroup>();
        ///<summary>
        ///user
        ///<para>The key must be lowercase</para>
        ///</summary>
        public Dictionary<string, FtpUser> Users = new Dictionary<string, FtpUser>();
        private DateTime _startTime;
        private Timer _timer;
        public List<FtpConnectionInfo> ConnectionInfos { get; set; }

        internal X509Certificate2 ServerCertificate;
        public ICounter ServerPerformanceCounter { get; set; }

        public event LogEventHandler OnLog;
        public bool Active { get; set; }

        #region Public Methods

        ///<summary>
        ///Forcibly disconnect a Connection
        ///</summary>
        ///< the connection number >param name="id"</param>
        ///<returns></returns>
        public bool Disconnect(string id)
        {
            ulong num;
            if (!ulong.TryParse(id, out num))
            {
                return false;
            }
            var cons = Connections.FindAll(t => t.ID == num);
            if (cons.Count < 1)
            {
                return false;
            }
            else
            {
                try
                {
                    cons.ForEach(t => t.Dispose());
                }
                catch (Exception)
                {
                    return false;
                }

            }
            return true;
        }

        ///<summary>
        ///Load log configuration settings
        ///<para>The configuration should be a string representing XML</para>
        ///</summary>
        ///<param name="xml" > string representing XML, with the xml root element being log4net</param>
        ///<returns></returns>
        public bool LoadLogConfigs(string xml)
        {
            try
            {
                XmlDocument x = new XmlDocument();
                x.LoadXml(xml);

                log4net.Config.XmlConfigurator.Configure(log4net.LogManager.GetRepository(typeof(FtpServer).Assembly), x["log4net"]);
                return true;
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
            return false;
        }

        ///<summary>
        ///Load log configuration settings
        ///<para>The configuration should be a standard log4net XML document</para>
        ///</summary>
        ///<param name="path"></param>
        ///<returns></returns>
        public bool LoadLogConfigsFromFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    log4net.Config.XmlConfigurator.ConfigureAndWatch(log4net.LogManager.GetRepository(typeof(FtpServer).Assembly), new FileInfo(path));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                    //throw;
                }
            }
            return false;
        }

        ///<summary>
        ///Read configuration information
        ///</summary>
        public void LoadConfigs()
        {
            try
            {
                FtpStore.LoadConfig(this);
                if (File.Exists(Config.CertificatePath))    //There are x509 certificates
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

        ///<summary>
        ///Import the X509 certificate
        ///</summary>
        ///<param name="path"></param>
        ///<param name="password"></param>
        ///<returns></returns>
        public bool ImportCertificate(string path, string password)
        {
            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                ServerCertificate = string.IsNullOrEmpty(password) ? new X509Certificate2(path) : new X509Certificate2(path, password);
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

        ///<summary>
        ///Add a group permission rule
        ///</summary>
        ///<param name="groupname" > group name</param>
        ///< virtual path >param name="vPath"</param>
        ///<param name="permission" > permissions, 9-digit UNIX permissions</param>
        ///<returns></returns>
        public bool AddGroupRule(string groupname, string vPath, string permission)
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

        ///<summary>
        ///Delete a user group
        ///<para>only</para>
        ///</summary>
        ///<param name="groupname"></param>
        ///<returns></returns>
        public bool DeleteUserGroup(string groupname)
        {
            if (groupname.ToLower() == "anonymous")
            {
                return false;
            }
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

        public void AddUserGroup(string groupname, AuthType auth, string homeDir = null, bool forbidden = false)
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

        public void AddUser(string username, string password, string groupname, int maxconn = 4096)
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
                        group.Links.Add(path, vParentPath);
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
            : this(IPAddress.Any, 21, false, -1, logHeader)
        {
        }

        public FtpServer(int port, string logHeader = null)
            : this(IPAddress.Any, port, false, -1, logHeader)
        {
        }

        ///<summary>
        ///FTP server
        ///</summary>
        ///<param name="port" > listening port number</param>
        ///<param name="config" > FTP configuration (nullable</param>).
        ///<param name="enable IPv6" ></param>
        ///<param name="ipv6Port" > IPv6 port</param> //MARK: It is not possible to bind a Socket to the same port as IPv4 and IPv6 in Linux
        ///<param name="logHeader" > log header and also used as an instance partition for performance counters, pass in the server name</param>
        public FtpServer(int port, FtpConfig config = null, bool enableIPv6 = false, int ipv6Port = -1, string logHeader = null)
            //: this(IPAddress.Any, port, enableIPv6, ipv6Port, logHeader)
            : base(new[] { new IPEndPoint(IPAddress.Any, port), enableIPv6 ? new IPEndPoint(IPAddress.IPv6Any, (ipv6Port > 0 ? ipv6Port : port)) : null }, logHeader)
        {
            Config = config;
            if (logHeader == null && config != null)
            {
                logHeader = config.ServerName;
            }
            Active = false;
            InitServer(logHeader);
        }

        //public FtpServer(IPAddress ipAddress, int port, string logHeader = null)
        //    : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        //{
        //}

        public FtpServer(IPAddress ipAddress, int port, bool enableIPv6 = false, int ipv6Port = -1, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port), enableIPv6 ? new IPEndPoint(IPAddress.IPv6Any, (ipv6Port > 0 ? ipv6Port : port)) : null }, logHeader)
        {
        }

        public FtpServer(IPEndPoint[] localEndPoints, string logHeader = "UniFTP")
            : base(localEndPoints, logHeader)
        {
            Active = false;
            InitServer(logHeader);
        }

        private void InitServer(string logHeader)
        {
            if (_config == null)
            {
                _config = new FtpConfig();
            }
            if (File.Exists("UniFTP.Server.log4net"))
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(log4net.LogManager.GetRepository(typeof(FtpServer).Assembly), new FileInfo("UniFTP.Server.log4net"));
            }
            UserGroups.Clear();
            UserGroups.Add("anonymous", FtpUserGroup.Anonymous);
            Users.Clear();
            if (Config.AllowAnonymous)
            {
                Users.Add("anonymous", FtpUser.Anonymous);
            }
            if (OnLog == null)
            {
                OnLog += sender => { };
            }
            ConnectionInfos = new List<FtpConnectionInfo>();
            //foreach (var endPoint in localEndPoints)
            //{
            //    //_performanceCounter.Initialize(endPoint.Port);
            //}
            SetCounter(logHeader);
        }

        private void SetCounter(string logHeader = "UniFTP")
        {
            try
            {
                switch (Config.CounterType)
                {
                    case CounterType.System:
                        ServerPerformanceCounter = new SystemCounter(logHeader);
                        break;
                    case CounterType.BuiltIn:
                        ServerPerformanceCounter = new SimpleCounter();
                        break;
                    default:
                        ServerPerformanceCounter = new StubCounter();
                        break;
                }
            }
            catch (Exception)
            {
                throw new Exception("Performance counters failed, usually due to incorrect system configuration. Try the following solution:\nRun the command prompt (cmd) with administrator privileges, enter 'lodct', press Enter, and wait a few moments until you get a success prompt.");
            }
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
            foreach (var tcpListener in base.Listeners)
            {
                FtpLogEntry logEntry = new FtpLogEntry()
                {
                    Date = DateTime.Now,
                    Info = LogInfo.ServerStart.ToString(),
                    SPort = tcpListener.LocalEndpoint.ToString()
                };

                OnLog(logEntry);
            }
            Active = true;
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
            Active = false;
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
