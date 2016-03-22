namespace UniFTPServer
{
    partial class FormServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnDel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.listServers = new System.Windows.Forms.ListView();
            this.rdoCtrNone = new System.Windows.Forms.RadioButton();
            this.rdoCtrBuiltIn = new System.Windows.Forms.RadioButton();
            this.rdoCtrSystem = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkV6 = new System.Windows.Forms.CheckBox();
            this.txtV6Port = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtCerPwd = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnCerBrowse = new System.Windows.Forms.Button();
            this.txtCer = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkTLS = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnInstance = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtDir = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkAnonymous = new System.Windows.Forms.CheckBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnWelcome = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cboAddress = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.btnNew);
            this.splitContainer1.Panel1.Controls.Add(this.btnDel);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.listServers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.cboAddress);
            this.splitContainer1.Panel2.Controls.Add(this.btnWelcome);
            this.splitContainer1.Panel2.Controls.Add(this.chkAnonymous);
            this.splitContainer1.Panel2.Controls.Add(this.rdoCtrNone);
            this.splitContainer1.Panel2.Controls.Add(this.rdoCtrBuiltIn);
            this.splitContainer1.Panel2.Controls.Add(this.rdoCtrSystem);
            this.splitContainer1.Panel2.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel2.Controls.Add(this.chkV6);
            this.splitContainer1.Panel2.Controls.Add(this.txtV6Port);
            this.splitContainer1.Panel2.Controls.Add(this.label6);
            this.splitContainer1.Panel2.Controls.Add(this.label10);
            this.splitContainer1.Panel2.Controls.Add(this.txtCerPwd);
            this.splitContainer1.Panel2.Controls.Add(this.label9);
            this.splitContainer1.Panel2.Controls.Add(this.btnCerBrowse);
            this.splitContainer1.Panel2.Controls.Add(this.txtCer);
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.btnSave);
            this.splitContainer1.Panel2.Controls.Add(this.btnInstance);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Panel2.Controls.Add(this.label11);
            this.splitContainer1.Panel2.Controls.Add(this.btnBrowse);
            this.splitContainer1.Panel2.Controls.Add(this.txtDir);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.txtPort);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Panel2.Controls.Add(this.txtName);
            this.splitContainer1.Panel2.Controls.Add(this.label2);
            this.splitContainer1.Panel2.Controls.Add(this.chkTLS);
            this.splitContainer1.Size = new System.Drawing.Size(584, 375);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 0;
            // 
            // btnNew
            // 
            this.btnNew.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNew.Location = new System.Drawing.Point(3, 284);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(175, 37);
            this.btnNew.TabIndex = 17;
            this.btnNew.Text = "新建站点";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnDel
            // 
            this.btnDel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDel.Location = new System.Drawing.Point(3, 326);
            this.btnDel.Name = "btnDel";
            this.btnDel.Size = new System.Drawing.Size(175, 37);
            this.btnDel.TabIndex = 2;
            this.btnDel.Text = "删除站点";
            this.btnDel.UseVisualStyleBackColor = true;
            this.btnDel.Click += new System.EventHandler(this.btnDel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "站点列表";
            // 
            // listServers
            // 
            this.listServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listServers.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listServers.Location = new System.Drawing.Point(3, 30);
            this.listServers.MultiSelect = false;
            this.listServers.Name = "listServers";
            this.listServers.Size = new System.Drawing.Size(175, 244);
            this.listServers.TabIndex = 0;
            this.listServers.UseCompatibleStateImageBehavior = false;
            this.listServers.View = System.Windows.Forms.View.List;
            this.listServers.SelectedIndexChanged += new System.EventHandler(this.listServers_SelectedIndexChanged);
            // 
            // rdoCtrNone
            // 
            this.rdoCtrNone.AutoSize = true;
            this.rdoCtrNone.Location = new System.Drawing.Point(204, 260);
            this.rdoCtrNone.Name = "rdoCtrNone";
            this.rdoCtrNone.Size = new System.Drawing.Size(38, 17);
            this.rdoCtrNone.TabIndex = 28;
            this.rdoCtrNone.Text = "无";
            this.rdoCtrNone.UseVisualStyleBackColor = true;
            // 
            // rdoCtrBuiltIn
            // 
            this.rdoCtrBuiltIn.AutoSize = true;
            this.rdoCtrBuiltIn.Location = new System.Drawing.Point(147, 260);
            this.rdoCtrBuiltIn.Name = "rdoCtrBuiltIn";
            this.rdoCtrBuiltIn.Size = new System.Drawing.Size(51, 17);
            this.rdoCtrBuiltIn.TabIndex = 28;
            this.rdoCtrBuiltIn.Text = "内置";
            this.rdoCtrBuiltIn.UseVisualStyleBackColor = true;
            // 
            // rdoCtrSystem
            // 
            this.rdoCtrSystem.AutoSize = true;
            this.rdoCtrSystem.Checked = true;
            this.rdoCtrSystem.Location = new System.Drawing.Point(90, 260);
            this.rdoCtrSystem.Name = "rdoCtrSystem";
            this.rdoCtrSystem.Size = new System.Drawing.Size(51, 17);
            this.rdoCtrSystem.TabIndex = 28;
            this.rdoCtrSystem.TabStop = true;
            this.rdoCtrSystem.Text = "系统";
            this.rdoCtrSystem.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(207, 326);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 37);
            this.btnCancel.TabIndex = 27;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkV6
            // 
            this.chkV6.AutoSize = true;
            this.chkV6.Checked = true;
            this.chkV6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkV6.Location = new System.Drawing.Point(232, 108);
            this.chkV6.Name = "chkV6";
            this.chkV6.Size = new System.Drawing.Size(80, 17);
            this.chkV6.TabIndex = 26;
            this.chkV6.Text = "启用IPv6";
            this.toolTip1.SetToolTip(this.chkV6, "勾选后监听所有IPv6地址");
            this.chkV6.UseVisualStyleBackColor = true;
            this.chkV6.CheckedChanged += new System.EventHandler(this.chkV6_CheckedChanged);
            // 
            // txtV6Port
            // 
            this.txtV6Port.Location = new System.Drawing.Point(95, 106);
            this.txtV6Port.Name = "txtV6Port";
            this.txtV6Port.Size = new System.Drawing.Size(121, 22);
            this.txtV6Port.TabIndex = 25;
            this.txtV6Port.Text = "2121";
            this.toolTip1.SetToolTip(this.txtV6Port, "在Linux下，IPv4与v6不能指定相同的端口");
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 109);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 13);
            this.label10.TabIndex = 24;
            this.label10.Text = "IPv6端口:";
            // 
            // txtCerPwd
            // 
            this.txtCerPwd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCerPwd.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCerPwd.Location = new System.Drawing.Point(95, 224);
            this.txtCerPwd.Name = "txtCerPwd";
            this.txtCerPwd.PasswordChar = '●';
            this.txtCerPwd.Size = new System.Drawing.Size(200, 24);
            this.txtCerPwd.TabIndex = 23;
            this.txtCerPwd.UseSystemPasswordChar = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 228);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(66, 13);
            this.label9.TabIndex = 22;
            this.label9.Text = "证书密码:";
            this.toolTip1.SetToolTip(this.label9, "如果您的证书没有密码，可以不输入");
            // 
            // btnCerBrowse
            // 
            this.btnCerBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCerBrowse.Location = new System.Drawing.Point(312, 224);
            this.btnCerBrowse.Name = "btnCerBrowse";
            this.btnCerBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnCerBrowse.TabIndex = 21;
            this.btnCerBrowse.Text = "浏览";
            this.toolTip1.SetToolTip(this.btnCerBrowse, "浏览以选择证书位置");
            this.btnCerBrowse.UseVisualStyleBackColor = true;
            this.btnCerBrowse.Click += new System.EventHandler(this.btnCerBrowse_Click);
            // 
            // txtCer
            // 
            this.txtCer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCer.Location = new System.Drawing.Point(95, 196);
            this.txtCer.Name = "txtCer";
            this.txtCer.Size = new System.Drawing.Size(292, 22);
            this.txtCer.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 199);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "TLS证书:";
            this.toolTip1.SetToolTip(this.label8, "导入pfx证书文件以启用TLS安全连接");
            // 
            // chkTLS
            // 
            this.chkTLS.AutoSize = true;
            this.chkTLS.Enabled = false;
            this.chkTLS.Location = new System.Drawing.Point(316, 75);
            this.chkTLS.Name = "chkTLS";
            this.chkTLS.Size = new System.Drawing.Size(73, 17);
            this.chkTLS.TabIndex = 18;
            this.chkTLS.Text = "隐式TLS";
            this.chkTLS.UseVisualStyleBackColor = true;
            this.chkTLS.Visible = false;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(8, 326);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 37);
            this.btnSave.TabIndex = 17;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnInstance
            // 
            this.btnInstance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInstance.Location = new System.Drawing.Point(301, 326);
            this.btnInstance.Name = "btnInstance";
            this.btnInstance.Size = new System.Drawing.Size(88, 37);
            this.btnInstance.TabIndex = 16;
            this.btnInstance.Text = "创建";
            this.btnInstance.UseVisualStyleBackColor = true;
            this.btnInstance.Click += new System.EventHandler(this.btnInstance_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 262);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(79, 13);
            this.label11.TabIndex = 14;
            this.label11.Text = "性能计数器:";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(312, 166);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 9;
            this.btnBrowse.Text = "浏览";
            this.toolTip1.SetToolTip(this.btnBrowse, "浏览以选择默认主目录");
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtDir
            // 
            this.txtDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDir.Location = new System.Drawing.Point(95, 138);
            this.txtDir.Name = "txtDir";
            this.txtDir.Size = new System.Drawing.Size(292, 22);
            this.txtDir.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "默认主目录:";
            this.toolTip1.SetToolTip(this.label4, "此目录将作为默认可见的根目录");
            // 
            // chkAnonymous
            // 
            this.chkAnonymous.AutoSize = true;
            this.chkAnonymous.Checked = true;
            this.chkAnonymous.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAnonymous.Location = new System.Drawing.Point(232, 75);
            this.chkAnonymous.Name = "chkAnonymous";
            this.chkAnonymous.Size = new System.Drawing.Size(78, 17);
            this.chkAnonymous.TabIndex = 5;
            this.chkAnonymous.Text = "允许匿名";
            this.toolTip1.SetToolTip(this.chkAnonymous, "勾选后允许匿名登录");
            this.chkAnonymous.UseVisualStyleBackColor = true;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(95, 73);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(121, 22);
            this.txtPort.TabIndex = 3;
            this.txtPort.Text = "21";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "监听端口:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(95, 12);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(292, 22);
            this.txtName.TabIndex = 1;
            this.txtName.Text = "UniFTP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "服务器名:";
            // 
            // btnWelcome
            // 
            this.btnWelcome.Location = new System.Drawing.Point(95, 284);
            this.btnWelcome.Name = "btnWelcome";
            this.btnWelcome.Size = new System.Drawing.Size(121, 23);
            this.btnWelcome.TabIndex = 21;
            this.btnWelcome.Text = "提示语设置";
            this.btnWelcome.UseVisualStyleBackColor = true;
            this.btnWelcome.Click += new System.EventHandler(this.btnWelcome_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 289);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "服务器提示语:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 24;
            this.label6.Text = "监听地址:";
            // 
            // cboAddress
            // 
            this.cboAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAddress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAddress.FormattingEnabled = true;
            this.cboAddress.Location = new System.Drawing.Point(95, 43);
            this.cboAddress.Name = "cboAddress";
            this.cboAddress.Size = new System.Drawing.Size(292, 21);
            this.cboAddress.TabIndex = 29;
            this.toolTip1.SetToolTip(this.cboAddress, "若不选择Any且未勾选“启用IPv6”，则只监听对应的一个地址");
            // 
            // FormServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 375);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "FormServer";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "站点管理器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormServer_FormClosing);
            this.Load += new System.EventHandler(this.FormServer_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnDel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listServers;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkAnonymous;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtDir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnInstance;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnCerBrowse;
        private System.Windows.Forms.TextBox txtCer;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCerPwd;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtV6Port;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkTLS;
        private System.Windows.Forms.CheckBox chkV6;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rdoCtrNone;
        private System.Windows.Forms.RadioButton rdoCtrBuiltIn;
        private System.Windows.Forms.RadioButton rdoCtrSystem;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnWelcome;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboAddress;
        private System.Windows.Forms.Label label6;
    }
}