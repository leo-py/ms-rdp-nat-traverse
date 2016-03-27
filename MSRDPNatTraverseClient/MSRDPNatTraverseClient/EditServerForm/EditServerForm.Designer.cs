namespace MSRDPNatTraverseClient
{
    partial class EditServerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditServerForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.serversListBox = new System.Windows.Forms.ListBox();
            this.removeServerButton = new System.Windows.Forms.Button();
            this.addServerButton = new System.Windows.Forms.Button();
            this.groupBoxInfo = new System.Windows.Forms.GroupBox();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.serverDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.serverPasswordTextBox = new System.Windows.Forms.TextBox();
            this.serverUserNameTextBox = new System.Windows.Forms.TextBox();
            this.serverIPTextBox = new System.Windows.Forms.TextBox();
            this.serverNameTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBoxInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.serversListBox);
            this.groupBox1.Controls.Add(this.removeServerButton);
            this.groupBox1.Controls.Add(this.addServerButton);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 298);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "代理服务器列表";
            // 
            // serversListBox
            // 
            this.serversListBox.FormattingEnabled = true;
            this.serversListBox.ItemHeight = 17;
            this.serversListBox.Location = new System.Drawing.Point(7, 23);
            this.serversListBox.Name = "serversListBox";
            this.serversListBox.Size = new System.Drawing.Size(247, 242);
            this.serversListBox.TabIndex = 0;
            this.serversListBox.SelectedIndexChanged += new System.EventHandler(this.serversListBox_SelectedIndexChanged);
            // 
            // removeServerButton
            // 
            this.removeServerButton.Location = new System.Drawing.Point(179, 269);
            this.removeServerButton.Name = "removeServerButton";
            this.removeServerButton.Size = new System.Drawing.Size(75, 23);
            this.removeServerButton.TabIndex = 2;
            this.removeServerButton.Text = "删除 -";
            this.removeServerButton.UseVisualStyleBackColor = true;
            this.removeServerButton.Click += new System.EventHandler(this.removeServerButton_Click);
            // 
            // addServerButton
            // 
            this.addServerButton.Location = new System.Drawing.Point(6, 269);
            this.addServerButton.Name = "addServerButton";
            this.addServerButton.Size = new System.Drawing.Size(75, 23);
            this.addServerButton.TabIndex = 1;
            this.addServerButton.Text = "添加 +";
            this.addServerButton.UseVisualStyleBackColor = true;
            this.addServerButton.Click += new System.EventHandler(this.addServerButton_Click);
            // 
            // groupBoxInfo
            // 
            this.groupBoxInfo.Controls.Add(this.portTextBox);
            this.groupBoxInfo.Controls.Add(this.serverDescriptionTextBox);
            this.groupBoxInfo.Controls.Add(this.label5);
            this.groupBoxInfo.Controls.Add(this.serverPasswordTextBox);
            this.groupBoxInfo.Controls.Add(this.serverUserNameTextBox);
            this.groupBoxInfo.Controls.Add(this.serverIPTextBox);
            this.groupBoxInfo.Controls.Add(this.serverNameTextBox);
            this.groupBoxInfo.Controls.Add(this.label4);
            this.groupBoxInfo.Controls.Add(this.label3);
            this.groupBoxInfo.Controls.Add(this.label2);
            this.groupBoxInfo.Controls.Add(this.label1);
            this.groupBoxInfo.Enabled = false;
            this.groupBoxInfo.Location = new System.Drawing.Point(294, 13);
            this.groupBoxInfo.Name = "groupBoxInfo";
            this.groupBoxInfo.Size = new System.Drawing.Size(260, 298);
            this.groupBoxInfo.TabIndex = 1;
            this.groupBoxInfo.TabStop = false;
            this.groupBoxInfo.Text = "代理服务器信息";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(202, 49);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(51, 23);
            this.portTextBox.TabIndex = 5;
            this.portTextBox.Tag = "port";
            this.portTextBox.Text = "22";
            this.portTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.portTextBox.TextChanged += new System.EventHandler(this.serverInfoTextBox_TextChanged);
            // 
            // serverDescriptionTextBox
            // 
            this.serverDescriptionTextBox.Location = new System.Drawing.Point(9, 160);
            this.serverDescriptionTextBox.Multiline = true;
            this.serverDescriptionTextBox.Name = "serverDescriptionTextBox";
            this.serverDescriptionTextBox.Size = new System.Drawing.Size(245, 132);
            this.serverDescriptionTextBox.TabIndex = 8;
            this.serverDescriptionTextBox.Tag = "desc";
            this.serverDescriptionTextBox.Text = "示例：这是一台用于远程控制的代理服务器。";
            this.serverDescriptionTextBox.Click += new System.EventHandler(this.TextBox_Click);
            this.serverDescriptionTextBox.TextChanged += new System.EventHandler(this.serverInfoTextBox_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "描述";
            // 
            // serverPasswordTextBox
            // 
            this.serverPasswordTextBox.Location = new System.Drawing.Point(80, 107);
            this.serverPasswordTextBox.Name = "serverPasswordTextBox";
            this.serverPasswordTextBox.PasswordChar = '*';
            this.serverPasswordTextBox.Size = new System.Drawing.Size(174, 23);
            this.serverPasswordTextBox.TabIndex = 7;
            this.serverPasswordTextBox.Tag = "password";
            this.serverPasswordTextBox.Text = "password";
            this.serverPasswordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.serverPasswordTextBox.Click += new System.EventHandler(this.TextBox_Click);
            this.serverPasswordTextBox.TextChanged += new System.EventHandler(this.serverInfoTextBox_TextChanged);
            // 
            // serverUserNameTextBox
            // 
            this.serverUserNameTextBox.Location = new System.Drawing.Point(80, 78);
            this.serverUserNameTextBox.Name = "serverUserNameTextBox";
            this.serverUserNameTextBox.Size = new System.Drawing.Size(174, 23);
            this.serverUserNameTextBox.TabIndex = 6;
            this.serverUserNameTextBox.Tag = "loginName";
            this.serverUserNameTextBox.Text = "root";
            this.serverUserNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.serverUserNameTextBox.Click += new System.EventHandler(this.TextBox_Click);
            this.serverUserNameTextBox.TextChanged += new System.EventHandler(this.serverInfoTextBox_TextChanged);
            // 
            // serverIPTextBox
            // 
            this.serverIPTextBox.Location = new System.Drawing.Point(80, 49);
            this.serverIPTextBox.Name = "serverIPTextBox";
            this.serverIPTextBox.Size = new System.Drawing.Size(116, 23);
            this.serverIPTextBox.TabIndex = 4;
            this.serverIPTextBox.Tag = "ip";
            this.serverIPTextBox.Text = "example.com";
            this.serverIPTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.serverIPTextBox.Click += new System.EventHandler(this.TextBox_Click);
            this.serverIPTextBox.TextChanged += new System.EventHandler(this.serverInfoTextBox_TextChanged);
            // 
            // serverNameTextBox
            // 
            this.serverNameTextBox.Location = new System.Drawing.Point(80, 20);
            this.serverNameTextBox.Name = "serverNameTextBox";
            this.serverNameTextBox.Size = new System.Drawing.Size(174, 23);
            this.serverNameTextBox.TabIndex = 3;
            this.serverNameTextBox.Tag = "name";
            this.serverNameTextBox.Text = "代理服务器";
            this.serverNameTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.serverNameTextBox.Click += new System.EventHandler(this.TextBox_Click);
            this.serverNameTextBox.TextChanged += new System.EventHandler(this.serverInfoTextBox_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "密码";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "登录名";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "服务器地址";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称";
            // 
            // EditServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 325);
            this.Controls.Add(this.groupBoxInfo);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditServerForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "编辑服务器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditServerForm_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBoxInfo.ResumeLayout(false);
            this.groupBoxInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox serversListBox;
        private System.Windows.Forms.Button removeServerButton;
        private System.Windows.Forms.Button addServerButton;
        private System.Windows.Forms.GroupBox groupBoxInfo;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox serverPasswordTextBox;
        private System.Windows.Forms.TextBox serverUserNameTextBox;
        private System.Windows.Forms.TextBox serverIPTextBox;
        private System.Windows.Forms.TextBox serverNameTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox serverDescriptionTextBox;
        private System.Windows.Forms.TextBox portTextBox;
    }
}