using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
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
                    lblState.Text = "未启动";
                    lblState.Image = Resources.Stop;
                    ChangeButtonStart(true);
                }
            }
            else
            {
                if (Core.StartServer())
                {
                    lblState.Text = "运行中";
                    lblState.Image = Resources.Start;
                    ChangeButtonStart(false);
                    if (!bgWorkerCounter.IsBusy)
                    {
                        bgWorkerCounter.RunWorkerAsync();
                    }
                }
                else
                {
                    MessageBox.Show("服务器启动失败，可能是端口已经被占用！", "ERROR");
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
            fs.ShowDialog();
        }

        private void toolsStop_Click(object sender, EventArgs e)
        {
            Core.StopServer();
            Core.DelServerTab();
            ChangeButtonStart(true);
            
            if (tabInstance.TabCount>1)
            {
                tabInstance.TabPages.Remove(tabInstance.SelectedTab);
                //if (Environment.OSVersion.Platform == PlatformID.Unix)
                //{
                //    tabInstance.SelectedTab = tabInstance.TabPages[0];
                //    tabInstance_Selected(null,null);
                //}
            }
            else
            {
                if (tabInstance.TabCount>0)
                {
                    tabInstance.SelectedTab = tabInstance.TabPages[0];
                    tabInstance_Selected(null, null);
                    //Core.CurrentTabData = null;
                    //tabInstance.TabPages[0].Text = "未启动";
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

        private int _lastSelection = 0;
        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (listCon.SelectedItems.Count > 0)
            {
                _lastSelection = listCon.SelectedItems[0].Index;
            }
            RefreshConnectionList();
            if (listCon.SelectedItems.Count > _lastSelection + 1)
            {
                listCon.Items[_lastSelection].Selected = true;
            }
            
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
            Core.CurrentTabData = tab;
            tab.WakeUp();
            
            if (tab.Server.Active)
            {
                lblState.Text = "运行中";
                lblState.Image = Resources.Start;
            }
            else
            {
                lblState.Text = "未启动";
                lblState.Image = Resources.Stop;
            }
            ChangeButtonStart(!tab.Server.Active);
        }

        private void 用户ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormUsers f = new FormUsers();
            f.ShowDialog();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void menuCon_Opening(object sender, CancelEventArgs e)
        {
            if (listCon.SelectedItems.Count < 1)
            {
                foreach (ToolStripItem item in menuCon.Items)
                {
                    item.Enabled = false;
                }
            }
            else
            {
                foreach (ToolStripItem item in menuCon.Items)
                {
                    item.Enabled = true;
                }
            }
        }

        private void 断开连接ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listCon.SelectedItems.Count < 1)
            {
                return;
            }
            Core.CurrentTabData.Server.Disconnect(listCon.SelectedItems[0].Text);
        }

        
        private void bgWorkerCounter_DoWork(object sender, DoWorkEventArgs e)
        {
            while (Core.CurrentTabData != null)
            {
                try
                {
                    if (Core.CurrentTabData.Server.ServerPerformanceCounter.Enabled)
                    {
                        this.Invoke(new Action((() =>
                        {
                            lblSpeed.Text = string.Format("上传:{0}/s 下载:{1}/s 当前连接:{2} 用户:{3}/{4}",
                        Util.ByteConvert(
                        Core.CurrentTabData.Server.ServerPerformanceCounter.BytesReceivedPerSec),
                        Util.ByteConvert(
                            Core.CurrentTabData.Server.ServerPerformanceCounter.BytesSentPerSec),
                        Core.CurrentTabData.Server.ServerPerformanceCounter.CurrentConnections,
                        Core.CurrentTabData.Server.ServerPerformanceCounter.CurrentAnonymousUsers,
                        Core.CurrentTabData.Server.ServerPerformanceCounter.CurrentNonAnonymousUsers);
                        })));
                    }
                    else
                    {
                        this.Invoke(new Action((() =>
                        {
                            lblSpeed.Text = "Not Available";
                        })));
                    }
                    //lblSpeed.Text = string.Format("上传:{0}/s 下载:{1}/s 当前连接:{2} 用户:{3}/{4}",
                    //    Util.ByteConvert(
                    //    Core.CurrentTabData.Server.ServerPerformanceCounter.CounterBytesReceivedPerSec.NextValue()),
                    //    Util.ByteConvert(
                    //        Core.CurrentTabData.Server.ServerPerformanceCounter.CounterBytesSentPerSec.NextValue()),
                    //    Core.CurrentTabData.Server.ServerPerformanceCounter.CounterCurrentConnections.RawValue,
                    //    Core.CurrentTabData.Server.ServerPerformanceCounter.CounterCurrentAnonymousUsers.RawValue,
                    //    Core.CurrentTabData.Server.ServerPerformanceCounter.CounterCurrentNonAnonymousUsers.RawValue);

                }
                catch (Exception)
                {
                    //throw;
                }
                Thread.Sleep(1000);
            }
        }

        private void toolServer_Click(object sender, EventArgs e)
        {
            FormServer fs = new FormServer();
            fs.ShowDialog();
        }
    }
}
