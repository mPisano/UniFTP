using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UniFTPServer.ToolsForm
{
    internal partial class FormWelcome : Form
    {
        private ServerUnit _unit;
        public string[] LogIn { get; private set; }
        public string[] LogOut { get; private set; }
        public string[] Welcome { get; private set; }

        public FormWelcome(string[] welcome, string[] login, string[] logout)
        {
            Welcome = welcome;
            LogIn = login;
            LogOut = logout;
            txtWelcome.Text = Welcome.ToSingleString();
            txtLogIn.Text = LogIn.ToSingleString();
            txtLogOut.Text = LogOut.ToSingleString();
        }

        public FormWelcome(ServerUnit unit)
        {
            _unit = unit;
            InitializeComponent();
            if (unit != null)
            {
                txtWelcome.Text = _unit.Welcome.ToSingleString();
                txtLogIn.Text = _unit.LogInWelcome.ToSingleString();
                txtLogOut.Text = _unit.LogOutWelcome.ToSingleString();
            }
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            LogIn = txtLogIn.Text.ToArrayStrings();
            LogOut = txtLogOut.Text.ToArrayStrings();
            Welcome = txtWelcome.Text.ToArrayStrings();
            this.DialogResult = DialogResult.OK;
        }

        private void FormWelcome_Load(object sender, EventArgs e)
        {

        }
    }
}
