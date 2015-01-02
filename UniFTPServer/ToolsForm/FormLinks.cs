using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using UniFTP.Server;
using UniFTP.Server.Virtual;

namespace UniFTPServer
{
    public partial class FormLinks : Form
    {
        public FormLinks(string groupName)
        {
            InitializeComponent();
            _groupName = groupName.ToLower();
        }

        private string _groupName;
        private Dictionary<string, string> _links;
        private Dictionary<string, FilePermission> _rules;
        private FtpUserGroup _group;

        private void btnAddLink_Click(object sender, EventArgs e)
        {
            FormAddLink formAdd = new FormAddLink(_groupName);
            formAdd.ShowDialog();
            UpdateLists();
        }

        private void FormLinks_Load(object sender, EventArgs e)
        {
            UpdateLists();
        }

        private void SavePermission()
        {
            
        }

        private void UpdateLists()
        {
            if (!FormUsers.Groups.ContainsKey(_groupName))
            {
                this.Close();
                return;
            }
            
            _group = FormUsers.Groups[_groupName];
            _links = _group.Links;
            _rules = _group.Rules;
            listLink.Items.Clear();
            foreach (var link in _links)
            {
                listLink.Items.Add(new ListViewItem(new []{link.Key, link.Value}) );
            }
            listRules.Items.Clear();
            foreach (var rule in _rules)
            {
                listRules.Items.Add(new ListViewItem(new[] { rule.Key, GetPermissionString(rule.Value) }));
            }
        }

        private string GetPermissionString(FilePermission p)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(p.CanRead ? "下载 " : "").Append(p.CanWrite ? "上传 " : "").Append(p.GroupCanWrite ? "修改 " : "");
            return sb.ToString();
        }

        private void menuPermission_Opening(object sender, CancelEventArgs e)
        {
            if (listRules.SelectedItems.Count<1)
            {
                foreach (ToolStripItem item in menuPermission.Items)
                {
                    item.Enabled = false;
                }
            }
            else
            {
                foreach (ToolStripItem item in menuPermission.Items)
                {
                    item.Enabled = true;
                }
            }
        }

        private void menuPerDel_Click(object sender, EventArgs e)
        {
            if (listRules.SelectedItems.Count < 1)
            {
                return;
            }
            _rules.Remove(listRules.SelectedItems[0].Text);
            listRules.Items.Remove(listRules.SelectedItems[0]);
        }

        private void menuPerEdit_Click(object sender, EventArgs e)
        {
            if (listRules.SelectedItems.Count < 1)
            {
                return;
            }
            var rule = _rules[listRules.SelectedItems[0].Text];
            FormPermission f = new FormPermission(_groupName,listRules.SelectedItems[0].Text, rule.CanRead,rule.CanWrite,rule.GroupCanWrite);
            f.ShowDialog();
            UpdateLists();
        }

        private void menuLinkDel_Click(object sender, EventArgs e)
        {
            if (listLink.SelectedItems.Count < 1)
            {
                return;
            }
            _links.Remove(listLink.SelectedItems[0].Text);
            listLink.Items.Remove(listLink.SelectedItems[0]);
        }

        private void menuLink_Opening(object sender, CancelEventArgs e)
        {
            if (listLink.SelectedItems.Count < 1)
            {
                foreach (ToolStripItem item in menuLink.Items)
                {
                    item.Enabled = false;
                }
            }
            else
            {
                foreach (ToolStripItem item in menuLink.Items)
                {
                    item.Enabled = true;
                }
            }
        }

        private void menuLinkEdit_Click(object sender, EventArgs e)
        {
            if (listLink.SelectedItems.Count < 1)
            {
                return;
            }
            FormAddLink f = new FormAddLink(_groupName, listLink.SelectedItems[0].SubItems[0].Text,
                listLink.SelectedItems[0].SubItems[1].Text);
            f.ShowDialog();
            UpdateLists();
        }

        private void btnAddRule_Click(object sender, EventArgs e)
        {
            FormPermission f = new FormPermission(_groupName);
            f.ShowDialog();
            UpdateLists();
        }
    }
}
