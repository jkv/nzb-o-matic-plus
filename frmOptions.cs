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
// File:    frmOptions.cs
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
	public struct OptionValues
	{
		public bool MinimizeToTray;
		public bool DisconnectOnIdle;
		public int IdleDelay;
		public bool AutoPrune;
		public bool RetryConnections;
		public int RetryDelay;
		public bool LimitAttempts;
		public int RetryAttempts;
		public bool AssociateWithNZB;
		public bool ConnectOnStart;
		public string SavePath;
		public string SaveFolder;
		public bool DeleteNZB;
		public bool MonitorFolder;
		public string MonitorPath;
		public bool PausePar2;

		public OptionValues(bool settray, bool setidle, int setidledelay, bool setprune, bool setretry, int setretrydelay, bool setlimitattempts, int setretryattempts, bool setassociate, bool setconnectonstart, string setsavepath, string setsavefolder, bool setdeletenzb, bool monFolder, string monPath, bool setpausepar2)
		{
			MinimizeToTray = settray;
			DisconnectOnIdle = setidle;
			IdleDelay = setidledelay;
			AutoPrune = setprune;
			RetryConnections = setretry;
			RetryDelay = setretrydelay;
			LimitAttempts = setlimitattempts;
			RetryAttempts = setretryattempts;
			AssociateWithNZB = setassociate;
			ConnectOnStart = setconnectonstart;
			SavePath = setsavepath;
			SaveFolder = setsavefolder;
			DeleteNZB = setdeletenzb;
			MonitorFolder = monFolder;
			MonitorPath = monPath;
			PausePar2 = setpausepar2;
		}
	}

	/// <summary>
	/// Summary description for frmOptions.
	/// </summary>
	public class frmOptions : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label Label_MinTray;
		private System.Windows.Forms.Label Label_Idle;
		private System.Windows.Forms.Label Label_IdleTime;
		private System.Windows.Forms.Label Label_Prune;
		private System.Windows.Forms.Label Label_Retry;
		private System.Windows.Forms.Label Label_Delay;
		private System.Windows.Forms.Label Label_Associate;
		private System.Windows.Forms.CheckBox Check_Tray;
		private System.Windows.Forms.CheckBox Check_Idle;
		private System.Windows.Forms.NumericUpDown Number_Idle;
		private System.Windows.Forms.CheckBox Check_Prune;
		private System.Windows.Forms.CheckBox Check_Retry;
		private System.Windows.Forms.NumericUpDown Number_Delay;
		private System.Windows.Forms.CheckBox Check_Associate;
		private System.Windows.Forms.NumericUpDown Numer_Attempts;
		private System.Windows.Forms.CheckBox Check_Attempts;
		private System.Windows.Forms.Button Button_Cancel;
		private System.Windows.Forms.Button Button_OK;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Label Label_Connect;
		private System.Windows.Forms.CheckBox Check_Connect;
		private System.Windows.Forms.Label Label_Attempts;
		private System.Windows.Forms.Label Label_Limit;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label Label_Path;
		private System.Windows.Forms.FolderBrowserDialog FolderBrowse_Path;
		private System.Windows.Forms.TextBox TextBox_Path;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Button Button_Save;
		private System.Windows.Forms.TextBox TextBox_Folder;
		private System.Windows.Forms.Label Label_Folder;
		private System.Windows.Forms.Label Label_Delete;
		private System.Windows.Forms.CheckBox Check_Delete;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button Button_NZB;
		private System.Windows.Forms.TextBox Monitor_folder;
		private System.Windows.Forms.CheckBox Check_Monitor;
		private System.Windows.Forms.CheckBox Check_Pause_Par2;
		private System.Windows.Forms.Label Label_Pause_Par2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmOptions()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			string folderTip = "'%x', = file extension\r\n'%s', = article subject\r\n'%n', = file name\r\n'%g', = newsgroup\r\n'%p', = file poster\r\n'%S', = file size\r\n'%d', = post date\r\n'%D', = date now\r\n'%t', = article status ----- NEED \r\n'%i'  = name of file imported from\r\n'%y'  = name of the newzbin file minus the beginning msgid_<number> part (replaces '_' with ' ')\r\n'%z'  = name of the newzbin file minus the beginning msgid_<number> part";
			ToolTip tip = new ToolTip();
			tip.SetToolTip(this.TextBox_Folder, folderTip);
			tip.SetToolTip(this.Label_Folder, folderTip);
			tip.Active = true;
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

		public OptionValues GetOptions()
		{
			bool tray = this.Check_Tray.Checked;
			bool idle = this.Check_Idle.Checked;
			int idledelay = System.Convert.ToInt32(this.Number_Idle.Value);
			bool prune = this.Check_Prune.Checked;
			bool retryconnections = this.Check_Retry.Checked;
			int retrydelay = System.Convert.ToInt32(this.Number_Delay.Value);
			bool limitattempts = this.Check_Attempts.Checked;
			int retryattempts = System.Convert.ToInt32(this.Numer_Attempts.Value);
			bool associate = this.Check_Associate.Checked;
			bool connect = this.Check_Connect.Checked;
			//NEED TO DO VALIDATION
			string savepath = this.TextBox_Path.Text;
			string savefolder = this.TextBox_Folder.Text;
			bool delete = this.Check_Delete.Checked;
			bool monFolder = this.Check_Monitor.Checked;
			string monPath = this.Monitor_folder.Text;
			bool setpausepar2 = this.Check_Pause_Par2.Checked;

			return new OptionValues(tray, idle, idledelay, prune, retryconnections, retrydelay, limitattempts, retryattempts, associate, connect, savepath, savefolder, delete, monFolder, monPath, setpausepar2 );
		}

		public void SetOptions(OptionValues ov)
		{
			if(ov.SavePath == "")
				ov.SavePath = System.IO.Path.GetFullPath(Global.m_DownloadDirectory );

			this.Check_Tray.Checked = ov.MinimizeToTray;
			this.Check_Idle.Checked = ov.DisconnectOnIdle;
			this.Number_Idle.Value = ov.IdleDelay;
			this.Check_Prune.Checked = ov.AutoPrune;
			this.Check_Retry.Checked = ov.RetryConnections;
			this.Number_Delay.Value = ov.RetryDelay;
			this.Check_Attempts.Checked = ov.LimitAttempts;
			this.Numer_Attempts.Value = ov.RetryAttempts;
			this.Check_Associate.Checked = ov.AssociateWithNZB;
			this.Check_Connect.Checked = ov.ConnectOnStart;
			this.TextBox_Path.Text = ov.SavePath;
			this.TextBox_Folder.Text = ov.SaveFolder;
			this.Check_Delete.Checked = ov.DeleteNZB;	
			this.Monitor_folder.Text = ov.MonitorPath;
			this.Check_Monitor.Checked = ov.MonitorFolder;
			this.Check_Pause_Par2.Checked = ov.PausePar2;
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Label_MinTray = new System.Windows.Forms.Label();
			this.Label_Idle = new System.Windows.Forms.Label();
			this.Label_IdleTime = new System.Windows.Forms.Label();
			this.Label_Prune = new System.Windows.Forms.Label();
			this.Label_Retry = new System.Windows.Forms.Label();
			this.Label_Delay = new System.Windows.Forms.Label();
			this.Label_Associate = new System.Windows.Forms.Label();
			this.Check_Tray = new System.Windows.Forms.CheckBox();
			this.Check_Idle = new System.Windows.Forms.CheckBox();
			this.Number_Idle = new System.Windows.Forms.NumericUpDown();
			this.Check_Prune = new System.Windows.Forms.CheckBox();
			this.Check_Retry = new System.Windows.Forms.CheckBox();
			this.Number_Delay = new System.Windows.Forms.NumericUpDown();
			this.Check_Associate = new System.Windows.Forms.CheckBox();
			this.Label_Attempts = new System.Windows.Forms.Label();
			this.Numer_Attempts = new System.Windows.Forms.NumericUpDown();
			this.Label_Limit = new System.Windows.Forms.Label();
			this.Check_Attempts = new System.Windows.Forms.CheckBox();
			this.Button_Cancel = new System.Windows.Forms.Button();
			this.Button_OK = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.Check_Pause_Par2 = new System.Windows.Forms.CheckBox();
			this.Label_Pause_Par2 = new System.Windows.Forms.Label();
			this.Check_Connect = new System.Windows.Forms.CheckBox();
			this.Label_Connect = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.TextBox_Folder = new System.Windows.Forms.TextBox();
			this.Label_Folder = new System.Windows.Forms.Label();
			this.Button_Save = new System.Windows.Forms.Button();
			this.TextBox_Path = new System.Windows.Forms.TextBox();
			this.Label_Path = new System.Windows.Forms.Label();
			this.FolderBrowse_Path = new System.Windows.Forms.FolderBrowserDialog();
			this.panel5 = new System.Windows.Forms.Panel();
			this.Label_Delete = new System.Windows.Forms.Label();
			this.Check_Delete = new System.Windows.Forms.CheckBox();
			this.panel6 = new System.Windows.Forms.Panel();
			this.Check_Monitor = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.Button_NZB = new System.Windows.Forms.Button();
			this.Monitor_folder = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.Number_Idle)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Number_Delay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Numer_Attempts)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panel6.SuspendLayout();
			this.SuspendLayout();
			// 
			// Label_MinTray
			// 
			this.Label_MinTray.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_MinTray.ImageAlign = System.Drawing.ContentAlignment.TopRight;
			this.Label_MinTray.Location = new System.Drawing.Point(-16, 0);
			this.Label_MinTray.Name = "Label_MinTray";
			this.Label_MinTray.Size = new System.Drawing.Size(144, 20);
			this.Label_MinTray.TabIndex = 0;
			this.Label_MinTray.Text = "Minimize to Tray:";
			this.Label_MinTray.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label_Idle
			// 
			this.Label_Idle.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Idle.Location = new System.Drawing.Point(-24, 2);
			this.Label_Idle.Name = "Label_Idle";
			this.Label_Idle.Size = new System.Drawing.Size(144, 24);
			this.Label_Idle.TabIndex = 1;
			this.Label_Idle.Text = "Disconnect on Idle:";
			this.Label_Idle.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label_IdleTime
			// 
			this.Label_IdleTime.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_IdleTime.Location = new System.Drawing.Point(-8, 24);
			this.Label_IdleTime.Name = "Label_IdleTime";
			this.Label_IdleTime.Size = new System.Drawing.Size(128, 20);
			this.Label_IdleTime.TabIndex = 2;
			this.Label_IdleTime.Text = "Idle Time (mins):";
			this.Label_IdleTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label_Prune
			// 
			this.Label_Prune.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Prune.ImageAlign = System.Drawing.ContentAlignment.TopRight;
			this.Label_Prune.Location = new System.Drawing.Point(-16, 24);
			this.Label_Prune.Name = "Label_Prune";
			this.Label_Prune.Size = new System.Drawing.Size(144, 20);
			this.Label_Prune.TabIndex = 3;
			this.Label_Prune.Text = "Auto-Prune:";
			this.Label_Prune.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label_Retry
			// 
			this.Label_Retry.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Retry.Location = new System.Drawing.Point(-16, 0);
			this.Label_Retry.Name = "Label_Retry";
			this.Label_Retry.Size = new System.Drawing.Size(140, 22);
			this.Label_Retry.TabIndex = 4;
			this.Label_Retry.Text = "Retry Connections:";
			this.Label_Retry.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label_Delay
			// 
			this.Label_Delay.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Delay.Location = new System.Drawing.Point(-16, 24);
			this.Label_Delay.Name = "Label_Delay";
			this.Label_Delay.Size = new System.Drawing.Size(140, 22);
			this.Label_Delay.TabIndex = 5;
			this.Label_Delay.Text = "Delay (mins):";
			this.Label_Delay.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Label_Associate
			// 
			this.Label_Associate.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Associate.Location = new System.Drawing.Point(-16, 2);
			this.Label_Associate.Name = "Label_Associate";
			this.Label_Associate.Size = new System.Drawing.Size(144, 20);
			this.Label_Associate.TabIndex = 6;
			this.Label_Associate.Text = "Associate with .nzb:";
			this.Label_Associate.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Check_Tray
			// 
			this.Check_Tray.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Tray.Location = new System.Drawing.Point(128, 2);
			this.Check_Tray.Name = "Check_Tray";
			this.Check_Tray.Size = new System.Drawing.Size(16, 16);
			this.Check_Tray.TabIndex = 7;
			// 
			// Check_Idle
			// 
			this.Check_Idle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Idle.Location = new System.Drawing.Point(128, 2);
			this.Check_Idle.Name = "Check_Idle";
			this.Check_Idle.Size = new System.Drawing.Size(16, 16);
			this.Check_Idle.TabIndex = 8;
			// 
			// Number_Idle
			// 
			this.Number_Idle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Number_Idle.Location = new System.Drawing.Point(128, 24);
			this.Number_Idle.Minimum = new System.Decimal(new int[] {
																		1,
																		0,
																		0,
																		0});
			this.Number_Idle.Name = "Number_Idle";
			this.Number_Idle.Size = new System.Drawing.Size(40, 20);
			this.Number_Idle.TabIndex = 9;
			this.Number_Idle.Value = new System.Decimal(new int[] {
																	  15,
																	  0,
																	  0,
																	  0});
			// 
			// Check_Prune
			// 
			this.Check_Prune.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Prune.Location = new System.Drawing.Point(128, 25);
			this.Check_Prune.Name = "Check_Prune";
			this.Check_Prune.Size = new System.Drawing.Size(16, 16);
			this.Check_Prune.TabIndex = 10;
			// 
			// Check_Retry
			// 
			this.Check_Retry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Retry.Location = new System.Drawing.Point(128, 2);
			this.Check_Retry.Name = "Check_Retry";
			this.Check_Retry.Size = new System.Drawing.Size(16, 16);
			this.Check_Retry.TabIndex = 11;
			// 
			// Number_Delay
			// 
			this.Number_Delay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Number_Delay.Location = new System.Drawing.Point(128, 24);
			this.Number_Delay.Minimum = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 0});
			this.Number_Delay.Name = "Number_Delay";
			this.Number_Delay.Size = new System.Drawing.Size(40, 20);
			this.Number_Delay.TabIndex = 12;
			this.Number_Delay.Value = new System.Decimal(new int[] {
																	   5,
																	   0,
																	   0,
																	   0});
			// 
			// Check_Associate
			// 
			this.Check_Associate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Associate.Location = new System.Drawing.Point(128, 2);
			this.Check_Associate.Name = "Check_Associate";
			this.Check_Associate.Size = new System.Drawing.Size(16, 16);
			this.Check_Associate.TabIndex = 13;
			// 
			// Label_Attempts
			// 
			this.Label_Attempts.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Attempts.Location = new System.Drawing.Point(-16, 72);
			this.Label_Attempts.Name = "Label_Attempts";
			this.Label_Attempts.Size = new System.Drawing.Size(140, 22);
			this.Label_Attempts.TabIndex = 14;
			this.Label_Attempts.Text = "Retry Attempts:";
			this.Label_Attempts.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Numer_Attempts
			// 
			this.Numer_Attempts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Numer_Attempts.Location = new System.Drawing.Point(128, 72);
			this.Numer_Attempts.Name = "Numer_Attempts";
			this.Numer_Attempts.Size = new System.Drawing.Size(40, 20);
			this.Numer_Attempts.TabIndex = 15;
			this.Numer_Attempts.Value = new System.Decimal(new int[] {
																		 3,
																		 0,
																		 0,
																		 0});
			// 
			// Label_Limit
			// 
			this.Label_Limit.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Limit.Location = new System.Drawing.Point(-16, 48);
			this.Label_Limit.Name = "Label_Limit";
			this.Label_Limit.Size = new System.Drawing.Size(140, 22);
			this.Label_Limit.TabIndex = 16;
			this.Label_Limit.Text = "Limit Attempts:";
			this.Label_Limit.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Check_Attempts
			// 
			this.Check_Attempts.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Attempts.Location = new System.Drawing.Point(128, 50);
			this.Check_Attempts.Name = "Check_Attempts";
			this.Check_Attempts.Size = new System.Drawing.Size(16, 16);
			this.Check_Attempts.TabIndex = 17;
			// 
			// Button_Cancel
			// 
			this.Button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Button_Cancel.BackColor = System.Drawing.Color.White;
			this.Button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Button_Cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Button_Cancel.Location = new System.Drawing.Point(181, 296);
			this.Button_Cancel.Name = "Button_Cancel";
			this.Button_Cancel.Size = new System.Drawing.Size(80, 24);
			this.Button_Cancel.TabIndex = 19;
			this.Button_Cancel.Text = "Cancel";
			this.Button_Cancel.Click += new System.EventHandler(this.Button_Cancel_Click);
			// 
			// Button_OK
			// 
			this.Button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Button_OK.BackColor = System.Drawing.Color.White;
			this.Button_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Button_OK.Location = new System.Drawing.Point(93, 296);
			this.Button_OK.Name = "Button_OK";
			this.Button_OK.Size = new System.Drawing.Size(80, 24);
			this.Button_OK.TabIndex = 18;
			this.Button_OK.Text = "OK";
			this.Button_OK.Click += new System.EventHandler(this.Button_OK_Click);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.Check_Pause_Par2);
			this.panel1.Controls.Add(this.Label_Pause_Par2);
			this.panel1.Controls.Add(this.Check_Connect);
			this.panel1.Controls.Add(this.Label_Connect);
			this.panel1.Controls.Add(this.Label_MinTray);
			this.panel1.Controls.Add(this.Label_Prune);
			this.panel1.Controls.Add(this.Check_Prune);
			this.panel1.Controls.Add(this.Check_Tray);
			this.panel1.Location = new System.Drawing.Point(192, 8);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(152, 96);
			this.panel1.TabIndex = 20;
			// 
			// Check_Pause_Par2
			// 
			this.Check_Pause_Par2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Pause_Par2.Location = new System.Drawing.Point(128, 72);
			this.Check_Pause_Par2.Name = "Check_Pause_Par2";
			this.Check_Pause_Par2.Size = new System.Drawing.Size(16, 16);
			this.Check_Pause_Par2.TabIndex = 17;
			// 
			// Label_Pause_Par2
			// 
			this.Label_Pause_Par2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Pause_Par2.Location = new System.Drawing.Point(-24, 72);
			this.Label_Pause_Par2.Name = "Label_Pause_Par2";
			this.Label_Pause_Par2.Size = new System.Drawing.Size(144, 16);
			this.Label_Pause_Par2.TabIndex = 16;
			this.Label_Pause_Par2.Text = "Pause PAR2 Files";
			this.Label_Pause_Par2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Check_Connect
			// 
			this.Check_Connect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Connect.Location = new System.Drawing.Point(128, 48);
			this.Check_Connect.Name = "Check_Connect";
			this.Check_Connect.Size = new System.Drawing.Size(16, 16);
			this.Check_Connect.TabIndex = 15;
			// 
			// Label_Connect
			// 
			this.Label_Connect.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Connect.Location = new System.Drawing.Point(-16, 48);
			this.Label_Connect.Name = "Label_Connect";
			this.Label_Connect.Size = new System.Drawing.Size(144, 20);
			this.Label_Connect.TabIndex = 14;
			this.Label_Connect.Text = "Connect on start:";
			this.Label_Connect.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.Color.White;
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.Check_Idle);
			this.panel2.Controls.Add(this.Number_Idle);
			this.panel2.Controls.Add(this.Label_IdleTime);
			this.panel2.Controls.Add(this.Label_Idle);
			this.panel2.Location = new System.Drawing.Point(8, 112);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(176, 48);
			this.panel2.TabIndex = 21;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.Color.White;
			this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel3.Controls.Add(this.Label_Delay);
			this.panel3.Controls.Add(this.Check_Retry);
			this.panel3.Controls.Add(this.Number_Delay);
			this.panel3.Controls.Add(this.Label_Attempts);
			this.panel3.Controls.Add(this.Label_Retry);
			this.panel3.Controls.Add(this.Numer_Attempts);
			this.panel3.Controls.Add(this.Label_Limit);
			this.panel3.Controls.Add(this.Check_Attempts);
			this.panel3.Location = new System.Drawing.Point(8, 8);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(176, 96);
			this.panel3.TabIndex = 22;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.Color.White;
			this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel4.Controls.Add(this.TextBox_Folder);
			this.panel4.Controls.Add(this.Label_Folder);
			this.panel4.Controls.Add(this.Button_Save);
			this.panel4.Controls.Add(this.TextBox_Path);
			this.panel4.Controls.Add(this.Label_Path);
			this.panel4.Location = new System.Drawing.Point(8, 168);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(336, 56);
			this.panel4.TabIndex = 23;
			// 
			// TextBox_Folder
			// 
			this.TextBox_Folder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextBox_Folder.Location = new System.Drawing.Point(88, 29);
			this.TextBox_Folder.Name = "TextBox_Folder";
			this.TextBox_Folder.Size = new System.Drawing.Size(240, 20);
			this.TextBox_Folder.TabIndex = 6;
			this.TextBox_Folder.Text = "";
			// 
			// Label_Folder
			// 
			this.Label_Folder.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Folder.Location = new System.Drawing.Point(-40, 29);
			this.Label_Folder.Name = "Label_Folder";
			this.Label_Folder.Size = new System.Drawing.Size(128, 20);
			this.Label_Folder.TabIndex = 5;
			this.Label_Folder.Text = "Save folder:";
			this.Label_Folder.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Button_Save
			// 
			this.Button_Save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Button_Save.Location = new System.Drawing.Point(296, 5);
			this.Button_Save.Name = "Button_Save";
			this.Button_Save.Size = new System.Drawing.Size(32, 20);
			this.Button_Save.TabIndex = 4;
			this.Button_Save.Text = "...";
			this.Button_Save.Click += new System.EventHandler(this.Button_Save_Click);
			// 
			// TextBox_Path
			// 
			this.TextBox_Path.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.TextBox_Path.Location = new System.Drawing.Point(88, 5);
			this.TextBox_Path.Name = "TextBox_Path";
			this.TextBox_Path.Size = new System.Drawing.Size(210, 20);
			this.TextBox_Path.TabIndex = 3;
			this.TextBox_Path.Text = "";
			// 
			// Label_Path
			// 
			this.Label_Path.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Path.Location = new System.Drawing.Point(-40, 5);
			this.Label_Path.Name = "Label_Path";
			this.Label_Path.Size = new System.Drawing.Size(128, 20);
			this.Label_Path.TabIndex = 2;
			this.Label_Path.Text = "Save path:";
			this.Label_Path.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// panel5
			// 
			this.panel5.BackColor = System.Drawing.Color.White;
			this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel5.Controls.Add(this.Label_Delete);
			this.panel5.Controls.Add(this.Check_Delete);
			this.panel5.Controls.Add(this.Label_Associate);
			this.panel5.Controls.Add(this.Check_Associate);
			this.panel5.Location = new System.Drawing.Point(192, 112);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(152, 48);
			this.panel5.TabIndex = 24;
			// 
			// Label_Delete
			// 
			this.Label_Delete.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Label_Delete.Location = new System.Drawing.Point(-16, 24);
			this.Label_Delete.Name = "Label_Delete";
			this.Label_Delete.Size = new System.Drawing.Size(144, 20);
			this.Label_Delete.TabIndex = 14;
			this.Label_Delete.Text = "Delete after import:";
			this.Label_Delete.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Check_Delete
			// 
			this.Check_Delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Delete.Location = new System.Drawing.Point(128, 25);
			this.Check_Delete.Name = "Check_Delete";
			this.Check_Delete.Size = new System.Drawing.Size(16, 16);
			this.Check_Delete.TabIndex = 15;
			// 
			// panel6
			// 
			this.panel6.BackColor = System.Drawing.Color.White;
			this.panel6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel6.Controls.Add(this.Check_Monitor);
			this.panel6.Controls.Add(this.label3);
			this.panel6.Controls.Add(this.Button_NZB);
			this.panel6.Controls.Add(this.Monitor_folder);
			this.panel6.Controls.Add(this.label2);
			this.panel6.Location = new System.Drawing.Point(9, 232);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(336, 56);
			this.panel6.TabIndex = 25;
			// 
			// Check_Monitor
			// 
			this.Check_Monitor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Check_Monitor.Location = new System.Drawing.Point(144, 8);
			this.Check_Monitor.Name = "Check_Monitor";
			this.Check_Monitor.Size = new System.Drawing.Size(16, 16);
			this.Check_Monitor.TabIndex = 10;
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(128, 16);
			this.label3.TabIndex = 9;
			this.label3.Text = "Monitor NZB Folder:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// Button_NZB
			// 
			this.Button_NZB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Button_NZB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Button_NZB.Location = new System.Drawing.Point(296, 29);
			this.Button_NZB.Name = "Button_NZB";
			this.Button_NZB.Size = new System.Drawing.Size(32, 20);
			this.Button_NZB.TabIndex = 4;
			this.Button_NZB.Text = "...";
			this.Button_NZB.Click += new System.EventHandler(this.Button_NZB_Click);
			// 
			// Monitor_folder
			// 
			this.Monitor_folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.Monitor_folder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Monitor_folder.Location = new System.Drawing.Point(88, 29);
			this.Monitor_folder.Name = "Monitor_folder";
			this.Monitor_folder.Size = new System.Drawing.Size(210, 20);
			this.Monitor_folder.TabIndex = 3;
			this.Monitor_folder.Text = "";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(-40, 29);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(128, 20);
			this.label2.TabIndex = 2;
			this.label2.Text = "NZB path:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// frmOptions
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(354, 328);
			this.Controls.Add(this.panel6);
			this.Controls.Add(this.panel5);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.Button_Cancel);
			this.Controls.Add(this.Button_OK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmOptions";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Options";
			((System.ComponentModel.ISupportInitialize)(this.Number_Idle)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Number_Delay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Numer_Attempts)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void Button_Save_Click(object sender, System.EventArgs e)
		{
			if(this.FolderBrowse_Path.ShowDialog(this) != DialogResult.OK)
				return;

			this.TextBox_Path.Text = this.FolderBrowse_Path.SelectedPath;
		}

		private void Button_NZB_Click(object sender, System.EventArgs e)
		{
			if(this.FolderBrowse_Path.ShowDialog(this) != DialogResult.OK)
				return;

			this.Monitor_folder.Text = this.FolderBrowse_Path.SelectedPath;
		}

		private void Button_Cancel_Click(object sender, System.EventArgs e)
		{
		
		}

		private void Button_OK_Click(object sender, System.EventArgs e)
		{
		
		}

	}
}
