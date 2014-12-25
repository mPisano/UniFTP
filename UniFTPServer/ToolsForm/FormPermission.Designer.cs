namespace UniFTPServer
{
    partial class FormPermission
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
            this.btnAdd = new System.Windows.Forms.Button();
            this.chkXW = new System.Windows.Forms.CheckBox();
            this.chkW = new System.Windows.Forms.CheckBox();
            this.chkR = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtVirtual = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(321, 92);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(103, 34);
            this.btnAdd.TabIndex = 26;
            this.btnAdd.Text = "确定";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // chkXW
            // 
            this.chkXW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkXW.AutoSize = true;
            this.chkXW.Location = new System.Drawing.Point(149, 102);
            this.chkXW.Name = "chkXW";
            this.chkXW.Size = new System.Drawing.Size(103, 18);
            this.chkXW.TabIndex = 25;
            this.chkXW.Text = "可修改/删除";
            this.chkXW.UseVisualStyleBackColor = true;
            // 
            // chkW
            // 
            this.chkW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkW.AutoSize = true;
            this.chkW.Location = new System.Drawing.Point(75, 102);
            this.chkW.Name = "chkW";
            this.chkW.Size = new System.Drawing.Size(68, 18);
            this.chkW.TabIndex = 24;
            this.chkW.Text = "可上传";
            this.chkW.UseVisualStyleBackColor = true;
            // 
            // chkR
            // 
            this.chkR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkR.AutoSize = true;
            this.chkR.Location = new System.Drawing.Point(15, 102);
            this.chkR.Name = "chkR";
            this.chkR.Size = new System.Drawing.Size(54, 18);
            this.chkR.TabIndex = 23;
            this.chkR.Text = "可读";
            this.chkR.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10.18868F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(12, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 16);
            this.label3.TabIndex = 22;
            this.label3.Text = "权限";
            // 
            // txtVirtual
            // 
            this.txtVirtual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVirtual.Font = new System.Drawing.Font("宋体", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtVirtual.Location = new System.Drawing.Point(12, 37);
            this.txtVirtual.Name = "txtVirtual";
            this.txtVirtual.Size = new System.Drawing.Size(412, 24);
            this.txtVirtual.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10.18868F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 16);
            this.label2.TabIndex = 20;
            this.label2.Text = "虚拟路径";
            // 
            // FormPermission
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 132);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.chkXW);
            this.Controls.Add(this.chkW);
            this.Controls.Add(this.chkR);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtVirtual);
            this.Controls.Add(this.label2);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(452, 171);
            this.Name = "FormPermission";
            this.ShowIcon = false;
            this.Text = "文件权限设置";
            this.Load += new System.EventHandler(this.FormPermission_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.CheckBox chkXW;
        private System.Windows.Forms.CheckBox chkW;
        private System.Windows.Forms.CheckBox chkR;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtVirtual;
        private System.Windows.Forms.Label label2;

    }
}