using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniFTP.Server;

namespace UniFTPServer
{
    public partial class FormAddUser : Form
    {
        private string _groupName = "";
        private string _oldName = "";
        private bool _modify = false;
        private Dictionary<string, FtpUser> _users;
        public FormAddUser(string groupName)
        {
            InitializeComponent();
            _groupName = groupName;
        }
        public FormAddUser(string groupName, string userName)
        {
            InitializeComponent();
            _groupName = groupName;
            _oldName = userName;
            _modify = true;
            txtName.Text = userName;
        }

        private void FormAddUser_Load(object sender, EventArgs e)
        {
            _users = FormUsers.Users;
            cboGroup.Items.Clear();
            foreach (var g in FormUsers.Groups)
            {
                cboGroup.Items.Add(g.Key);
            }
            cboGroup.SelectedItem = _groupName;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string username = txtName.Text.Trim().ToLower();
            //BUG: If change a user's name to another already-exists user's name, setting will be apply to that user.
            //But since username are exclusive and all users can be managed here, currently this bug is ingnored.
            if (_users.ContainsKey(username))
            {
                _users[username].GroupName = cboGroup.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(txtPwd.Text.Trim()))
                {
                    _users[username].Password = txtPwd.Text.Trim();
                }
            }
            else
            {
                //FIXED: BUG: Change Username
                if (!string.IsNullOrEmpty(_oldName) && _users.ContainsKey(_oldName))
                {
                    var user = _users[_oldName];
                    _users.Remove(_oldName);
                    user.UserName = username;
                    _users.Add(username, user);
                }
                else
                {
                    _users.Add(username, new FtpUser(username, cboGroup.SelectedItem.ToString(), pass: txtPwd.Text.Trim()));
                }
            }
            this.Close();

        }
    }
}
