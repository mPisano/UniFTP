﻿namespace UniFTPServer
{
    partial class FormMain
    {
        ///<summary>
///Required designer variables.
///</summary>
        private System.ComponentModel.IContainer components = null;

        ///<summary>
///Clean up all resources that are in use.
///</summary>
///<param name="disposing" > true if managed resources should be released; false otherwise. </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        ///<summary>
///The designer supports the required method - don't
///Use the Code Editor to modify the contents of this method.
///</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolServer = new System.Windows.Forms.ToolStripButton();
            this.toolStart = new System.Windows.Forms.ToolStripButton();
            this.toolsStop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolClearLog = new System.Windows.Forms.ToolStripButton();
            this.toolRefreshConnect = new System.Windows.Forms.ToolStripButton();
            this.toolAutoRefresh = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MNU_File_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MNU_Site_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.退出ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MNU_Setup_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MNU_Users_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MNU_About_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblState = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.lblSpeed = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabInstance = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.listCon = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuCon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.断开连接ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.bgWorkerCounter = new System.ComponentModel.BackgroundWorker();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabInstance.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.menuCon.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            this.toolStripContainer1.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.menuStrip1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(723, 59);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(723, 59);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            this.toolStripContainer1.TopToolStripPanelVisible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(17, 17);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolServer,
            this.toolStart,
            this.toolsStop,
            this.toolStripSeparator2,
            this.toolClearLog,
            this.toolRefreshConnect,
            this.toolAutoRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(8, 28);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(542, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolServer
            // 
            this.toolServer.Image = ((System.Drawing.Image)(resources.GetObject("toolServer.Image")));
            this.toolServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolServer.Name = "toolServer";
            this.toolServer.Size = new System.Drawing.Size(97, 22);
            this.toolServer.Text = "Site Manager";
            this.toolServer.Click += new System.EventHandler(this.toolServer_Click);
            // 
            // toolStart
            // 
            this.toolStart.Enabled = false;
            this.toolStart.Image = global::UniFTPServer.Properties.Resources.Start;
            this.toolStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStart.Name = "toolStart";
            this.toolStart.Size = new System.Drawing.Size(52, 22);
            this.toolStart.Text = "Start";
            this.toolStart.Click += new System.EventHandler(this.toolStart_Click);
            // 
            // toolsStop
            // 
            this.toolsStop.Image = global::UniFTPServer.Properties.Resources.Stop;
            this.toolsStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolsStop.Name = "toolsStop";
            this.toolsStop.Size = new System.Drawing.Size(52, 22);
            this.toolsStop.Text = "Stop";
            this.toolsStop.Click += new System.EventHandler(this.toolsStop_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolClearLog
            // 
            this.toolClearLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolClearLog.Image = ((System.Drawing.Image)(resources.GetObject("toolClearLog.Image")));
            this.toolClearLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolClearLog.Name = "toolClearLog";
            this.toolClearLog.Size = new System.Drawing.Size(78, 22);
            this.toolClearLog.Text = "Clear the log";
            this.toolClearLog.Click += new System.EventHandler(this.toolClearLog_Click);
            // 
            // toolRefreshConnect
            // 
            this.toolRefreshConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolRefreshConnect.Image = ((System.Drawing.Image)(resources.GetObject("toolRefreshConnect.Image")));
            this.toolRefreshConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRefreshConnect.Name = "toolRefreshConnect";
            this.toolRefreshConnect.Size = new System.Drawing.Size(167, 22);
            this.toolRefreshConnect.Text = "Refresh the connection status";
            this.toolRefreshConnect.Click += new System.EventHandler(this.toolRefreshConnect_Click);
            // 
            // toolAutoRefresh
            // 
            this.toolAutoRefresh.CheckOnClick = true;
            this.toolAutoRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolAutoRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolAutoRefresh.Image")));
            this.toolAutoRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolAutoRefresh.Name = "toolAutoRefresh";
            this.toolAutoRefresh.Size = new System.Drawing.Size(78, 22);
            this.toolAutoRefresh.Text = "Auto-refresh";
            this.toolAutoRefresh.CheckedChanged += new System.EventHandler(this.toolAutoRefresh_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(17, 17);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MNU_File_ToolStripMenuItem,
            this.MNU_Setup_ToolStripMenuItem,
            this.MNU_About_ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(3, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(148, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MNU_File_ToolStripMenuItem
            // 
            this.MNU_File_ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MNU_Site_ToolStripMenuItem,
            this.toolStripSeparator1,
            this.退出ToolStripMenuItem});
            this.MNU_File_ToolStripMenuItem.Name = "MNU_File_ToolStripMenuItem";
            this.MNU_File_ToolStripMenuItem.ShortcutKeyDisplayString = "S";
            this.MNU_File_ToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.MNU_File_ToolStripMenuItem.Text = "File";
            // 
            // MNU_Site_ToolStripMenuItem
            // 
            this.MNU_Site_ToolStripMenuItem.Name = "MNU_Site_ToolStripMenuItem";
            this.MNU_Site_ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.MNU_Site_ToolStripMenuItem.Text = "Site Manager..";
            this.MNU_Site_ToolStripMenuItem.Click += new System.EventHandler(this.站点管理器ToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // 退出ToolStripMenuItem
            // 
            this.退出ToolStripMenuItem.Name = "退出ToolStripMenuItem";
            this.退出ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.退出ToolStripMenuItem.Text = "quit";
            this.退出ToolStripMenuItem.Click += new System.EventHandler(this.退出ToolStripMenuItem_Click);
            // 
            // MNU_Setup_ToolStripMenuItem
            // 
            this.MNU_Setup_ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MNU_Users_ToolStripMenuItem});
            this.MNU_Setup_ToolStripMenuItem.Name = "MNU_Setup_ToolStripMenuItem";
            this.MNU_Setup_ToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.MNU_Setup_ToolStripMenuItem.Text = "Set up";
            // 
            // MNU_Users_ToolStripMenuItem
            // 
            this.MNU_Users_ToolStripMenuItem.Name = "MNU_Users_ToolStripMenuItem";
            this.MNU_Users_ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.MNU_Users_ToolStripMenuItem.Text = "User groups/users";
            this.MNU_Users_ToolStripMenuItem.Click += new System.EventHandler(this.用户ToolStripMenuItem_Click);
            // 
            // MNU_About_ToolStripMenuItem
            // 
            this.MNU_About_ToolStripMenuItem.Name = "MNU_About_ToolStripMenuItem";
            this.MNU_About_ToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.MNU_About_ToolStripMenuItem.Text = "About";
            this.MNU_About_ToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(17, 17);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblState,
            this.toolStripSeparator3,
            this.lblSpeed});
            this.statusStrip1.Location = new System.Drawing.Point(0, 437);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip1.Size = new System.Drawing.Size(723, 23);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblState
            // 
            this.lblState.Image = global::UniFTPServer.Properties.Resources.Stop;
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(61, 18);
            this.lblState.Text = "UniFTP";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            // 
            // lblSpeed
            // 
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(48, 18);
            this.lblSpeed.Text = "Speed";
            // 
            // tabInstance
            // 
            this.tabInstance.Controls.Add(this.tabPage1);
            this.tabInstance.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabInstance.Location = new System.Drawing.Point(0, 59);
            this.tabInstance.Name = "tabInstance";
            this.tabInstance.SelectedIndex = 0;
            this.tabInstance.Size = new System.Drawing.Size(723, 378);
            this.tabInstance.TabIndex = 2;
            this.tabInstance.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabInstance_Selecting);
            this.tabInstance.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabInstance_Selected);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainerMain);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(715, 352);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Not started";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(3, 3);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.txtLog);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.listCon);
            this.splitContainerMain.Size = new System.Drawing.Size(709, 346);
            this.splitContainerMain.SplitterDistance = 187;
            this.splitContainerMain.TabIndex = 0;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.Size = new System.Drawing.Size(709, 187);
            this.txtLog.TabIndex = 0;
            this.txtLog.Text = "";
            this.txtLog.TextChanged += new System.EventHandler(this.txtLog_TextChanged);
            // 
            // listCon
            // 
            this.listCon.AllowColumnReorder = true;
            this.listCon.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader7,
            this.columnHeader6});
            this.listCon.ContextMenuStrip = this.menuCon;
            this.listCon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listCon.FullRowSelect = true;
            this.listCon.HideSelection = false;
            this.listCon.Location = new System.Drawing.Point(0, 0);
            this.listCon.Name = "listCon";
            this.listCon.Size = new System.Drawing.Size(709, 155);
            this.listCon.TabIndex = 0;
            this.listCon.UseCompatibleStateImageBehavior = false;
            this.listCon.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "user";
            this.columnHeader2.Width = 108;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "User groups";
            this.columnHeader3.Width = 92;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "IP";
            this.columnHeader4.Width = 187;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "The current path";
            this.columnHeader5.Width = 192;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "The current file";
            this.columnHeader7.Width = 90;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Recent Actions";
            this.columnHeader6.Width = 93;
            // 
            // menuCon
            // 
            this.menuCon.ImageScalingSize = new System.Drawing.Size(17, 17);
            this.menuCon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.断开连接ToolStripMenuItem});
            this.menuCon.Name = "menuCon";
            this.menuCon.Size = new System.Drawing.Size(134, 26);
            this.menuCon.Opening += new System.ComponentModel.CancelEventHandler(this.menuCon_Opening);
            // 
            // 断开连接ToolStripMenuItem
            // 
            this.断开连接ToolStripMenuItem.Name = "断开连接ToolStripMenuItem";
            this.断开连接ToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.断开连接ToolStripMenuItem.Text = "Disconnect";
            this.断开连接ToolStripMenuItem.Click += new System.EventHandler(this.断开连接ToolStripMenuItem_Click);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Interval = 1500;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // bgWorkerCounter
            // 
            this.bgWorkerCounter.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorkerCounter_DoWork);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 460);
            this.Controls.Add(this.tabInstance);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormMain";
            this.Text = "UniFTP#Server";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabInstance.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.menuCon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MNU_File_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MNU_Site_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MNU_Setup_ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MNU_Users_ToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        public System.Windows.Forms.TabControl tabInstance;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 退出ToolStripMenuItem;
        public System.Windows.Forms.SplitContainer splitContainerMain;
        public System.Windows.Forms.RichTextBox txtLog;
        public System.Windows.Forms.ListView listCon;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ToolStripButton toolsStop;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolClearLog;
        private System.Windows.Forms.ToolStripButton toolRefreshConnect;
        private System.Windows.Forms.ToolStripButton toolAutoRefresh;
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.ToolStripStatusLabel lblState;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripStatusLabel lblSpeed;
        private System.Windows.Forms.ToolStripMenuItem MNU_About_ToolStripMenuItem;
        public System.Windows.Forms.ToolStripButton toolStart;
        private System.Windows.Forms.ContextMenuStrip menuCon;
        private System.Windows.Forms.ToolStripMenuItem 断开连接ToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bgWorkerCounter;
        private System.Windows.Forms.ToolStripButton toolServer;

    }
}

