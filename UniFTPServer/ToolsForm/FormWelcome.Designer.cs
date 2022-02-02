namespace UniFTPServer.ToolsForm
{
    partial class FormWelcome
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
            this.txtLogOut = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtLogIn = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtWelcome = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtLogOut
            // 
            this.txtLogOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogOut.Location = new System.Drawing.Point(17, 241);
            this.txtLogOut.Multiline = true;
            this.txtLogOut.Name = "txtLogOut";
            this.txtLogOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogOut.Size = new System.Drawing.Size(457, 72);
            this.txtLogOut.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 225);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "Exit prompt:";
            // 
            // txtLogIn
            // 
            this.txtLogIn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogIn.Location = new System.Drawing.Point(17, 133);
            this.txtLogIn.Multiline = true;
            this.txtLogIn.Name = "txtLogIn";
            this.txtLogIn.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogIn.Size = new System.Drawing.Size(457, 72);
            this.txtLogIn.TabIndex = 19;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 117);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(231, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "User Login Welcome Note: (FTPS mode is not visible)";
            // 
            // txtWelcome
            // 
            this.txtWelcome.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWelcome.Location = new System.Drawing.Point(17, 27);
            this.txtWelcome.Multiline = true;
            this.txtWelcome.Name = "txtWelcome";
            this.txtWelcome.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtWelcome.Size = new System.Drawing.Size(457, 72);
            this.txtWelcome.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(92, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Server Welcome Word:";
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(372, 332);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(102, 23);
            this.btnOk.TabIndex = 22;
            this.btnOk.Text = "Are you sure";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // FormWelcome
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 367);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.txtLogOut);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtLogIn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtWelcome);
            this.Controls.Add(this.label5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormWelcome";
            this.ShowIcon = false;
            this.Text = "Prompt settings";
            this.Load += new System.EventHandler(this.FormWelcome_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLogOut;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtLogIn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtWelcome;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOk;
    }
}