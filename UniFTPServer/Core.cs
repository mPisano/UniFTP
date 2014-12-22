using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UniFTP.Server;

namespace UniFTPServer
{
    static class Core
    {
        public static List<FtpServer> Servers = new List<FtpServer>();

        private static FormMain _mainForm;

        public static List<TabData> Tabs = new List<TabData>();
        public static TabData LastTabData;

        public static TabControl TabContainer;
        public static RichTextBox LogTextBox;
        public static SplitContainer MainSpiltContainer;

        private static Dictionary<string, FtpUserGroup> _groups;
        private static Dictionary<string, FtpUser> _users;

        public static void Init(FormMain main)
        {
            _mainForm = main;
            TabContainer = _mainForm.tabInstance;
            LogTextBox = _mainForm.txtLog;
            MainSpiltContainer = _mainForm.splitContainerMain;
            if (File.Exists("UserGroups.cfg"))
            {
                _groups = FtpStore.LoadUserGroups("UserGroups.cfg");
            }

            if (File.Exists("Users.cfg"))
            {
                _users = FtpStore.LoadUsers("Users.cfg");
            }
        }

        public static void AddServerTab(FtpServer server)
        {
            server.UserGroups = _groups ?? server.UserGroups;
            server.Users = _users ?? server.Users;
            TabPage tabPage = new TabPage(server.Config.ServerName);
            tabPage.Controls.Clear();
            tabPage.Controls.AddRange(new Control[] { _mainForm.splitContainerMain });
            var tab = new TabData() {Tab = tabPage,Server = server};
            Tabs.Add(tab);
            _mainForm.tabInstance.Invoke(new MethodInvoker(() =>
            {
                if (_mainForm.tabInstance.TabPages.Count == 1 && Tabs.Count <= 1)
                {
                    _mainForm.tabInstance.TabPages.Clear();
                }
                _mainForm.tabInstance.TabPages.Add(tabPage);
                _mainForm.tabInstance.SelectedTab = tabPage;
            }));
            //_mainForm
        }

        public static void DelServerTab()
        {
            var tab = Tabs.FirstOrDefault(t => t.Active);
            if (tab == null)
            {
                return;
            }
            Tabs.Remove(tab);
        }

        public static bool IsServerRunning()
        {
            var tab = Tabs.FirstOrDefault(t => t.Active);
            if (tab == null)
            {
                return false;
            }
            return tab.Server.Active;
        }

        public static bool StartServer()
        {
            var tab = Tabs.FirstOrDefault(t => t.Active);
            if (tab == null)
            {
                return false;
            }
            try
            {
                tab.Server.Start();
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
            return true;
        }

        public static void StopAll()
        {
            foreach (var tabData in Tabs)
            {
                tabData.Server.Stop();
                tabData.Server.Dispose();
            }
            Tabs = null;
        }

        public static bool StopServer()
        {
            var tab = Tabs.FirstOrDefault(t => t.Active);
            if (tab == null)
            {
                return false;
            }
            try
            {
                tab.Server.Stop();
            }
            catch (Exception)
            {
                return false;
                //throw;
            }
            return true;
        }

        public static TabData GetActiveTabData()
        {
            var tab = Tabs.FirstOrDefault(t => t.Active);
            return tab;
        }
    }
}
