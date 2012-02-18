//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//
//    Please look in the accompanying license.htm file for the license that 
//    applies to this source code. (a copy can also be found at: 
//    http://nzb.wordtgek.nl/license.htm)
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    frmServer.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NZB_O_Matic
{
	public struct ServerValues
	{
		public int Group;
		public string Host;
		public int Port;
		public int Connections;
		public bool Login;
		public string User;
		public string Password;
		public bool NeedsGroup;
        public bool UseSSL;

		public ServerValues(int setGroup, string setHost, int setPort, int setConnections, bool setLogin, string setUser, string setPassword, bool setNeedsGroup, bool setUseSSL)
		{
			Group = setGroup;
			Host = setHost;
			Port = setPort;
			Connections = setConnections;
			Login = setLogin;
			User = setUser;
			Password = setPassword;
			NeedsGroup = setNeedsGroup;
            UseSSL = setUseSSL;
		}
	}

	/// <summary>
	/// Summary description for frmServer.
	/// </summary>
	public class Form_Server : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label Label_Address;
		private System.Windows.Forms.Label Label_Port;
		private System.Windows.Forms.Label Label_Connections;
		private System.Windows.Forms.Label Label_UseLogin;
		private System.Windows.Forms.Label Label_Login;
		private System.Windows.Forms.Label Label_Password;
		private System.Windows.Forms.Label Label_Group;
		private System.Windows.Forms.TextBox Box_Address;
		private System.Windows.Forms.NumericUpDown Number_Connections;
		private System.Windows.Forms.CheckBox Check_Login;
		private System.Windows.Forms.TextBox Text_Login;
		private System.Windows.Forms.TextBox Text_Password;
		private System.Windows.Forms.Button Button_OK;
		private System.Windows.Forms.Button Button_Cancel;
		private System.Windows.Forms.NumericUpDown Number_Group;
		private System.Windows.Forms.NumericUpDown Number_Port;
		private System.Windows.Forms.CheckBox cbNeedsGroup;
		private System.Windows.Forms.Label Label_NeedsGroup;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label Label_UseSSL;
        private System.Windows.Forms.CheckBox cbUseSSL;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form_Server()
		{
			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			ToolTip tip = new ToolTip();
			string needsGroupTip = "More than likely you want to leave this unchecked.  If you are unsure continue reading this tip or check the changelog.txt.\r\n\r\nThe NNTP RFC specs are a bit unclear about this, but some servers do not require a 'GROUP' command before retrieving an article (with 'ARTICLE' or 'BODY') when using message-id's.\r\n\r\nWhat does this mean? On some NNTP servers, the GROUP command is rather slow, when by-passing the 'GROUP' command, you can download articles quicker if they come from different groups.\r\n\r\n Also, sometimes this will allow you to download articles from groups that your news server doesnt carry.\r\n\r\nIf, after turning off 'Needs Group', you get article errors, you may need to enable 'Needs Group' again.";

			tip.SetToolTip(this.Label_NeedsGroup, needsGroupTip);
			tip.SetToolTip(this.cbNeedsGroup, needsGroupTip);
			tip.Active = true;
		}

		public Server LastSetServer;

		public ServerValues GetServer()
		{
			int group = System.Convert.ToInt32(Number_Group.Value - 1);
			string host = Box_Address.Text;
			int port = System.Convert.ToInt32(Number_Port.Value);
			int connections = System.Convert.ToInt32(Number_Connections.Value);
			bool login = Check_Login.Checked;
			string user = "";
			string password = "";
			bool needsgroup = cbNeedsGroup.Checked;
            bool usessl = cbUseSSL.Checked;
			
			if(login)
			{
				user = Text_Login.Text;
				password = Text_Password.Text;
			}

			if(host != "")
			{
				return new ServerValues(group, host, port, connections, login, user, password, needsgroup, usessl);
			}

			return new ServerValues(0, "", 0, 0, false, "", "", true, false);
		}

		public void SetServer(Server server)
		{
			this.Number_Group.Minimum = 1;
			this.Number_Group.Maximum = server.ServerGroup.ServerManager.m_ServerGroups.Count + 1;

			Number_Group.Value = server.ServerGroup.ServerManager.m_ServerGroups.IndexOf(server.ServerGroup) + 1;
			Box_Address.Text = server.Hostname;
			Number_Port.Value = server.Port;
			Number_Connections.Value = server.NoConnections;
			Check_Login.Checked = server.RequiresLogin;
			if(server.RequiresLogin)
			{
				Text_Login.Text = server.Username;
				Text_Password.Text = server.Password;
			}
			cbNeedsGroup.Checked = server.NeedsGroup;
            cbUseSSL.Checked = server.UseSSL;
			LastSetServer = server;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.Label_Address = new System.Windows.Forms.Label();
            this.Label_Port = new System.Windows.Forms.Label();
            this.Label_Connections = new System.Windows.Forms.Label();
            this.Label_UseLogin = new System.Windows.Forms.Label();
            this.Label_Login = new System.Windows.Forms.Label();
            this.Label_Password = new System.Windows.Forms.Label();
            this.Label_Group = new System.Windows.Forms.Label();
            this.Box_Address = new System.Windows.Forms.TextBox();
            this.Number_Connections = new System.Windows.Forms.NumericUpDown();
            this.Check_Login = new System.Windows.Forms.CheckBox();
            this.Text_Login = new System.Windows.Forms.TextBox();
            this.Text_Password = new System.Windows.Forms.TextBox();
            this.Button_OK = new System.Windows.Forms.Button();
            this.Button_Cancel = new System.Windows.Forms.Button();
            this.Number_Group = new System.Windows.Forms.NumericUpDown();
            this.Number_Port = new System.Windows.Forms.NumericUpDown();
            this.cbNeedsGroup = new System.Windows.Forms.CheckBox();
            this.Label_NeedsGroup = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Label_UseSSL = new System.Windows.Forms.Label();
            this.cbUseSSL = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.Number_Connections)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Number_Group)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Number_Port)).BeginInit();
            this.SuspendLayout();
            // 
            // Label_Address
            // 
            this.Label_Address.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Address.Location = new System.Drawing.Point(8, 32);
            this.Label_Address.Name = "Label_Address";
            this.Label_Address.Size = new System.Drawing.Size(100, 20);
            this.Label_Address.TabIndex = 11;
            this.Label_Address.Text = "Address:";
            this.Label_Address.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label_Port
            // 
            this.Label_Port.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Port.Location = new System.Drawing.Point(8, 56);
            this.Label_Port.Name = "Label_Port";
            this.Label_Port.Size = new System.Drawing.Size(100, 20);
            this.Label_Port.TabIndex = 12;
            this.Label_Port.Text = "Port:";
            this.Label_Port.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label_Connections
            // 
            this.Label_Connections.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Connections.Location = new System.Drawing.Point(8, 80);
            this.Label_Connections.Name = "Label_Connections";
            this.Label_Connections.Size = new System.Drawing.Size(100, 20);
            this.Label_Connections.TabIndex = 13;
            this.Label_Connections.Text = "Connections:";
            this.Label_Connections.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label_UseLogin
            // 
            this.Label_UseLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_UseLogin.Location = new System.Drawing.Point(8, 104);
            this.Label_UseLogin.Name = "Label_UseLogin";
            this.Label_UseLogin.Size = new System.Drawing.Size(100, 20);
            this.Label_UseLogin.TabIndex = 14;
            this.Label_UseLogin.Text = "Use Login:";
            this.Label_UseLogin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label_Login
            // 
            this.Label_Login.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Login.Location = new System.Drawing.Point(8, 128);
            this.Label_Login.Name = "Label_Login";
            this.Label_Login.Size = new System.Drawing.Size(100, 20);
            this.Label_Login.TabIndex = 15;
            this.Label_Login.Text = "Login:";
            this.Label_Login.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label_Password
            // 
            this.Label_Password.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Password.Location = new System.Drawing.Point(8, 152);
            this.Label_Password.Name = "Label_Password";
            this.Label_Password.Size = new System.Drawing.Size(100, 20);
            this.Label_Password.TabIndex = 16;
            this.Label_Password.Text = "Password:";
            this.Label_Password.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Label_Group
            // 
            this.Label_Group.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Group.Location = new System.Drawing.Point(8, 8);
            this.Label_Group.Name = "Label_Group";
            this.Label_Group.Size = new System.Drawing.Size(100, 20);
            this.Label_Group.TabIndex = 10;
            this.Label_Group.Text = "Group:";
            this.Label_Group.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Box_Address
            // 
            this.Box_Address.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Box_Address.Location = new System.Drawing.Point(120, 32);
            this.Box_Address.Name = "Box_Address";
            this.Box_Address.Size = new System.Drawing.Size(120, 20);
            this.Box_Address.TabIndex = 2;
            // 
            // Number_Connections
            // 
            this.Number_Connections.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Number_Connections.Location = new System.Drawing.Point(120, 80);
            this.Number_Connections.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.Number_Connections.Name = "Number_Connections";
            this.Number_Connections.Size = new System.Drawing.Size(64, 20);
            this.Number_Connections.TabIndex = 4;
            this.Number_Connections.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Number_Connections.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Check_Login
            // 
            this.Check_Login.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Check_Login.Location = new System.Drawing.Point(120, 104);
            this.Check_Login.Name = "Check_Login";
            this.Check_Login.Size = new System.Drawing.Size(24, 24);
            this.Check_Login.TabIndex = 5;
            this.Check_Login.CheckedChanged += new System.EventHandler(this.Check_Login_CheckedChanged);
            // 
            // Text_Login
            // 
            this.Text_Login.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Text_Login.Enabled = false;
            this.Text_Login.Location = new System.Drawing.Point(120, 128);
            this.Text_Login.Name = "Text_Login";
            this.Text_Login.Size = new System.Drawing.Size(120, 20);
            this.Text_Login.TabIndex = 6;
            // 
            // Text_Password
            // 
            this.Text_Password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Text_Password.Enabled = false;
            this.Text_Password.Location = new System.Drawing.Point(120, 152);
            this.Text_Password.Name = "Text_Password";
            this.Text_Password.PasswordChar = '*';
            this.Text_Password.Size = new System.Drawing.Size(120, 20);
            this.Text_Password.TabIndex = 7;
            // 
            // Button_OK
            // 
            this.Button_OK.BackColor = System.Drawing.Color.White;
            this.Button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_OK.Location = new System.Drawing.Point(49, 223);
            this.Button_OK.Name = "Button_OK";
            this.Button_OK.Size = new System.Drawing.Size(80, 24);
            this.Button_OK.TabIndex = 8;
            this.Button_OK.Text = "OK";
            this.Button_OK.UseVisualStyleBackColor = false;
            // 
            // Button_Cancel
            // 
            this.Button_Cancel.BackColor = System.Drawing.Color.White;
            this.Button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Button_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Cancel.Location = new System.Drawing.Point(137, 223);
            this.Button_Cancel.Name = "Button_Cancel";
            this.Button_Cancel.Size = new System.Drawing.Size(80, 24);
            this.Button_Cancel.TabIndex = 9;
            this.Button_Cancel.Text = "Cancel";
            this.Button_Cancel.UseVisualStyleBackColor = false;
            // 
            // Number_Group
            // 
            this.Number_Group.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Number_Group.Location = new System.Drawing.Point(120, 8);
            this.Number_Group.Name = "Number_Group";
            this.Number_Group.Size = new System.Drawing.Size(64, 20);
            this.Number_Group.TabIndex = 1;
            this.Number_Group.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Number_Group.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Number_Port
            // 
            this.Number_Port.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Number_Port.Location = new System.Drawing.Point(120, 56);
            this.Number_Port.Maximum = new decimal(new int[] {
            65000,
            0,
            0,
            0});
            this.Number_Port.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.Number_Port.Name = "Number_Port";
            this.Number_Port.Size = new System.Drawing.Size(64, 20);
            this.Number_Port.TabIndex = 3;
            this.Number_Port.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Number_Port.Value = new decimal(new int[] {
            119,
            0,
            0,
            0});
            // 
            // cbNeedsGroup
            // 
            this.cbNeedsGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbNeedsGroup.Location = new System.Drawing.Point(120, 176);
            this.cbNeedsGroup.Name = "cbNeedsGroup";
            this.cbNeedsGroup.Size = new System.Drawing.Size(24, 24);
            this.cbNeedsGroup.TabIndex = 17;
            // 
            // Label_NeedsGroup
            // 
            this.Label_NeedsGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_NeedsGroup.Location = new System.Drawing.Point(-4, 176);
            this.Label_NeedsGroup.Name = "Label_NeedsGroup";
            this.Label_NeedsGroup.Size = new System.Drawing.Size(112, 20);
            this.Label_NeedsGroup.TabIndex = 18;
            this.Label_NeedsGroup.Text = "Needs group*:";
            this.Label_NeedsGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 255);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 16);
            this.label1.TabIndex = 19;
            this.label1.Text = "* See tooltip or changelog.txt for more information";
            // 
            // Label_UseSSL
            // 
            this.Label_UseSSL.AutoSize = true;
            this.Label_UseSSL.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_UseSSL.Location = new System.Drawing.Point(37, 200);
            this.Label_UseSSL.Name = "Label_UseSSL";
            this.Label_UseSSL.Size = new System.Drawing.Size(71, 18);
            this.Label_UseSSL.TabIndex = 20;
            this.Label_UseSSL.Text = "Use SSL:";
            // 
            // cbUseSSL
            // 
            this.cbUseSSL.AutoSize = true;
            this.cbUseSSL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbUseSSL.Location = new System.Drawing.Point(120, 205);
            this.cbUseSSL.Name = "cbUseSSL";
            this.cbUseSSL.Size = new System.Drawing.Size(12, 11);
            this.cbUseSSL.TabIndex = 21;
            this.cbUseSSL.UseVisualStyleBackColor = true;
            // 
            // Form_Server
            // 
            this.AcceptButton = this.Button_OK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.Button_Cancel;
            this.ClientSize = new System.Drawing.Size(250, 271);
            this.ControlBox = false;
            this.Controls.Add(this.cbUseSSL);
            this.Controls.Add(this.Label_UseSSL);
            this.Controls.Add(this.cbNeedsGroup);
            this.Controls.Add(this.Label_NeedsGroup);
            this.Controls.Add(this.Number_Port);
            this.Controls.Add(this.Number_Group);
            this.Controls.Add(this.Button_Cancel);
            this.Controls.Add(this.Button_OK);
            this.Controls.Add(this.Text_Password);
            this.Controls.Add(this.Text_Login);
            this.Controls.Add(this.Box_Address);
            this.Controls.Add(this.Check_Login);
            this.Controls.Add(this.Number_Connections);
            this.Controls.Add(this.Label_Group);
            this.Controls.Add(this.Label_Password);
            this.Controls.Add(this.Label_Login);
            this.Controls.Add(this.Label_UseLogin);
            this.Controls.Add(this.Label_Connections);
            this.Controls.Add(this.Label_Port);
            this.Controls.Add(this.Label_Address);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form_Server";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Server";
            ((System.ComponentModel.ISupportInitialize)(this.Number_Connections)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Number_Group)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Number_Port)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void Check_Login_CheckedChanged(object sender, System.EventArgs e)
		{
			Text_Login.Enabled = Check_Login.Checked;
			Text_Password.Enabled = Check_Login.Checked;
		}
	}
}
