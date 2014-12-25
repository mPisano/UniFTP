using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UniFTP.Server.Virtual;

namespace UniFTPServer
{
    public partial class FormPermission : Form
    {
        public FormPermission(string groupName)
        {
            InitializeComponent();
            _groupName = groupName;
        }

        public FormPermission(string groupName,string vpath, bool r,bool w,bool xw)
        {
            InitializeComponent();
            _groupName = groupName;
            txtVirtual.Text = vpath;
            _olddir = vpath;
            chkR.Checked = r;
            chkW.Checked = w;
            chkXW.Checked = xw;
            _modify = true;
        }

        private string _olddir = "";
        private bool _modify = false;
        private string _groupName;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_groupName))
            {
                this.Close();
                return;
            }
            if (!FormUsers.Groups.ContainsKey(_groupName))
            {
                this.Close();
                return;
            }
            var group = FormUsers.Groups[_groupName];
            StringBuilder sb = new StringBuilder();
            sb.Append(chkR.Checked ? 'r' : '-')
                .Append(chkW.Checked ? 'w' : '-')
                .Append("xr")
                .Append(chkXW.Checked ? 'w' : '-')
                .Append("xr-x");

            FilePermission f;
            try
            {
                f = new FilePermission(sb.ToString());
            }
            catch (FormatException)
            {
                this.Close();
                return;
            }
            string vdir = txtVirtual.Text;
            if (_modify)
            {
                group.Rules.Remove(_olddir);
            }
            if (!group.Rules.ContainsKey(vdir))
            {
                group.Rules.Add(vdir, f);
            }
            else
            {
                group.Rules[vdir] = f;
            }
            this.Close();
            return;
        }

        private void FormPermission_Load(object sender, EventArgs e)
        {

        }
    }
}
