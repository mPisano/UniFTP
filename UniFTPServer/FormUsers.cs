﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UniFTP.Server;
using UniFTP.Server.Virtual;
using UniFTPServer;

namespace UniFTPServer
{
    public partial class FormUsers : Form
    {
        public FormUsers()
        {
            InitializeComponent();
        }

        internal static Dictionary<string, FtpUserGroup> Groups;
        internal static Dictionary<string, FtpUser> Users;

        private void FormUser_Load(object sender, EventArgs e)
        {
            Groups = Core.UserGroups;
            Users = Core.Users;
            UpdateGroups();
        }

        private void UpdateGroups(string selected = null)
        {
            listGroups.Items.Clear();
            foreach (var g in Groups)
            {
                listGroups.Items.Add(new ListViewItem(g.Key){Name = g.Key});    //MARK:The Name attribute is the key
            }
            if (listGroups.Items.Count > 0)
            {
                listGroups.Items[0].Selected = true;
            }
            if (!string.IsNullOrEmpty(selected) && listGroups.Items.ContainsKey(selected))
            {
                listGroups.Items[selected].Selected = true;
            }
        }

        private void UpdateUsers(string groupName)
        {
            listUsers.Items.Clear();
            foreach (var u in Users)
            {
                if (u.Value.GroupName == groupName)
                {
                    listUsers.Items.Add(u.Key);
                }
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            uint gnum = 0;
            string name = "NewGroup";
            while (Groups.ContainsKey(name+gnum))
            {
                gnum++;
                if (gnum == uint.MaxValue)
                {
                    return; //MARK:Don't mess around! Denial of Service
                }
            }
            Groups.Add((name + gnum).ToLower(), new FtpUserGroup(name + gnum,AuthType.Password));
            UpdateGroups();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (listGroups.SelectedItems.Count<1)
            {
                return;
            }

            string oldname = listGroups.SelectedItems[0].Text;
            string newname = txtName.Text.ToLower().Trim();
            if (!Groups.ContainsKey(oldname.ToLower()))
            {
                return;
            }
            if (Groups.ContainsKey(newname.ToLower()) && newname != oldname)    //尝试用户组重名
            {
                MessageBox.Show("A user group with the same name already exists!", "Failed to save user group");
                return;
            }
            if (!string.IsNullOrEmpty(txtDir.Text) && !Directory.Exists(txtDir.Text))
            {
                MessageBox.Show("Directory is not legitimate!", "Failed to save user group");
                return;
            }
            bool forbid = false;
            AuthType auth = AuthType.Password;
            if (rdoNone.Checked)
            {
                auth = AuthType.None;
            }
            if (rdoTLS.Checked)
            {
                auth = AuthType.SSL;
            }
            if (rdoNone.Checked)
            {
                forbid = true;
            }
            //FtpUserGroup g = new FtpUserGroup(newname,auth,string.IsNullOrEmpty(txtDir.Text)?null:txtDir.Text)
            //{
            //    Forbidden = forbid
            //};
            Groups[oldname].UserGroupName = newname;
            Groups[oldname].Auth = auth;
            Groups[oldname].HomeDir = txtDir.Text;
            //Groups[oldname].AutoMakeDirectory = chkAutoMakeDir.Checked;
            if (newname == oldname)
            {

            }
            else
            {
                //重命名用户记录
                foreach (var user in Users.Values)
                {
                    if (user.GroupName == oldname.ToLower())
                    {
                        user.GroupName = newname.ToLower();
                    }
                }
                Groups.Add(newname.ToLower(), Groups[oldname.ToLower()]);
                Groups.Remove(oldname.ToLower());
            }
            UpdateGroups(newname);
        }

        private void LoadGroup(string groupName)
        {
            if (!Groups.ContainsKey(groupName))
            {
                return;
            }
            var group = Groups[groupName];
            if (groupName == "anonymous")
            {
                txtName.ReadOnly = true;
            }
            else
            {
                txtName.ReadOnly = false;
            }
            txtName.Text = group.UserGroupName;
            txtDir.Text = group.HomeDir;
            if (group.Auth == AuthType.None)
            {
                rdoNone.Checked = true;
            }
            if (group.Auth == AuthType.Password)
            {
                rdoPwd.Checked = true;
            }
            if (group.Auth == AuthType.SSL)
            {
                rdoTLS.Checked = true;
            }
            if (group.Forbidden)
            {
                rdoForbid.Checked = true;
            }
            //chkAutoMakeDir.Checked = group.AutoMakeDirectory;
            UpdateUsers(groupName);
        }

        private void listGroups_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listGroups.SelectedItems.Count<1)
            {
                return;
            }
            //_currentSelectedName = listGroups.SelectedItems[0].Text;
            LoadGroup(listGroups.SelectedItems[0].Text);
        }

        private void btnLink_Click(object sender, EventArgs e)
        {
            if (listGroups.SelectedItems.Count < 1)
            {
                return;
            }
            FormLinks f = new FormLinks(listGroups.SelectedItems[0].Text);
            f.ShowDialog();
        }

        private void Save()
        {
            FtpStore.SaveAs(Users, "Users.cfg");
            FtpStore.SaveAs(Groups, "UserGroups.cfg");
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            Core.UserGroups = Groups;
            Core.Users = Users;
            Core.UpdateServerUsers();
            Save();
            this.Close();
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (listGroups.SelectedItems.Count < 1)
            {
                return;
            }
            string groupname = listGroups.SelectedItems[0].Text;
            if (groupname.ToLower() == "anonymous")
            {
                MessageBox.Show("Anonymous groups cannot be deleted! Do not need to disable.", "ERROR");
                return;
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
            if (Groups.ContainsKey(groupname.ToLower()))
            {
                Groups.Remove(groupname.ToLower());
            }
            listGroups.Items.Remove(listGroups.SelectedItems[0]);
            return;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "Please select the default home directory for the user group";
            folderBrowserDialog1.ShowDialog();
            txtDir.Text = folderBrowserDialog1.SelectedPath;
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUsers.SelectedItems.Count < 1)
            {
                return;
            }
            Users.Remove(listUsers.SelectedItems[0].Text);
            listUsers.Items.Remove(listUsers.SelectedItems[0]);
        }

        private void btnUser_Click(object sender, EventArgs e)
        {
            if (listGroups.SelectedItems.Count < 1)
            {
                return;
            }
            string groupname = listGroups.SelectedItems[0].Text;
            FormAddUser f = new FormAddUser(groupname);
            f.ShowDialog();
            UpdateUsers(groupname);
        }

        private void menuUser_Opening(object sender, CancelEventArgs e)
        {
            if (listUsers.SelectedItems.Count < 1)
            {
                foreach (ToolStripItem item in menuUser.Items)
                {
                    item.Enabled = false;
                }
            }
            else
            {
                foreach (ToolStripItem item in menuUser.Items)
                {
                    item.Enabled = true;
                }
            }
        }

        private void 修改ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listUsers.SelectedItems.Count < 1 || listGroups.SelectedItems.Count < 1)
            {
                return;
            }
            string groupname = listGroups.SelectedItems[0].Text;
            string username = listUsers.SelectedItems[0].Text;
            FormAddUser f = new FormAddUser(groupname,username);
            f.ShowDialog();
            UpdateUsers(groupname);
        }
    }
}
