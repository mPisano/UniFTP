using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniFTP.Server;

namespace UniFTPServer
{
    class TabData
    {
        public string LogRecorder { get; private set; }
        public StringBuilder LogBuilder { get; set; }
        public List<ListViewItem> ConnectionList { get; set; }
        public FtpServer Server {
            get { return _server; }
            set
            {
                _server = value;
                _server.OnLog += UpdateLog;
            }
        }
        public TabPage Tab {
            get { return _tab; }
            set {_tab = value;}
        }

        public bool Active {
            get { return Tab != null && IsActive(this); }
        }

        private TabPage _tab;
        private FtpServer _server;
        private StringBuilder _entryBuilder = new StringBuilder();

        public TabData()
        {
            ConnectionList = new List<ListViewItem>();
            LogBuilder = new StringBuilder();
        }

        public static bool IsActive(TabData tab)
        {
            bool active = false;
            Core.TabContainer.Invoke(new MethodInvoker(() => active = Core.TabContainer.SelectedTab == tab.Tab));
            return tab.Tab != null && active;
        }

        public void Sleep()
        {
            LogRecorder = Core.LogTextBox.Text;
        }

        public void WakeUp()
        {
            try
            {
                Core.LastTabData = this;
                Core.LogTextBox.Invoke(new MethodInvoker(() =>
                {
                    Core.LogTextBox.Text = LogRecorder;
                    Core.LogTextBox.AppendText(LogBuilder.ToString());
                }));
                Core.TabContainer.Invoke(new MethodInvoker(() =>
                {
                    Tab.Controls.Clear();
                    Tab.Controls.Add(Core.MainSpiltContainer);
                }));
                LogBuilder.Clear();
                LogRecorder = null;
                UpdateConnectionList();
            }
            catch (Exception)
            {
                return;
                //throw;
            }

        }

        public void UpdateLog(object obj)
        {
            FtpLogEntry entry = obj as FtpLogEntry;
            if (entry == null)
            {
                return;
            }
            _entryBuilder.Append(entry.Date.ToLongTimeString()).Append("\t")
                .Append(entry.CIP??"-").Append("\t")
                .Append(entry.CSUsername ?? "-").Append("\t")
                .Append(entry.CSMethod ?? "-").Append(" ")
                .Append(entry.CSArgs ?? "-").Append("\t")
                .Append(entry.CSBytes ?? "-").Append("\t")
                .Append(entry.SCStatus ?? "-").Append("\t")
                .Append(entry.SCBytes ?? "-").Append("\t")
                .Append(entry.Info ?? "");
            if (Active)
            {
                Core.LogTextBox.Invoke(new MethodInvoker(() =>
                {
                    Core.LogTextBox.AppendText(_entryBuilder.ToString());
                    Core.LogTextBox.AppendText(Environment.NewLine);
                }));

            }
            else
            {
                LogBuilder.Append(_entryBuilder.ToString()).AppendLine();
            }
            _entryBuilder.Clear();
        }

        public void UpdateConnectionList()
        {
            if (Server == null)
            {
                return;
            }
            ConnectionList.Clear();
            Server.ConnectionInfos.ForEach((c) =>
            {
                ListViewItem item = new ListViewItem(new []{c.ID.ToString(),c.User,c.UserGroup,c.IP,c.CurrentPosition,c.CurrentFile,c.LastCommand});
                ConnectionList.Add(item);
            });
        }
    }
}
