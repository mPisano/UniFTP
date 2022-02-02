﻿namespace UniFTPServer
{
    partial class FormLinks
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
            this.listLink = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuLink = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuLinkDel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLinkEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listRules = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuPermission = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuPerDel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPerEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddLink = new System.Windows.Forms.Button();
            this.btnAddRule = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuLink.SuspendLayout();
            this.menuPermission.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listLink
            // 
            this.listLink.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listLink.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listLink.ContextMenuStrip = this.menuLink;
            this.listLink.FullRowSelect = true;
            this.listLink.HideSelection = false;
            this.listLink.Location = new System.Drawing.Point(10, 52);
            this.listLink.MultiSelect = false;
            this.listLink.Name = "listLink";
            this.listLink.Size = new System.Drawing.Size(501, 253);
            this.listLink.TabIndex = 0;
            this.listLink.UseCompatibleStateImageBehavior = false;
            this.listLink.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Real path";
            this.columnHeader1.Width = 277;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "The virtual path parent directory";
            this.columnHeader2.Width = 293;
            // 
            // menuLink
            // 
            this.menuLink.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLinkDel,
            this.menuLinkEdit});
            this.menuLink.Name = "menuLink";
            this.menuLink.Size = new System.Drawing.Size(108, 48);
            this.menuLink.Opening += new System.ComponentModel.CancelEventHandler(this.menuLink_Opening);
            // 
            // menuLinkDel
            // 
            this.menuLinkDel.Name = "menuLinkDel";
            this.menuLinkDel.Size = new System.Drawing.Size(107, 22);
            this.menuLinkDel.Text = "Delete";
            this.menuLinkDel.Click += new System.EventHandler(this.menuLinkDel_Click);
            // 
            // menuLinkEdit
            // 
            this.menuLinkEdit.Name = "menuLinkEdit";
            this.menuLinkEdit.Size = new System.Drawing.Size(107, 22);
            this.menuLinkEdit.Text = "revise";
            this.menuLinkEdit.Click += new System.EventHandler(this.menuLinkEdit_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.18868F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Link";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(10, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(351, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "You can map multiple files/folders to virtual directories.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(10, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(344, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Permissions for different files/folders can be set here.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.18868F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(10, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "Permissions";
            // 
            // listRules
            // 
            this.listRules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listRules.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.listRules.ContextMenuStrip = this.menuPermission;
            this.listRules.HideSelection = false;
            this.listRules.Location = new System.Drawing.Point(10, 364);
            this.listRules.Name = "listRules";
            this.listRules.Size = new System.Drawing.Size(501, 254);
            this.listRules.TabIndex = 5;
            this.listRules.UseCompatibleStateImageBehavior = false;
            this.listRules.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Virtual path";
            this.columnHeader3.Width = 444;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Permissions";
            this.columnHeader4.Width = 132;
            // 
            // menuPermission
            // 
            this.menuPermission.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuPerDel,
            this.menuPerEdit});
            this.menuPermission.Name = "menuPermission";
            this.menuPermission.Size = new System.Drawing.Size(108, 48);
            this.menuPermission.Opening += new System.ComponentModel.CancelEventHandler(this.menuPermission_Opening);
            // 
            // menuPerDel
            // 
            this.menuPerDel.Name = "menuPerDel";
            this.menuPerDel.Size = new System.Drawing.Size(107, 22);
            this.menuPerDel.Text = "Delete";
            this.menuPerDel.Click += new System.EventHandler(this.menuPerDel_Click);
            // 
            // menuPerEdit
            // 
            this.menuPerEdit.Name = "menuPerEdit";
            this.menuPerEdit.Size = new System.Drawing.Size(107, 22);
            this.menuPerEdit.Text = "revise";
            this.menuPerEdit.Click += new System.EventHandler(this.menuPerEdit_Click);
            // 
            // btnAddLink
            // 
            this.btnAddLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddLink.Location = new System.Drawing.Point(423, 21);
            this.btnAddLink.Name = "btnAddLink";
            this.btnAddLink.Size = new System.Drawing.Size(88, 23);
            this.btnAddLink.TabIndex = 6;
            this.btnAddLink.Text = "Add Link";
            this.btnAddLink.UseVisualStyleBackColor = true;
            this.btnAddLink.Click += new System.EventHandler(this.btnAddLink_Click);
            // 
            // btnAddRule
            // 
            this.btnAddRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddRule.Location = new System.Drawing.Point(423, 16);
            this.btnAddRule.Name = "btnAddRule";
            this.btnAddRule.Size = new System.Drawing.Size(88, 23);
            this.btnAddRule.TabIndex = 7;
            this.btnAddRule.Text = "Add Permission";
            this.btnAddRule.UseVisualStyleBackColor = true;
            this.btnAddRule.Click += new System.EventHandler(this.btnAddRule_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnAddRule);
            this.splitContainer1.Panel2.Controls.Add(this.label4);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(521, 630);
            this.splitContainer1.SplitterDistance = 315;
            this.splitContainer1.TabIndex = 8;
            // 
            // FormLinks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 630);
            this.Controls.Add(this.btnAddLink);
            this.Controls.Add(this.listRules);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listLink);
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormLinks";
            this.ShowIcon = false;
            this.Text = "Directory Settings";
            this.Load += new System.EventHandler(this.FormLinks_Load);
            this.menuLink.ResumeLayout(false);
            this.menuPermission.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listLink;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listRules;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button btnAddLink;
        private System.Windows.Forms.Button btnAddRule;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ContextMenuStrip menuPermission;
        private System.Windows.Forms.ToolStripMenuItem menuPerDel;
        private System.Windows.Forms.ToolStripMenuItem menuPerEdit;
        private System.Windows.Forms.ContextMenuStrip menuLink;
        private System.Windows.Forms.ToolStripMenuItem menuLinkDel;
        private System.Windows.Forms.ToolStripMenuItem menuLinkEdit;
    }
}