﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using UniFTP.Server;
using UniFTP.Server.Virtual;
using UniFTPServer.Properties;
using UniFTPServer.ToolsForm;

namespace UniFTPServer
{
    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
            _configSerializer = new BinaryFormatter();

        }

        private string[] _welcome;
        private string[] _logIn;
        private string[] _logOut;
        private readonly string _path = "Servers";
        private string _selected = "";
        private Dictionary<string, ServerUnit> _serverUnits = new Dictionary<string, ServerUnit>();
        private bool _modified = false;
        private BinaryFormatter _configSerializer;
        private static readonly ServerUnit DefaultServerUnit = new ServerUnit() { AllowAnonymous = true, Name = "UniFTP Default Site", Port = 21, V6Port = 2121, RootDir = Application.StartupPath };

        private void FormServer_Load(object sender, EventArgs e)
        {
            listServers.Items.Clear();

            if (File.Exists("Servers.cfg"))
            {
                using (FileStream fs = File.Open("Servers.cfg", FileMode.Open))
                {
                    _serverUnits =
                    _configSerializer.Deserialize(fs) as Dictionary<string, ServerUnit> ??
                    _serverUnits;
                }

            }
            if (_serverUnits.Count == 0)
            {
                _serverUnits.Add(DefaultServerUnit.UID, DefaultServerUnit);
            }
            foreach (var serverUnit in _serverUnits)
            {
                listServers.Items.Add(new ListViewItem(new[] { serverUnit.Value.Name, serverUnit.Key }));
            }
            if (listServers.Items.Count > 0)
            {
                listServers.Items[0].Selected = true;
            }
            cboAddress.Items.Clear();
            cboAddress.Items.Add("Any");
            try
            {
                cboAddress.Items.AddRange(Dns.GetHostAddresses(Dns.GetHostName()));
            }
            catch (Exception)
            {
            }
            cboAddress.SelectedItem = "Any";
        }

        private CounterType GetCounterType()
        {
            if (rdoCtrSystem.Checked)
            {
                return CounterType.System;
            }
            else if (rdoCtrBuiltIn.Checked)
            {
                return CounterType.BuiltIn;
            }
            else
            {
                return CounterType.None;
            }
        }

        private void SetCounterType(CounterType type)
        {
            switch (type)
            {
                case CounterType.System:
                    rdoCtrSystem.Checked = true;
                    break;
                case CounterType.BuiltIn:
                    rdoCtrBuiltIn.Checked = true;
                    break;
                case CounterType.None:
                default:
                    rdoCtrNone.Checked = true;
                    break;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_serverUnits.ContainsKey(_selected))
            {
                if (!CheckInput())
                {
                    return;
                }
                UInt16 port = 21;
                UInt16.TryParse(txtPort.Text, out port);
                var s = _serverUnits[_selected];
                s.Name = txtName.Text;
                s.Port = port;
                s.V6Port = UInt16.Parse(txtV6Port.Text);
                s.CertificatePath = txtCer.Text;
                s.AllowAnonymous = chkAnonymous.Checked;
                s.RootDir = txtDir.Text;
                s.Welcome = _welcome;
                s.LogInWelcome = _logIn;
                s.LogOutWelcome = _logOut;
                s.CounterType = GetCounterType();
                s.UseTls = chkTLS.Checked;

                if (listServers.SelectedItems.Count == 1)
                {
                    listServers.SelectedItems[0].Text = s.Name;
                }
                //string sname = VPath.RemoveInvalidPathChars(s.Name);
                //s.UserConfig = Path.Combine(_path, s.UID, sname, "users.cfg");
                //s.UserGroupConfig = Path.Combine(_path, s.UID, sname, "usergroups.cfg");
            }
            _modified = true;
        }

        private void listServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listServers.SelectedItems.Count > 0)
            {
                if (_serverUnits.ContainsKey(listServers.SelectedItems[0].SubItems[1].Text))
                {
                    _selected = listServers.SelectedItems[0].SubItems[1].Text;
                    LoadUnit(_serverUnits[_selected]);
                }
            }
            else
            {
                _selected = "";
            }
        }

        private void LoadUnit(ServerUnit unit)
        {
            txtName.Text = unit.Name;
            txtPort.Text = unit.Port.ToString();
            if (unit.V6Port < 1)
            {
                chkV6.Checked = false;
            }
            else
            {
                chkV6.Checked = true;
                txtV6Port.Text = unit.V6Port.ToString();
            }
            txtDir.Text = unit.RootDir;
            chkV6.Checked = unit.V6Port > 0;
            txtV6Port.Text = unit.V6Port.ToString();
            txtCer.Text = unit.CertificatePath;
            _welcome = unit.Welcome;
            _logIn = unit.LogInWelcome;
            _logOut = unit.LogOutWelcome;
            chkAnonymous.Checked = unit.AllowAnonymous;
            chkTLS.Checked = unit.UseTls;
            SetCounterType(unit.CounterType);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Please select the default home directory for the server";
            folderBrowserDialog1.ShowDialog();
            txtDir.Text = folderBrowserDialog1.SelectedPath;
            _modified = true;
        }

        private bool CheckInput()
        {
            UInt16 tmp;
            bool result = true;
            StringBuilder sb = new StringBuilder();
            if (!UInt16.TryParse(txtPort.Text, out tmp))
            {
                sb.Append("Incorrect port number!").AppendLine();
                result = false;
            }
            if (!UInt16.TryParse(txtV6Port.Text, out tmp))
            {
                sb.Append("Incorrect I pv6 port number! Will try to use the same port as i pv4!").AppendLine();
                if (result)
                {
                    txtV6Port.Text = txtPort.Text;
                }
            }
            if (!File.Exists(txtCer.Text) && !string.IsNullOrEmpty(txtCer.Text))
            {
                sb.Append("The certification path does not exist or is malformed, failed to load the certificate!").AppendLine();
            }
            if (!Directory.Exists(txtDir.Text))
            {
                sb.Append("The default main path does not exist or is malformed!").AppendLine();
            }
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                MessageBox.Show(sb.ToString(), "prompt");
            }
            return result;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _modified = true;
            ServerUnit s = new ServerUnit();
            s.CounterType = (Environment.OSVersion.Platform == PlatformID.Unix
                ? CounterType.BuiltIn
                : CounterType.System);
            _serverUnits.Add(s.UID, s);
            listServers.Items.Add(new ListViewItem(new[] { s.Name, s.UID }));
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (listServers.SelectedItems.Count > 0)
            {
                if (_serverUnits.ContainsKey(_selected))
                {
                    var dr = MessageBox.Show("Do you really want to delete this server?", "warn", MessageBoxButtons.YesNo);
                    if (dr == DialogResult.Yes)
                    {
                        listServers.Items.Remove(listServers.SelectedItems[0]);
                        _serverUnits.Remove(_selected);
                        _modified = true;
                    }
                }
            }
        }

        private void btnCerBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Please select.Pfx certificate file";
            fileDialog.AddExtension = true;
            //fileDialog.RestoreDirectory = true;
            fileDialog.Filter = "pfx files (*.pfx)|*.pfx";
            //fileDialog.FilterIndex = 1;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                txtCer.Text = fileDialog.FileName;
                _modified = true;
            }
        }

        private void btnInstance_Click(object sender, EventArgs e)
        {
            if (!CheckInput())
            {
                MessageBox.Show("Parameter error. Failed to create server！");
                return;
            }
            try
            {
                FtpConfig config = new FtpConfig(txtDir.Text, txtName.Text, chkAnonymous.Checked, "UniFTP", "UniFTP", _welcome, _logIn, _logOut, GetCounterType());
                FtpServer f;
                if (cboAddress.SelectedItem.ToString() != "Any")
                {
                    f = new FtpServer(IPAddress.Parse(cboAddress.SelectedItem.ToString()), int.Parse(txtPort.Text), chkV6.Checked, chkV6.Checked ? int.Parse(txtV6Port.Text) : -1);
                    f.Config = config;
                }
                else
                {
                    f = new FtpServer(int.Parse(txtPort.Text), config, chkV6.Checked, chkV6.Checked ? int.Parse(txtV6Port.Text) : -1);
                }
                f.LoadLogConfigs(string.Format(Resources.LogConfig, Path.Combine(Core.LogDirectory, VPath.RemoveInvalidPathChars(txtName.Text)) + ".log", Path.Combine(Core.LogDirectory, VPath.RemoveInvalidPathChars(txtName.Text)) + ".error.log"));
                if (!string.IsNullOrWhiteSpace(txtCer.Text)) //BUG: logic error
                {
                    if (!f.ImportCertificate(txtCer.Text, txtCerPwd.Text))
                    {
                        var result = MessageBox.Show("Failed to load the TLS certificate, it may be that you entered the wrong password. \nClick 'OK' not to load the certificate (do not enable TLS), or click 'Cancel' to re-enter password","TLS certificate not imported.", MessageBoxButtons.OKCancel);
                        if (result != DialogResult.OK)
                        {
                            return;
                        }
                    }
                }

                Core.AddServerTab(f);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while creating the server.\n" + ex.ToString(), "ERROR");
                //throw;
            }
        }

        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_modified)
            {
                using (FileStream w = File.Open("Servers.cfg", FileMode.Create))
                {
                    _configSerializer.Serialize(w, _serverUnits);
                }
            }
        }

        private void chkV6_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkV6.Checked)
            {
                txtV6Port.Enabled = false;
            }
            else
            {
                txtV6Port.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnWelcome_Click(object sender, EventArgs e)
        {
            FormWelcome welcome;
            if (_serverUnits.ContainsKey(_selected))
            {
                welcome = new FormWelcome(_serverUnits[_selected]);
            }
            else
            {
                welcome = new FormWelcome(_welcome,_logIn,_logOut);
            }
            var dr = welcome.ShowDialog();
            if (dr == DialogResult.OK)
            {
                _logIn = welcome.LogIn;
                _logOut = welcome.LogOut;
                _welcome = welcome.Welcome;
                _modified = true;
            }

        }
    }
}
