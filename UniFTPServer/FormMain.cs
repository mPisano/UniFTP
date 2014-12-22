using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniFTPServer.Properties;

namespace UniFTPServer
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Core.Init(this);
        }

        private void toolStart_Click(object sender, EventArgs e)
        {
            if (Core.IsServerRunning())
            {
                if (Core.StopServer())
                {
                    ChangeButtonStart(true);
                }
            }
            else
            {
                if (Core.StartServer())
                {
                    ChangeButtonStart(false);
                }
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Core.StopAll();
            Application.Exit();
        }

        private void 站点管理器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormServer fs = new FormServer();
            fs.Show();
        }

        private void toolsStop_Click(object sender, EventArgs e)
        {
            Core.StopServer();
            Core.DelServerTab();
            ChangeButtonStart(true);
            
            if (tabInstance.TabCount>1)
            {
                tabInstance.TabPages.Remove(tabInstance.SelectedTab);
            }
            else
            {
                if (tabInstance.TabCount>0)
                {
                    tabInstance.TabPages[0].Text = "未启动";
                }
            }
        }

        private void toolClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
        }

        private void toolRefreshConnect_Click(object sender, EventArgs e)
        {
            RefreshConnectionList();
        }

        private void RefreshConnectionList()
        {
            var tab = Core.GetActiveTabData();
            if (tab == null)
            {
                return;
            }
            tab.UpdateConnectionList();
            listCon.Items.Clear();
            tab.ConnectionList.ForEach(t => listCon.Items.Add(t));
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            RefreshConnectionList();
        }

        private void toolAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            if (toolAutoRefresh.Checked)
            {
                timerRefresh.Start();
            }
            else
            {
                timerRefresh.Stop();
            }
        }

        private void tabInstance_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabInstance_Selecting(object sender, TabControlCancelEventArgs e)
        {

        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            if (txtLog.Focused)
            {
                return;
            }
            txtLog.Select(Math.Max(txtLog.TextLength,0),0);
        }

        private void ChangeButtonStart(bool Start = true)
        {
            if (Start)
            {
                toolStart.Image = Resources.Start;
                toolStart.Text = "启动";
            }
            else
            {
                toolStart.Image = Resources.Pause;
                toolStart.Text = "暂停";
            }
        }

        private void tabInstance_Selected(object sender, TabControlEventArgs e)
        {
            if (Core.LastTabData!=null)
            {
                Core.LastTabData.Sleep();
            }
            var tab = Core.GetActiveTabData();
            if (tab == null)
            {
                return;
            }
            tab.WakeUp();
            ChangeButtonStart(!tab.Server.Active);
        }
    }
}
