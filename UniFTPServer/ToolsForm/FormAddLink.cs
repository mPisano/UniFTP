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
    public partial class FormAddLink : Form
    {
        private string _groupName;
        private string _fileName;
        private bool _modify = false;
        private string _oldRealPath = "";
        public FormAddLink(string groupName)
        {
            InitializeComponent();
            _groupName = groupName.ToLower();
        }
        public FormAddLink(string groupName,string realpath,string vpath)
        {
            InitializeComponent();
            _groupName = groupName.ToLower();
            _oldRealPath = realpath;
            _fileName = Path.GetFileName(_oldRealPath); //FIXED:修正直接修改目录权限时的异常
            txtDir.Text = realpath;
            txtVirtual.Text = vpath;
            _modify = true;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "请选择文件";
            fileDialog.AddExtension = true;
            //fileDialog.RestoreDirectory = true;
            fileDialog.Filter = "All files (*.*)|*.*";
            //fileDialog.FilterIndex = 1;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtDir.Text = fileDialog.FileName;
                _fileName = Path.GetFileName(fileDialog.FileName);
            }

        }

        private void btnDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.Description = "请选择要添加的文件夹";
            
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtDir.Text = folderBrowserDialog1.SelectedPath;
                _fileName = Path.GetFileName(folderBrowserDialog1.SelectedPath);
            }

        }

        private void chkDefault_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDefault.Checked)
            {
                chkR.Enabled = false;
                chkW.Enabled = false;
                chkXW.Enabled = false;
            }
            else
            {
                chkR.Enabled = true;
                chkW.Enabled = true;
                chkXW.Enabled = true;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_groupName))
            {
                this.Close();
                return;
            }
            var userGroups = FormUsers.Groups;
            if (Directory.Exists(txtDir.Text) || File.Exists(txtDir.Text))
            {
                if (userGroups.ContainsKey(_groupName.ToLower()))
                {
                    var group = userGroups[_groupName.ToLower()];

                    if (_modify)
                    {
                        group.Links.Remove(_oldRealPath);
                    }

                    if (!group.Links.ContainsKey(txtDir.Text))
                    {
                        group.Links.Add(txtDir.Text, txtVirtual.Text);
                    }
                    else
                    {
                        group.Links[txtDir.Text] = txtVirtual.Text;
                    }

                    if (!chkDefault.Checked)
                    {
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
                        string vdir = VPath.Combine(txtVirtual.Text, _fileName); //BUG:有待考证
                        if (!group.Rules.ContainsKey(vdir))
                        {
                            group.Rules.Add(vdir, f);
                        }
                        else
                        {
                            group.Rules[vdir] = f;
                        }
                    }

                }
                this.Close();
            }
            else
            {
                MessageBox.Show("文件路径有误！", "ERROR");
            }
            return;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormAddLink_Load(object sender, EventArgs e)
        {

        }
    }
}
