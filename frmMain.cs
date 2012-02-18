#region Class Header
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
// File:    frmMain.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.
#endregion 

#region Using
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text.RegularExpressions;
#endregion 
namespace NZB_O_Matic
{
	public class frmMain : System.Windows.Forms.Form
	{
		#region Dll Imports
		//imports
		[DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern 
			bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern 
			bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
		[DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern 
			bool IsIconic(IntPtr hWnd);
		[DllImport("user32.dll")] private static extern
			int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")] private static extern 
			bool PostMessage(IntPtr hWnd,int msg, IntPtr wParam, IntPtr lParam);
		#endregion 

		#region Variables
		private const int SW_HIDE = 0;
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SW_SHOWNOACTIVATE = 4;
		private const int SW_RESTORE = 9;
		private const int SW_SHOWDEFAULT = 10;

		private const int HWND_BROADCAST = 0xFFFF;
		private const int WM_SETTINGCHANGE = 0x001A;
		private const int SPI_SETNONCLIENTMETRICS = 0x002A;
		private System.Windows.Forms.MenuItem Menu_Main_Options_Prefernces;
		private bool disconnectedOnIdle;
		private bool queueContainsNewItems;

		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem Context_Exit;
		private System.Windows.Forms.NotifyIcon Icon_Tray;
		private System.Windows.Forms.ContextMenu ContextMenu_Icon;
		private System.Windows.Forms.MainMenu MainMenu_Main;
		private System.Windows.Forms.MenuItem Menu_Main_File;
		private System.Windows.Forms.MenuItem Menu_Main_File_Exit;
		private System.Windows.Forms.MenuItem Menu_Main_File_ImportNZB;
		private System.Windows.Forms.MenuItem Menu_Main_File_Connect;
		private System.Windows.Forms.MenuItem Menu_Main_File_Disconnect;
		private System.Windows.Forms.MenuItem Menu_Main_Help;
		private System.Windows.Forms.MenuItem Menu_Main_Help_About;
		private System.Windows.Forms.MenuItem Context_Disconnect;
		private System.Windows.Forms.MenuItem Context_Connect;
		private System.Windows.Forms.Timer Update_Timer;

		private StatusBarPanel PanConnect;
		private StatusBarPanel PanSpeed;
		private StatusBarPanel PanFileSize;
		private StatusBarPanel PanDownloadTime;
		private System.Windows.Forms.MenuItem Menu_Main_Options;
		private System.Windows.Forms.MenuItem Menu_Main_Options_Exit;
		private System.Windows.Forms.OpenFileDialog OpenFile_ImportNZB;
		private System.Windows.Forms.StatusBar Main_StatusBar;
		internal System.Windows.Forms.ListView lvConnections;
		private System.Windows.Forms.ColumnHeader chServer;
		private System.Windows.Forms.ColumnHeader chID;
		private System.Windows.Forms.ColumnHeader chConnStatus;
		private System.Windows.Forms.ColumnHeader chProgress;
		private System.Windows.Forms.ColumnHeader chSpeed;
		private System.Windows.Forms.SaveFileDialog Save_Log;
		private System.Windows.Forms.ContextMenu ContextMenu_Queue;
		private System.Windows.Forms.MenuItem Context_MoveUp;
		private System.Windows.Forms.MenuItem Context_MoveDown;
		private System.Windows.Forms.MenuItem Context_MoveTop;
		private System.Windows.Forms.MenuItem Context_MoveBottom;
		private System.Windows.Forms.MenuItem Context_Decode;
		private StatusBarPanel PanPercent;
		private StatusBarPanel PanDownloaded;
		private System.Windows.Forms.MenuItem Menu_Main_Options_Reset;
		private System.Windows.Forms.MenuItem Menu_Main_Edit;
		private System.Windows.Forms.MenuItem Menu_Main_Edit_AddServer;
		private System.Windows.Forms.MenuItem Menu_Main_Edit_EditServer;
		private System.Windows.Forms.MenuItem Menu_Main_Edit_DeleteServer;
		private System.Windows.Forms.MenuItem Menu_Main_Edit_DeleteArticle;
		private System.Windows.Forms.MenuItem Menu_Main_Edit_DecodeArticle;
		private System.IO.FileSystemWatcher watcher;
		private System.Collections.Hashtable watchedFileList;
		private string m_Sorted = "none";
		//variable to make sure we don't get stuck in a window resize event loop
		private static bool m_ChangingState = false;
		private int count = 0;
		private long idlecount = 0;
		private double[] speedHistory = new double[20];
		private string m_ImportNZB_Filename;
		private bool m_UpdatingServers = false;

		private double m_TotalDownloadOffset = 0;
		private CopyData copydata;
		private System.Windows.Forms.MenuItem Menu_Main_Help_Update;
		private System.Windows.Forms.TabPage TabPage_Servers;
		private System.Windows.Forms.ListView lvServers;
		private System.Windows.Forms.ColumnHeader chServerGroup;
		private System.Windows.Forms.ColumnHeader chAddress;
		private System.Windows.Forms.ColumnHeader chPort;
		private System.Windows.Forms.ColumnHeader chConnections;
		private System.Windows.Forms.ColumnHeader chRequiresLogin;
		private System.Windows.Forms.ColumnHeader chUsername;
		private System.Windows.Forms.ColumnHeader chPassword;
		private System.Windows.Forms.Button Button_Disconnect;
		private System.Windows.Forms.Button Button_Connect;
		private System.Windows.Forms.Button Button_EditServer;
		private System.Windows.Forms.Button Button_DeleteServer;
		private System.Windows.Forms.Button Button_AddServer;
		private System.Windows.Forms.TabPage TabPage_Queue;
		private System.Windows.Forms.ListView lvArticles;
		private System.Windows.Forms.ColumnHeader chArticle;
		private System.Windows.Forms.ColumnHeader chSize;
		private System.Windows.Forms.ColumnHeader chParts;
		private System.Windows.Forms.ColumnHeader chStatus;
		private System.Windows.Forms.ColumnHeader chDate;
		private System.Windows.Forms.ColumnHeader chGroups;
		private System.Windows.Forms.Button Button_Bottom;
		private System.Windows.Forms.Button Button_Down;
		private System.Windows.Forms.Button Button_Up;
		private System.Windows.Forms.Button Button_Top;
		private System.Windows.Forms.Button Button_Prune;
		private System.Windows.Forms.Button Button_DeleteQueue;
		private System.Windows.Forms.Button Button_ImportNZB;
		private System.Windows.Forms.TabPage TabPage_Status;
		private System.Windows.Forms.ListBox List_StatusLog;
		private System.Windows.Forms.Button Button_ClearLog;
		private System.Windows.Forms.Button Button_SaveLog;
		private System.Windows.Forms.MenuItem Context_Divider1;
		private System.Windows.Forms.MenuItem Menu_Main_File_Divider1;
		private System.Windows.Forms.MenuItem Menu_Main_File_Divider2;
		private System.Windows.Forms.MenuItem Menu_Main_Edit_Divider1;
		private System.Windows.Forms.MenuItem Menu_Main_Options_Divider1;
		private System.Windows.Forms.Panel Panel_Connections;
		private System.Windows.Forms.MenuItem Context_Divider2;
		private System.Windows.Forms.MenuItem Context_Divider3;
		private System.Windows.Forms.Splitter Splitter_Lists;
		private System.Windows.Forms.Panel Panel_ServerButtons;
		private System.Windows.Forms.Panel Panel_QueueButtons;
		private System.Windows.Forms.Panel Panel_LogButtons;
		private System.Windows.Forms.TabControl TabControl_Main;
		private System.Windows.Forms.MenuItem Menu_Main_SaveWindowStatus;
		private System.Windows.Forms.MenuItem Menu_Main_Options_ClearCache;
		private System.Windows.Forms.Button setIncompleteToQueuedButton;
		private System.Windows.Forms.Button ButtonDecodeIncomplete;
		private System.Windows.Forms.MenuItem Context_Pause;
		private System.Windows.Forms.MenuItem Context_Delete;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem Context_Prune;
        private ColumnHeader chSSL;
		private ServerManager m_ServerManager;
		#endregion 

		#region Constructor/Dispose
		public frmMain()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//set window title to product + version
			AssemblyName asmn = Assembly.GetExecutingAssembly().GetName();
			Global.Name = asmn.Name;
			Global.Version = asmn.Version.ToString();

			string version = asmn.Version.Major + "." + asmn.Version.Minor;

			this.Text = "NZB-O-MaticPlus - " + version;

			//set selected tab control and enable/disable relevant menu items
			if(this.TabControl_Main.TabPages.Count > 0)
				this.TabControl_Main.SelectedTab = this.TabControl_Main.TabPages[0];

			this.Menu_Main_Edit_AddServer.Enabled = true;
			this.Menu_Main_Edit_EditServer.Enabled = true;
			this.Menu_Main_Edit_DeleteServer.Enabled = true;
			this.Menu_Main_Edit_DeleteArticle.Enabled = false;
			this.Menu_Main_Edit_DecodeArticle.Enabled = false;

			//setup panels for status bar
			PanConnect = new StatusBarPanel();
			PanConnect.MinWidth = 100;
			PanConnect.AutoSize = StatusBarPanelAutoSize.Contents;
			PanConnect.Alignment = HorizontalAlignment.Center;
			Main_StatusBar.Panels.Add(PanConnect);

			PanSpeed = new StatusBarPanel();
			PanSpeed.MinWidth = 100;
			PanSpeed.AutoSize = StatusBarPanelAutoSize.Contents;
			PanSpeed.Alignment = HorizontalAlignment.Center;
			Main_StatusBar.Panels.Add(PanSpeed);

			PanFileSize = new StatusBarPanel();
			PanFileSize.AutoSize = StatusBarPanelAutoSize.Contents;
			PanFileSize.MinWidth = 100;
			PanFileSize.Alignment = HorizontalAlignment.Center;
			Main_StatusBar.Panels.Add(PanFileSize);

			PanPercent = new StatusBarPanel();
			PanPercent.AutoSize = StatusBarPanelAutoSize.Contents;
			PanPercent.MinWidth = 50;
			PanPercent.Alignment = HorizontalAlignment.Center;
			Main_StatusBar.Panels.Add(PanPercent);

			PanDownloadTime = new StatusBarPanel();
			PanDownloadTime.AutoSize = StatusBarPanelAutoSize.Contents;
			PanDownloadTime.MinWidth = 75;
			PanDownloadTime.Alignment = HorizontalAlignment.Center;
			Main_StatusBar.Panels.Add(PanDownloadTime);

			PanDownloaded = new StatusBarPanel();
			PanDownloaded.AutoSize = StatusBarPanelAutoSize.Contents;
			PanDownloaded.MinWidth = 75;
			PanDownloaded.Alignment = HorizontalAlignment.Center;
			Main_StatusBar.Panels.Add(PanDownloaded);

			//set logging window
			frmMain.SetLog(this.List_StatusLog);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}

			base.Dispose( disposing );
		}
		#endregion 

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.OpenFile_ImportNZB = new System.Windows.Forms.OpenFileDialog();
            this.Icon_Tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.ContextMenu_Icon = new System.Windows.Forms.ContextMenu();
            this.Context_Connect = new System.Windows.Forms.MenuItem();
            this.Context_Disconnect = new System.Windows.Forms.MenuItem();
            this.Context_Divider1 = new System.Windows.Forms.MenuItem();
            this.Context_Exit = new System.Windows.Forms.MenuItem();
            this.MainMenu_Main = new System.Windows.Forms.MainMenu(this.components);
            this.Menu_Main_File = new System.Windows.Forms.MenuItem();
            this.Menu_Main_File_Connect = new System.Windows.Forms.MenuItem();
            this.Menu_Main_File_Disconnect = new System.Windows.Forms.MenuItem();
            this.Menu_Main_File_Divider1 = new System.Windows.Forms.MenuItem();
            this.Menu_Main_File_ImportNZB = new System.Windows.Forms.MenuItem();
            this.Menu_Main_File_Divider2 = new System.Windows.Forms.MenuItem();
            this.Menu_Main_File_Exit = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit_AddServer = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit_EditServer = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit_DeleteServer = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit_Divider1 = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit_DeleteArticle = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Edit_DecodeArticle = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Options = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Options_Reset = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Options_ClearCache = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Options_Exit = new System.Windows.Forms.MenuItem();
            this.Menu_Main_SaveWindowStatus = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Options_Divider1 = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Options_Prefernces = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Help = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Help_Update = new System.Windows.Forms.MenuItem();
            this.Menu_Main_Help_About = new System.Windows.Forms.MenuItem();
            this.Update_Timer = new System.Windows.Forms.Timer(this.components);
            this.Main_StatusBar = new System.Windows.Forms.StatusBar();
            this.Panel_Connections = new System.Windows.Forms.Panel();
            this.lvConnections = new System.Windows.Forms.ListView();
            this.chServer = new System.Windows.Forms.ColumnHeader();
            this.chID = new System.Windows.Forms.ColumnHeader();
            this.chConnStatus = new System.Windows.Forms.ColumnHeader();
            this.chProgress = new System.Windows.Forms.ColumnHeader();
            this.chSpeed = new System.Windows.Forms.ColumnHeader();
            this.ContextMenu_Queue = new System.Windows.Forms.ContextMenu();
            this.Context_MoveUp = new System.Windows.Forms.MenuItem();
            this.Context_MoveDown = new System.Windows.Forms.MenuItem();
            this.Context_Divider2 = new System.Windows.Forms.MenuItem();
            this.Context_MoveTop = new System.Windows.Forms.MenuItem();
            this.Context_MoveBottom = new System.Windows.Forms.MenuItem();
            this.Context_Divider3 = new System.Windows.Forms.MenuItem();
            this.Context_Decode = new System.Windows.Forms.MenuItem();
            this.Context_Pause = new System.Windows.Forms.MenuItem();
            this.Context_Delete = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.Context_Prune = new System.Windows.Forms.MenuItem();
            this.Splitter_Lists = new System.Windows.Forms.Splitter();
            this.Save_Log = new System.Windows.Forms.SaveFileDialog();
            this.TabPage_Servers = new System.Windows.Forms.TabPage();
            this.lvServers = new System.Windows.Forms.ListView();
            this.chServerGroup = new System.Windows.Forms.ColumnHeader();
            this.chAddress = new System.Windows.Forms.ColumnHeader();
            this.chPort = new System.Windows.Forms.ColumnHeader();
            this.chConnections = new System.Windows.Forms.ColumnHeader();
            this.chRequiresLogin = new System.Windows.Forms.ColumnHeader();
            this.chUsername = new System.Windows.Forms.ColumnHeader();
            this.chPassword = new System.Windows.Forms.ColumnHeader();
            this.Panel_ServerButtons = new System.Windows.Forms.Panel();
            this.Button_Disconnect = new System.Windows.Forms.Button();
            this.Button_Connect = new System.Windows.Forms.Button();
            this.Button_EditServer = new System.Windows.Forms.Button();
            this.Button_DeleteServer = new System.Windows.Forms.Button();
            this.Button_AddServer = new System.Windows.Forms.Button();
            this.TabPage_Queue = new System.Windows.Forms.TabPage();
            this.lvArticles = new System.Windows.Forms.ListView();
            this.chArticle = new System.Windows.Forms.ColumnHeader();
            this.chSize = new System.Windows.Forms.ColumnHeader();
            this.chParts = new System.Windows.Forms.ColumnHeader();
            this.chStatus = new System.Windows.Forms.ColumnHeader();
            this.chDate = new System.Windows.Forms.ColumnHeader();
            this.chGroups = new System.Windows.Forms.ColumnHeader();
            this.Panel_QueueButtons = new System.Windows.Forms.Panel();
            this.ButtonDecodeIncomplete = new System.Windows.Forms.Button();
            this.setIncompleteToQueuedButton = new System.Windows.Forms.Button();
            this.Button_Bottom = new System.Windows.Forms.Button();
            this.Button_Down = new System.Windows.Forms.Button();
            this.Button_Up = new System.Windows.Forms.Button();
            this.Button_Top = new System.Windows.Forms.Button();
            this.Button_Prune = new System.Windows.Forms.Button();
            this.Button_DeleteQueue = new System.Windows.Forms.Button();
            this.Button_ImportNZB = new System.Windows.Forms.Button();
            this.TabPage_Status = new System.Windows.Forms.TabPage();
            this.List_StatusLog = new System.Windows.Forms.ListBox();
            this.Panel_LogButtons = new System.Windows.Forms.Panel();
            this.Button_ClearLog = new System.Windows.Forms.Button();
            this.Button_SaveLog = new System.Windows.Forms.Button();
            this.TabControl_Main = new System.Windows.Forms.TabControl();
            this.chSSL = new System.Windows.Forms.ColumnHeader();
            this.Panel_Connections.SuspendLayout();
            this.TabPage_Servers.SuspendLayout();
            this.Panel_ServerButtons.SuspendLayout();
            this.TabPage_Queue.SuspendLayout();
            this.Panel_QueueButtons.SuspendLayout();
            this.TabPage_Status.SuspendLayout();
            this.Panel_LogButtons.SuspendLayout();
            this.TabControl_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // OpenFile_ImportNZB
            // 
            this.OpenFile_ImportNZB.Filter = "NZB files|*.nzb";
            this.OpenFile_ImportNZB.Multiselect = true;
            this.OpenFile_ImportNZB.RestoreDirectory = true;
            this.OpenFile_ImportNZB.Title = "Import NZB file";
            // 
            // Icon_Tray
            // 
            this.Icon_Tray.ContextMenu = this.ContextMenu_Icon;
            this.Icon_Tray.Icon = ((System.Drawing.Icon)(resources.GetObject("Icon_Tray.Icon")));
            this.Icon_Tray.Text = "NZB-O-Matic";
            this.Icon_Tray.Visible = true;
            this.Icon_Tray.Click += new System.EventHandler(this.Icon_Tray_Click);
            // 
            // ContextMenu_Icon
            // 
            this.ContextMenu_Icon.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Context_Connect,
            this.Context_Disconnect,
            this.Context_Divider1,
            this.Context_Exit});
            // 
            // Context_Connect
            // 
            this.Context_Connect.Index = 0;
            this.Context_Connect.Text = "Connect";
            this.Context_Connect.Click += new System.EventHandler(this.Context_Connect_Click);
            // 
            // Context_Disconnect
            // 
            this.Context_Disconnect.Enabled = false;
            this.Context_Disconnect.Index = 1;
            this.Context_Disconnect.Text = "Disconnect";
            this.Context_Disconnect.Click += new System.EventHandler(this.Context_Disconnect_Click);
            // 
            // Context_Divider1
            // 
            this.Context_Divider1.Index = 2;
            this.Context_Divider1.Text = "-";
            // 
            // Context_Exit
            // 
            this.Context_Exit.Index = 3;
            this.Context_Exit.Text = "Exit";
            this.Context_Exit.Click += new System.EventHandler(this.Context_Exit_Click);
            // 
            // MainMenu_Main
            // 
            this.MainMenu_Main.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Menu_Main_File,
            this.Menu_Main_Edit,
            this.Menu_Main_Options,
            this.Menu_Main_Help});
            // 
            // Menu_Main_File
            // 
            this.Menu_Main_File.Index = 0;
            this.Menu_Main_File.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Menu_Main_File_Connect,
            this.Menu_Main_File_Disconnect,
            this.Menu_Main_File_Divider1,
            this.Menu_Main_File_ImportNZB,
            this.Menu_Main_File_Divider2,
            this.Menu_Main_File_Exit});
            this.Menu_Main_File.Text = "File";
            // 
            // Menu_Main_File_Connect
            // 
            this.Menu_Main_File_Connect.Index = 0;
            this.Menu_Main_File_Connect.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftC;
            this.Menu_Main_File_Connect.Text = "Connect";
            this.Menu_Main_File_Connect.Click += new System.EventHandler(this.Menu_Main_File_Connect_Click);
            // 
            // Menu_Main_File_Disconnect
            // 
            this.Menu_Main_File_Disconnect.Enabled = false;
            this.Menu_Main_File_Disconnect.Index = 1;
            this.Menu_Main_File_Disconnect.Shortcut = System.Windows.Forms.Shortcut.CtrlShiftD;
            this.Menu_Main_File_Disconnect.Text = "Disconnect";
            this.Menu_Main_File_Disconnect.Click += new System.EventHandler(this.Menu_Main_File_Disconnect_Click);
            // 
            // Menu_Main_File_Divider1
            // 
            this.Menu_Main_File_Divider1.Index = 2;
            this.Menu_Main_File_Divider1.Text = "-";
            // 
            // Menu_Main_File_ImportNZB
            // 
            this.Menu_Main_File_ImportNZB.Index = 3;
            this.Menu_Main_File_ImportNZB.Text = "Import nzb...";
            this.Menu_Main_File_ImportNZB.Click += new System.EventHandler(this.Menu_Main_File_ImportNZB_Click);
            // 
            // Menu_Main_File_Divider2
            // 
            this.Menu_Main_File_Divider2.Index = 4;
            this.Menu_Main_File_Divider2.Text = "-";
            // 
            // Menu_Main_File_Exit
            // 
            this.Menu_Main_File_Exit.Index = 5;
            this.Menu_Main_File_Exit.Text = "Exit";
            this.Menu_Main_File_Exit.Click += new System.EventHandler(this.Menu_Main_File_Exit_Click);
            // 
            // Menu_Main_Edit
            // 
            this.Menu_Main_Edit.Index = 1;
            this.Menu_Main_Edit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Menu_Main_Edit_AddServer,
            this.Menu_Main_Edit_EditServer,
            this.Menu_Main_Edit_DeleteServer,
            this.Menu_Main_Edit_Divider1,
            this.Menu_Main_Edit_DeleteArticle,
            this.Menu_Main_Edit_DecodeArticle});
            this.Menu_Main_Edit.Text = "Edit";
            // 
            // Menu_Main_Edit_AddServer
            // 
            this.Menu_Main_Edit_AddServer.Index = 0;
            this.Menu_Main_Edit_AddServer.Text = "Add Server";
            this.Menu_Main_Edit_AddServer.Click += new System.EventHandler(this.Menu_Main_Edit_AddServer_Click);
            // 
            // Menu_Main_Edit_EditServer
            // 
            this.Menu_Main_Edit_EditServer.Index = 1;
            this.Menu_Main_Edit_EditServer.Text = "Edit Server";
            this.Menu_Main_Edit_EditServer.Click += new System.EventHandler(this.Menu_Main_Edit_EditServer_Click);
            // 
            // Menu_Main_Edit_DeleteServer
            // 
            this.Menu_Main_Edit_DeleteServer.Index = 2;
            this.Menu_Main_Edit_DeleteServer.Text = "Delete Server";
            this.Menu_Main_Edit_DeleteServer.Click += new System.EventHandler(this.Menu_Main_Edit_DeleteServer_Click);
            // 
            // Menu_Main_Edit_Divider1
            // 
            this.Menu_Main_Edit_Divider1.Index = 3;
            this.Menu_Main_Edit_Divider1.Text = "-";
            // 
            // Menu_Main_Edit_DeleteArticle
            // 
            this.Menu_Main_Edit_DeleteArticle.Index = 4;
            this.Menu_Main_Edit_DeleteArticle.Text = "Delete Article";
            this.Menu_Main_Edit_DeleteArticle.Click += new System.EventHandler(this.Menu_Main_Edit_DeleteArticle_Click);
            // 
            // Menu_Main_Edit_DecodeArticle
            // 
            this.Menu_Main_Edit_DecodeArticle.Index = 5;
            this.Menu_Main_Edit_DecodeArticle.Text = "Decode Article";
            this.Menu_Main_Edit_DecodeArticle.Click += new System.EventHandler(this.Menu_Main_Edit_DecodeArticle_Click);
            // 
            // Menu_Main_Options
            // 
            this.Menu_Main_Options.Index = 2;
            this.Menu_Main_Options.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Menu_Main_Options_Reset,
            this.Menu_Main_Options_ClearCache,
            this.Menu_Main_Options_Exit,
            this.Menu_Main_SaveWindowStatus,
            this.Menu_Main_Options_Divider1,
            this.Menu_Main_Options_Prefernces});
            this.Menu_Main_Options.Text = "Options";
            // 
            // Menu_Main_Options_Reset
            // 
            this.Menu_Main_Options_Reset.Index = 0;
            this.Menu_Main_Options_Reset.Text = "Reset Download Total";
            this.Menu_Main_Options_Reset.Click += new System.EventHandler(this.Menu_Main_Options_Reset_Click);
            // 
            // Menu_Main_Options_ClearCache
            // 
            this.Menu_Main_Options_ClearCache.Index = 1;
            this.Menu_Main_Options_ClearCache.Text = "Empty Cache";
            this.Menu_Main_Options_ClearCache.Click += new System.EventHandler(this.Menu_Main_Options_ClearCache_Click);
            // 
            // Menu_Main_Options_Exit
            // 
            this.Menu_Main_Options_Exit.Index = 2;
            this.Menu_Main_Options_Exit.Text = "Exit on completion";
            this.Menu_Main_Options_Exit.Click += new System.EventHandler(this.Menu_Main_Options_Exit_Click);
            // 
            // Menu_Main_SaveWindowStatus
            // 
            this.Menu_Main_SaveWindowStatus.Checked = true;
            this.Menu_Main_SaveWindowStatus.Index = 3;
            this.Menu_Main_SaveWindowStatus.Text = "Save window status";
            this.Menu_Main_SaveWindowStatus.Click += new System.EventHandler(this.Menu_Main_SaveWindowStatus_Click);
            // 
            // Menu_Main_Options_Divider1
            // 
            this.Menu_Main_Options_Divider1.Index = 4;
            this.Menu_Main_Options_Divider1.Text = "-";
            // 
            // Menu_Main_Options_Prefernces
            // 
            this.Menu_Main_Options_Prefernces.Index = 5;
            this.Menu_Main_Options_Prefernces.Text = "Preferences";
            this.Menu_Main_Options_Prefernces.Click += new System.EventHandler(this.Menu_Main_Options_Prefernces_Click);
            // 
            // Menu_Main_Help
            // 
            this.Menu_Main_Help.Index = 3;
            this.Menu_Main_Help.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Menu_Main_Help_Update,
            this.Menu_Main_Help_About});
            this.Menu_Main_Help.Text = "Help";
            // 
            // Menu_Main_Help_Update
            // 
            this.Menu_Main_Help_Update.Index = 0;
            this.Menu_Main_Help_Update.Text = "Update";
            this.Menu_Main_Help_Update.Click += new System.EventHandler(this.Menu_Main_Help_Update_Click);
            // 
            // Menu_Main_Help_About
            // 
            this.Menu_Main_Help_About.Index = 1;
            this.Menu_Main_Help_About.Text = "About";
            this.Menu_Main_Help_About.Click += new System.EventHandler(this.Menu_Main_Help_About_Click);
            // 
            // Update_Timer
            // 
            this.Update_Timer.Enabled = true;
            this.Update_Timer.Interval = 500;
            this.Update_Timer.Tick += new System.EventHandler(this.Update_Timer_Tick);
            // 
            // Main_StatusBar
            // 
            this.Main_StatusBar.Location = new System.Drawing.Point(0, 429);
            this.Main_StatusBar.Name = "Main_StatusBar";
            this.Main_StatusBar.ShowPanels = true;
            this.Main_StatusBar.Size = new System.Drawing.Size(542, 16);
            this.Main_StatusBar.TabIndex = 7;
            this.Main_StatusBar.Text = "StatusBar";
            // 
            // Panel_Connections
            // 
            this.Panel_Connections.Controls.Add(this.lvConnections);
            this.Panel_Connections.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel_Connections.Location = new System.Drawing.Point(0, 329);
            this.Panel_Connections.Name = "Panel_Connections";
            this.Panel_Connections.Size = new System.Drawing.Size(542, 100);
            this.Panel_Connections.TabIndex = 12;
            // 
            // lvConnections
            // 
            this.lvConnections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chServer,
            this.chID,
            this.chConnStatus,
            this.chProgress,
            this.chSpeed});
            this.lvConnections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvConnections.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvConnections.Location = new System.Drawing.Point(0, 0);
            this.lvConnections.Name = "lvConnections";
            this.lvConnections.Size = new System.Drawing.Size(542, 100);
            this.lvConnections.TabIndex = 11;
            this.lvConnections.UseCompatibleStateImageBehavior = false;
            this.lvConnections.View = System.Windows.Forms.View.Details;
            // 
            // chServer
            // 
            this.chServer.Text = "Server";
            this.chServer.Width = 124;
            // 
            // chID
            // 
            this.chID.Text = "#";
            this.chID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.chID.Width = 23;
            // 
            // chConnStatus
            // 
            this.chConnStatus.Text = "Status";
            this.chConnStatus.Width = 161;
            // 
            // chProgress
            // 
            this.chProgress.Text = "Progress";
            this.chProgress.Width = 124;
            // 
            // chSpeed
            // 
            this.chSpeed.Text = "Speed";
            // 
            // ContextMenu_Queue
            // 
            this.ContextMenu_Queue.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.Context_MoveUp,
            this.Context_MoveDown,
            this.Context_Divider2,
            this.Context_MoveTop,
            this.Context_MoveBottom,
            this.Context_Divider3,
            this.Context_Decode,
            this.Context_Pause,
            this.Context_Delete,
            this.menuItem2,
            this.Context_Prune});
            // 
            // Context_MoveUp
            // 
            this.Context_MoveUp.Index = 0;
            this.Context_MoveUp.Text = "Move Up";
            this.Context_MoveUp.Click += new System.EventHandler(this.Context_MoveUp_Click);
            // 
            // Context_MoveDown
            // 
            this.Context_MoveDown.Index = 1;
            this.Context_MoveDown.Text = "Move Down";
            this.Context_MoveDown.Click += new System.EventHandler(this.Context_MoveDown_Click);
            // 
            // Context_Divider2
            // 
            this.Context_Divider2.Index = 2;
            this.Context_Divider2.Text = "-";
            // 
            // Context_MoveTop
            // 
            this.Context_MoveTop.Index = 3;
            this.Context_MoveTop.Text = "Move to Top";
            this.Context_MoveTop.Click += new System.EventHandler(this.Context_MoveTop_Click);
            // 
            // Context_MoveBottom
            // 
            this.Context_MoveBottom.Index = 4;
            this.Context_MoveBottom.Text = "Move to Bottom";
            this.Context_MoveBottom.Click += new System.EventHandler(this.Context_MoveBottom_Click);
            // 
            // Context_Divider3
            // 
            this.Context_Divider3.Index = 5;
            this.Context_Divider3.Text = "-";
            // 
            // Context_Decode
            // 
            this.Context_Decode.Index = 6;
            this.Context_Decode.Text = "Decode";
            this.Context_Decode.Click += new System.EventHandler(this.Context_Decode_Click);
            // 
            // Context_Pause
            // 
            this.Context_Pause.Index = 7;
            this.Context_Pause.Text = "Pause";
            this.Context_Pause.Click += new System.EventHandler(this.Context_Pause_Click_1);
            // 
            // Context_Delete
            // 
            this.Context_Delete.Index = 8;
            this.Context_Delete.Text = "Delete";
            this.Context_Delete.Click += new System.EventHandler(this.Context_Delete_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 9;
            this.menuItem2.Text = "-";
            // 
            // Context_Prune
            // 
            this.Context_Prune.Index = 10;
            this.Context_Prune.Text = "Prune";
            this.Context_Prune.Click += new System.EventHandler(this.Context_Prune_Click);
            // 
            // Splitter_Lists
            // 
            this.Splitter_Lists.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Splitter_Lists.Location = new System.Drawing.Point(0, 326);
            this.Splitter_Lists.Name = "Splitter_Lists";
            this.Splitter_Lists.Size = new System.Drawing.Size(542, 3);
            this.Splitter_Lists.TabIndex = 14;
            this.Splitter_Lists.TabStop = false;
            // 
            // Save_Log
            // 
            this.Save_Log.Filter = "(Text files)|*.txt";
            // 
            // TabPage_Servers
            // 
            this.TabPage_Servers.Controls.Add(this.lvServers);
            this.TabPage_Servers.Controls.Add(this.Panel_ServerButtons);
            this.TabPage_Servers.Location = new System.Drawing.Point(4, 22);
            this.TabPage_Servers.Name = "TabPage_Servers";
            this.TabPage_Servers.Size = new System.Drawing.Size(534, 303);
            this.TabPage_Servers.TabIndex = 0;
            this.TabPage_Servers.Text = "Usenet Servers";
            // 
            // lvServers
            // 
            this.lvServers.CheckBoxes = true;
            this.lvServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chServerGroup,
            this.chAddress,
            this.chPort,
            this.chConnections,
            this.chRequiresLogin,
            this.chUsername,
            this.chPassword,
            this.chSSL});
            this.lvServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvServers.FullRowSelect = true;
            this.lvServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvServers.Location = new System.Drawing.Point(0, 32);
            this.lvServers.MultiSelect = false;
            this.lvServers.Name = "lvServers";
            this.lvServers.Size = new System.Drawing.Size(534, 271);
            this.lvServers.TabIndex = 3;
            this.lvServers.UseCompatibleStateImageBehavior = false;
            this.lvServers.View = System.Windows.Forms.View.Details;
            this.lvServers.ItemActivate += new System.EventHandler(this.lvServers_ItemActivate);
            this.lvServers.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvServers_ItemCheck);
            this.lvServers.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvServers_KeyDown);
            // 
            // chServerGroup
            // 
            this.chServerGroup.Text = "Group";
            this.chServerGroup.Width = 42;
            // 
            // chAddress
            // 
            this.chAddress.Text = "Address";
            this.chAddress.Width = 118;
            // 
            // chPort
            // 
            this.chPort.Text = "Port";
            this.chPort.Width = 32;
            // 
            // chConnections
            // 
            this.chConnections.Text = "Connections";
            this.chConnections.Width = 73;
            // 
            // chRequiresLogin
            // 
            this.chRequiresLogin.Text = "Requires Login";
            this.chRequiresLogin.Width = 85;
            // 
            // chUsername
            // 
            this.chUsername.Text = "Username";
            // 
            // chPassword
            // 
            this.chPassword.Text = "Password";
            // 
            // Panel_ServerButtons
            // 
            this.Panel_ServerButtons.BackColor = System.Drawing.SystemColors.Control;
            this.Panel_ServerButtons.Controls.Add(this.Button_Disconnect);
            this.Panel_ServerButtons.Controls.Add(this.Button_Connect);
            this.Panel_ServerButtons.Controls.Add(this.Button_EditServer);
            this.Panel_ServerButtons.Controls.Add(this.Button_DeleteServer);
            this.Panel_ServerButtons.Controls.Add(this.Button_AddServer);
            this.Panel_ServerButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.Panel_ServerButtons.Location = new System.Drawing.Point(0, 0);
            this.Panel_ServerButtons.Name = "Panel_ServerButtons";
            this.Panel_ServerButtons.Size = new System.Drawing.Size(534, 32);
            this.Panel_ServerButtons.TabIndex = 2;
            // 
            // Button_Disconnect
            // 
            this.Button_Disconnect.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Disconnect.Enabled = false;
            this.Button_Disconnect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Disconnect.Location = new System.Drawing.Point(456, 4);
            this.Button_Disconnect.Name = "Button_Disconnect";
            this.Button_Disconnect.Size = new System.Drawing.Size(75, 23);
            this.Button_Disconnect.TabIndex = 9;
            this.Button_Disconnect.Text = "Disconnect";
            this.Button_Disconnect.UseVisualStyleBackColor = false;
            this.Button_Disconnect.Click += new System.EventHandler(this.Button_Disconnect_Click);
            // 
            // Button_Connect
            // 
            this.Button_Connect.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Connect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Connect.Location = new System.Drawing.Point(376, 4);
            this.Button_Connect.Name = "Button_Connect";
            this.Button_Connect.Size = new System.Drawing.Size(75, 23);
            this.Button_Connect.TabIndex = 8;
            this.Button_Connect.Text = "Connect";
            this.Button_Connect.UseVisualStyleBackColor = false;
            this.Button_Connect.Click += new System.EventHandler(this.Button_Connect_Click);
            // 
            // Button_EditServer
            // 
            this.Button_EditServer.BackColor = System.Drawing.SystemColors.Control;
            this.Button_EditServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_EditServer.Location = new System.Drawing.Point(88, 4);
            this.Button_EditServer.Name = "Button_EditServer";
            this.Button_EditServer.Size = new System.Drawing.Size(75, 23);
            this.Button_EditServer.TabIndex = 6;
            this.Button_EditServer.Text = "Edit Server";
            this.Button_EditServer.UseVisualStyleBackColor = false;
            this.Button_EditServer.Click += new System.EventHandler(this.Button_EditServer_Click);
            // 
            // Button_DeleteServer
            // 
            this.Button_DeleteServer.BackColor = System.Drawing.SystemColors.Control;
            this.Button_DeleteServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_DeleteServer.Location = new System.Drawing.Point(168, 4);
            this.Button_DeleteServer.Name = "Button_DeleteServer";
            this.Button_DeleteServer.Size = new System.Drawing.Size(75, 23);
            this.Button_DeleteServer.TabIndex = 7;
            this.Button_DeleteServer.Text = "Delete";
            this.Button_DeleteServer.UseVisualStyleBackColor = false;
            this.Button_DeleteServer.Click += new System.EventHandler(this.Button_DeleteServer_Click);
            // 
            // Button_AddServer
            // 
            this.Button_AddServer.BackColor = System.Drawing.SystemColors.Control;
            this.Button_AddServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_AddServer.Location = new System.Drawing.Point(7, 4);
            this.Button_AddServer.Name = "Button_AddServer";
            this.Button_AddServer.Size = new System.Drawing.Size(75, 23);
            this.Button_AddServer.TabIndex = 5;
            this.Button_AddServer.Text = "Add Server";
            this.Button_AddServer.UseVisualStyleBackColor = false;
            this.Button_AddServer.Click += new System.EventHandler(this.Button_AddServer_Click);
            // 
            // TabPage_Queue
            // 
            this.TabPage_Queue.Controls.Add(this.lvArticles);
            this.TabPage_Queue.Controls.Add(this.Panel_QueueButtons);
            this.TabPage_Queue.Location = new System.Drawing.Point(4, 22);
            this.TabPage_Queue.Name = "TabPage_Queue";
            this.TabPage_Queue.Size = new System.Drawing.Size(534, 303);
            this.TabPage_Queue.TabIndex = 1;
            this.TabPage_Queue.Text = "Transfer Queue";
            this.TabPage_Queue.Visible = false;
            // 
            // lvArticles
            // 
            this.lvArticles.AllowColumnReorder = true;
            this.lvArticles.AllowDrop = true;
            this.lvArticles.BackColor = System.Drawing.Color.White;
            this.lvArticles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chArticle,
            this.chSize,
            this.chParts,
            this.chStatus,
            this.chDate,
            this.chGroups});
            this.lvArticles.ContextMenu = this.ContextMenu_Queue;
            this.lvArticles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvArticles.FullRowSelect = true;
            this.lvArticles.HideSelection = false;
            this.lvArticles.Location = new System.Drawing.Point(0, 32);
            this.lvArticles.Name = "lvArticles";
            this.lvArticles.Size = new System.Drawing.Size(534, 271);
            this.lvArticles.TabIndex = 2;
            this.lvArticles.UseCompatibleStateImageBehavior = false;
            this.lvArticles.View = System.Windows.Forms.View.Details;
            this.lvArticles.DragEnter += new System.Windows.Forms.DragEventHandler(this.NZBType_DragEnter);
            this.lvArticles.DragDrop += new System.Windows.Forms.DragEventHandler(this.NZBType_DragDrop);
            this.lvArticles.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvArticles_ItemCheck);
            this.lvArticles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvArticles_KeyDown);
            this.lvArticles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvArticles_ColumnClick);
            // 
            // chArticle
            // 
            this.chArticle.Text = "Subject";
            this.chArticle.Width = 163;
            // 
            // chSize
            // 
            this.chSize.Text = "Size";
            // 
            // chParts
            // 
            this.chParts.Text = "Parts";
            // 
            // chStatus
            // 
            this.chStatus.Text = "Status";
            // 
            // chDate
            // 
            this.chDate.Text = "Date";
            // 
            // chGroups
            // 
            this.chGroups.Text = "Groups";
            // 
            // Panel_QueueButtons
            // 
            this.Panel_QueueButtons.Controls.Add(this.ButtonDecodeIncomplete);
            this.Panel_QueueButtons.Controls.Add(this.setIncompleteToQueuedButton);
            this.Panel_QueueButtons.Controls.Add(this.Button_Bottom);
            this.Panel_QueueButtons.Controls.Add(this.Button_Down);
            this.Panel_QueueButtons.Controls.Add(this.Button_Up);
            this.Panel_QueueButtons.Controls.Add(this.Button_Top);
            this.Panel_QueueButtons.Controls.Add(this.Button_Prune);
            this.Panel_QueueButtons.Controls.Add(this.Button_DeleteQueue);
            this.Panel_QueueButtons.Controls.Add(this.Button_ImportNZB);
            this.Panel_QueueButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.Panel_QueueButtons.Location = new System.Drawing.Point(0, 0);
            this.Panel_QueueButtons.Name = "Panel_QueueButtons";
            this.Panel_QueueButtons.Size = new System.Drawing.Size(534, 32);
            this.Panel_QueueButtons.TabIndex = 1;
            // 
            // ButtonDecodeIncomplete
            // 
            this.ButtonDecodeIncomplete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ButtonDecodeIncomplete.Location = new System.Drawing.Point(358, 4);
            this.ButtonDecodeIncomplete.Name = "ButtonDecodeIncomplete";
            this.ButtonDecodeIncomplete.Size = new System.Drawing.Size(64, 23);
            this.ButtonDecodeIncomplete.TabIndex = 6;
            this.ButtonDecodeIncomplete.Text = "Decode";
            this.ButtonDecodeIncomplete.Click += new System.EventHandler(this.ButtonDecodeIncomplete_Click);
            // 
            // setIncompleteToQueuedButton
            // 
            this.setIncompleteToQueuedButton.BackColor = System.Drawing.SystemColors.Control;
            this.setIncompleteToQueuedButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.setIncompleteToQueuedButton.Location = new System.Drawing.Point(248, 4);
            this.setIncompleteToQueuedButton.Name = "setIncompleteToQueuedButton";
            this.setIncompleteToQueuedButton.Size = new System.Drawing.Size(104, 23);
            this.setIncompleteToQueuedButton.TabIndex = 5;
            this.setIncompleteToQueuedButton.Text = "Retry Incompletes";
            this.setIncompleteToQueuedButton.UseVisualStyleBackColor = false;
            this.setIncompleteToQueuedButton.Click += new System.EventHandler(this.setIncompleteToQueuedButton_Click);
            // 
            // Button_Bottom
            // 
            this.Button_Bottom.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Bottom.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Bottom.Image = ((System.Drawing.Image)(resources.GetObject("Button_Bottom.Image")));
            this.Button_Bottom.Location = new System.Drawing.Point(504, 4);
            this.Button_Bottom.Name = "Button_Bottom";
            this.Button_Bottom.Size = new System.Drawing.Size(23, 23);
            this.Button_Bottom.TabIndex = 10;
            this.Button_Bottom.UseVisualStyleBackColor = false;
            this.Button_Bottom.Click += new System.EventHandler(this.Context_MoveBottom_Click);
            // 
            // Button_Down
            // 
            this.Button_Down.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Down.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Down.Image = ((System.Drawing.Image)(resources.GetObject("Button_Down.Image")));
            this.Button_Down.Location = new System.Drawing.Point(480, 4);
            this.Button_Down.Name = "Button_Down";
            this.Button_Down.Size = new System.Drawing.Size(23, 23);
            this.Button_Down.TabIndex = 9;
            this.Button_Down.UseVisualStyleBackColor = false;
            this.Button_Down.Click += new System.EventHandler(this.Context_MoveDown_Click);
            // 
            // Button_Up
            // 
            this.Button_Up.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Up.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Up.Image = ((System.Drawing.Image)(resources.GetObject("Button_Up.Image")));
            this.Button_Up.Location = new System.Drawing.Point(456, 4);
            this.Button_Up.Name = "Button_Up";
            this.Button_Up.Size = new System.Drawing.Size(23, 23);
            this.Button_Up.TabIndex = 8;
            this.Button_Up.UseVisualStyleBackColor = false;
            this.Button_Up.Click += new System.EventHandler(this.Context_MoveUp_Click);
            // 
            // Button_Top
            // 
            this.Button_Top.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Top.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Top.Image = ((System.Drawing.Image)(resources.GetObject("Button_Top.Image")));
            this.Button_Top.Location = new System.Drawing.Point(432, 4);
            this.Button_Top.Name = "Button_Top";
            this.Button_Top.Size = new System.Drawing.Size(23, 23);
            this.Button_Top.TabIndex = 7;
            this.Button_Top.UseVisualStyleBackColor = false;
            this.Button_Top.Click += new System.EventHandler(this.Context_MoveTop_Click);
            // 
            // Button_Prune
            // 
            this.Button_Prune.BackColor = System.Drawing.SystemColors.Control;
            this.Button_Prune.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_Prune.Location = new System.Drawing.Point(88, 4);
            this.Button_Prune.Name = "Button_Prune";
            this.Button_Prune.Size = new System.Drawing.Size(75, 23);
            this.Button_Prune.TabIndex = 3;
            this.Button_Prune.Text = "Prune";
            this.Button_Prune.UseVisualStyleBackColor = false;
            this.Button_Prune.Click += new System.EventHandler(this.Button_Prune_Click);
            // 
            // Button_DeleteQueue
            // 
            this.Button_DeleteQueue.BackColor = System.Drawing.SystemColors.Control;
            this.Button_DeleteQueue.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_DeleteQueue.Location = new System.Drawing.Point(168, 4);
            this.Button_DeleteQueue.Name = "Button_DeleteQueue";
            this.Button_DeleteQueue.Size = new System.Drawing.Size(75, 23);
            this.Button_DeleteQueue.TabIndex = 4;
            this.Button_DeleteQueue.Text = "Delete";
            this.Button_DeleteQueue.UseVisualStyleBackColor = false;
            this.Button_DeleteQueue.Click += new System.EventHandler(this.Button_DeleteQueue_Click);
            // 
            // Button_ImportNZB
            // 
            this.Button_ImportNZB.BackColor = System.Drawing.SystemColors.Control;
            this.Button_ImportNZB.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_ImportNZB.Location = new System.Drawing.Point(7, 4);
            this.Button_ImportNZB.Name = "Button_ImportNZB";
            this.Button_ImportNZB.Size = new System.Drawing.Size(75, 23);
            this.Button_ImportNZB.TabIndex = 0;
            this.Button_ImportNZB.Text = "Import NZB ";
            this.Button_ImportNZB.UseVisualStyleBackColor = false;
            this.Button_ImportNZB.Click += new System.EventHandler(this.Button_ImportNZB_Click);
            // 
            // TabPage_Status
            // 
            this.TabPage_Status.Controls.Add(this.List_StatusLog);
            this.TabPage_Status.Controls.Add(this.Panel_LogButtons);
            this.TabPage_Status.Location = new System.Drawing.Point(4, 22);
            this.TabPage_Status.Name = "TabPage_Status";
            this.TabPage_Status.Size = new System.Drawing.Size(534, 303);
            this.TabPage_Status.TabIndex = 2;
            this.TabPage_Status.Text = "Status Log";
            this.TabPage_Status.Visible = false;
            // 
            // List_StatusLog
            // 
            this.List_StatusLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.List_StatusLog.IntegralHeight = false;
            this.List_StatusLog.Location = new System.Drawing.Point(0, 32);
            this.List_StatusLog.Name = "List_StatusLog";
            this.List_StatusLog.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.List_StatusLog.Size = new System.Drawing.Size(534, 271);
            this.List_StatusLog.TabIndex = 4;
            // 
            // Panel_LogButtons
            // 
            this.Panel_LogButtons.Controls.Add(this.Button_ClearLog);
            this.Panel_LogButtons.Controls.Add(this.Button_SaveLog);
            this.Panel_LogButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this.Panel_LogButtons.Location = new System.Drawing.Point(0, 0);
            this.Panel_LogButtons.Name = "Panel_LogButtons";
            this.Panel_LogButtons.Size = new System.Drawing.Size(534, 32);
            this.Panel_LogButtons.TabIndex = 3;
            // 
            // Button_ClearLog
            // 
            this.Button_ClearLog.BackColor = System.Drawing.SystemColors.Control;
            this.Button_ClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_ClearLog.Location = new System.Drawing.Point(88, 4);
            this.Button_ClearLog.Name = "Button_ClearLog";
            this.Button_ClearLog.Size = new System.Drawing.Size(75, 23);
            this.Button_ClearLog.TabIndex = 4;
            this.Button_ClearLog.Text = "Clear";
            this.Button_ClearLog.UseVisualStyleBackColor = false;
            this.Button_ClearLog.Click += new System.EventHandler(this.Button_ClearLog_Click);
            // 
            // Button_SaveLog
            // 
            this.Button_SaveLog.BackColor = System.Drawing.SystemColors.Control;
            this.Button_SaveLog.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Button_SaveLog.Location = new System.Drawing.Point(7, 4);
            this.Button_SaveLog.Name = "Button_SaveLog";
            this.Button_SaveLog.Size = new System.Drawing.Size(75, 23);
            this.Button_SaveLog.TabIndex = 0;
            this.Button_SaveLog.Text = "Save";
            this.Button_SaveLog.UseVisualStyleBackColor = false;
            this.Button_SaveLog.Click += new System.EventHandler(this.Button_SaveLog_Click);
            // 
            // TabControl_Main
            // 
            this.TabControl_Main.Controls.Add(this.TabPage_Servers);
            this.TabControl_Main.Controls.Add(this.TabPage_Queue);
            this.TabControl_Main.Controls.Add(this.TabPage_Status);
            this.TabControl_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl_Main.ItemSize = new System.Drawing.Size(85, 18);
            this.TabControl_Main.Location = new System.Drawing.Point(0, 0);
            this.TabControl_Main.Name = "TabControl_Main";
            this.TabControl_Main.Padding = new System.Drawing.Point(0, 0);
            this.TabControl_Main.SelectedIndex = 0;
            this.TabControl_Main.Size = new System.Drawing.Size(542, 329);
            this.TabControl_Main.TabIndex = 13;
            this.TabControl_Main.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // chSSL
            // 
            this.chSSL.Text = "SSL";
            this.chSSL.Width = 40;
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(542, 445);
            this.Controls.Add(this.Splitter_Lists);
            this.Controls.Add(this.TabControl_Main);
            this.Controls.Add(this.Panel_Connections);
            this.Controls.Add(this.Main_StatusBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.MainMenu_Main;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NZB-O-Matic";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.NZBType_DragDrop);
            this.Closed += new System.EventHandler(this.frmMain_Closed);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.NZBType_DragEnter);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Panel_Connections.ResumeLayout(false);
            this.TabPage_Servers.ResumeLayout(false);
            this.Panel_ServerButtons.ResumeLayout(false);
            this.TabPage_Queue.ResumeLayout(false);
            this.Panel_QueueButtons.ResumeLayout(false);
            this.TabPage_Status.ResumeLayout(false);
            this.Panel_LogButtons.ResumeLayout(false);
            this.TabControl_Main.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Log Methods
		public static void LogWriteInfo(object toLog)
		{
			LogWriteLine("[i] - " + System.DateTime.Now.ToString() + " - " + toLog.ToString());
		}

		public static void LogWriteError(object toLog)
		{
			LogWriteLine("[e] - " + System.DateTime.Now.ToString() + " - " + toLog.ToString());
		}


		private delegate void UIDelegate(object toLog);
		private static UIDelegate uiDelegate = new UIDelegate(LogWriteLine);

		private static void LogWriteLine(object toLog)
		{
			if (Global.m_AllowLogging)
			{
				if (Global.m_StatusLog.InvokeRequired)
				{
					object[] parms = { toLog };
					Global.m_StatusLog.Invoke(uiDelegate, parms);
				}
				else
				{
					lock (Global.m_StatusLog)
					{
						if (Global.m_StatusLog != null)
						{
							Global.m_StatusLog.Items.Insert(0, toLog);
							while (Global.m_StatusLog.Items.Count > 200)
								Global.m_StatusLog.Items.RemoveAt(200);
						}
					}
				}
			}
		}

		public static void SetLog(System.Windows.Forms.ListBox toSet)
		{
			Global.m_StatusLog = toSet;
			Global.m_StatusLog.Items.Add("Status log opened: " + System.DateTime.Now.ToString());
			Global.m_AllowLogging = true;
		}
		#endregion 
	
		#region Server Actions
		private void lvServers_ItemActivate(object sender, System.EventArgs e)
		{
			EditServer();
		}

		private void lvServers_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == System.Windows.Forms.Keys.Delete)
			{
				DeleteServer();
			}
		}

		private void AddServer()
		{
			Form_Server frmServer = new Form_Server();
			frmServer.Closing += new CancelEventHandler(frmServer_Add_Closing);
			frmServer.ShowDialog(this);
			frmServer.Close();
			frmServer.Dispose();
		}

		private void EditServer()
		{
			if(!Global.m_Connected)
			{
				if(lvServers.SelectedItems.Count > 0)
				{
					Form_Server frmServer = new Form_Server();
					frmServer.SetServer(((Server)lvServers.SelectedItems[0].Tag));
					frmServer.Closing += new CancelEventHandler(frmServer_Edit_Closing);
					frmServer.ShowDialog(this);
					frmServer.Close();
					frmServer.Dispose();
				}
			}
			UpdateServers();
		}

		private void DeleteServer()
		{
			for(int i = 0; i < lvServers.SelectedItems.Count; i++)
			{
				((Server)lvServers.SelectedItems[i].Tag).ServerGroup.RemoveServer(((Server)lvServers.SelectedItems[i].Tag));
			}
			UpdateServers();
		}

		private void lvServers_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if(e.CurrentValue == CheckState.Checked)
			{
				ServerManager.DisableServer(((Server)lvServers.Items[e.Index].Tag));
				m_ServerManager.RebuildQueue();
			}
			if(e.CurrentValue == CheckState.Unchecked)
			{
				ServerManager.EnableServer(((Server)lvServers.Items[e.Index].Tag));
				m_ServerManager.RebuildQueue();
			}

			UpdateServers();
		}


		private void UpdateServers()
		{
			if(m_UpdatingServers)
				return;
			
			m_UpdatingServers = true;

			lvServers.Items.Clear();
			RefreshServers();

			ResetConnectionStatus();

			m_ServerManager.InitializeQueues();
			m_ServerManager.FillQueues();

			m_UpdatingServers = false;
		}

		private void RefreshServers()
		{
			if(m_ServerManager.m_ServerGroups != null)
				foreach(ServerGroup servergroup in m_ServerManager.m_ServerGroups)
					if(servergroup.Servers != null)
						foreach(Server server in servergroup.Servers)
						{
							server.UpdateStatus();
							if(!lvServers.Items.Contains(server.StatusItem))
								lvServers.Items.Add(server.StatusItem);
						}
		}

		#endregion

		#region Menu Actions
		private void Menu_Main_Edit_AddServer_Click(object sender, System.EventArgs e)
		{
			AddServer();
		}

		private void Menu_Main_Edit_EditServer_Click(object sender, System.EventArgs e)
		{
			EditServer();
		}

		private void Menu_Main_Edit_DeleteServer_Click(object sender, System.EventArgs e)
		{
			DeleteServer();
		}

		private void Menu_Main_Edit_DeleteArticle_Click(object sender, System.EventArgs e)
		{
			DeleteQueueItems();
		}

		private void Menu_Main_Edit_DecodeArticle_Click(object sender, System.EventArgs e)
		{
			ForceDecode();
		}

		private void Menu_Main_Help_Update_Click(object sender, System.EventArgs e)
		{
			Global.Engine.Update = true;
			Global.Engine.Restart = true;
			this.Close();
		}

		private void Menu_Main_SaveWindowStatus_Click(object sender, System.EventArgs e)
		{
			Menu_Main_SaveWindowStatus.Checked = !Menu_Main_SaveWindowStatus.Checked;
		}


		private void Menu_Main_Options_ClearCache_Click(object sender, System.EventArgs e)
		{
			if (Global.m_Connected)
			{
				System.Windows.Forms.MessageBox.Show(
					"The cache can only be emptied when you are disconnected from the servers.  Please disconnect and try again.",
					"Disconnect first",
					System.Windows.Forms.MessageBoxButtons.OK,
					System.Windows.Forms.MessageBoxIcon.Exclamation,
					System.Windows.Forms.MessageBoxDefaultButton.Button1);
			}
			else
			{
				if (System.Windows.Forms.MessageBox.Show(
					"If you empty the cache all partially downloaded files will have all their parts removed.  It is recommended that you only empty the cache when your entire queue is empty.  Click Cancel to abort emptying the cache at this time.",
					"Empty Cache Warning",
					System.Windows.Forms.MessageBoxButtons.OKCancel,
					System.Windows.Forms.MessageBoxIcon.Warning,
					System.Windows.Forms.MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.OK)
				{
					System.IO.Directory.Delete(Global.m_CurrentDirectory + "Cache", true);
					System.IO.Directory.CreateDirectory( Global.m_CurrentDirectory + "Cache");
				}
			}
		}

		private void Menu_Main_File_Exit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void Menu_Main_File_Connect_Click(object sender, System.EventArgs e)
		{
			Connect();
		}

		private void Menu_Main_File_Disconnect_Click(object sender, System.EventArgs e)
		{
			Disconnect(false);
		}

		private void Menu_Main_File_ImportNZB_Click(object sender, System.EventArgs e)
		{
			OpenImportNZB();
		}
		private void Menu_Main_Options_Reset_Click(object sender, System.EventArgs e)
		{
			m_TotalDownloadOffset = -(((double)m_ServerManager.LifetimeBytesReceived / 1024) / 1024);
		}

		private void Menu_Main_Options_Exit_Click(object sender, System.EventArgs e)
		{
			if(Global.m_Connected)
			{
				Menu_Main_Options_Exit.Checked = !Menu_Main_Options_Exit.Checked;
				Global.m_ExitComplete = Menu_Main_Options_Exit.Checked;
			}
		}

		#endregion

		#region Button Clicks

		private void ButtonDecodeIncomplete_Click(object sender, System.EventArgs e)
		{
			ForceDecode();
		}
		
		private void setIncompleteToQueuedButton_Click(object sender, System.EventArgs e)
		{
			this.SetIncompleteToQueued();
		}

		private void Button_ImportNZB_Click(object sender, System.EventArgs e)
		{
			OpenImportNZB();
		}

		private void Button_Connect_Click(object sender, System.EventArgs e)
		{
			Connect();
		}

		private void Button_Disconnect_Click(object sender, System.EventArgs e)
		{
			Disconnect(false);
		}
		private void Button_Prune_Click(object sender, System.EventArgs e)
		{
			this.lvArticles.BeginUpdate();
			m_ServerManager.PruneQueue();
			this.lvArticles.EndUpdate();
		}

		private void Button_AddServer_Click(object sender, System.EventArgs e)
		{
			AddServer();
		}
		private void Button_DeleteQueue_Click(object sender, System.EventArgs e)
		{
			DeleteQueueItems();
		}
		private void Button_DeleteServer_Click(object sender, System.EventArgs e)
		{
			DeleteServer();
		}
		private void Button_ClearLog_Click(object sender, System.EventArgs e)
		{
			this.List_StatusLog.Items.Clear();
			this.List_StatusLog.Refresh();
		}

		private void Button_SaveLog_Click(object sender, System.EventArgs e)
		{
			if(Save_Log.ShowDialog() != DialogResult.OK)
				return;

			using(System.IO.StreamWriter sw = new System.IO.StreamWriter(Save_Log.FileName))
			{
				foreach(string str in List_StatusLog.Items)
					sw.WriteLine(str);
			}
		}
		private void Button_EditServer_Click(object sender, System.EventArgs e)
		{
			EditServer();
		}
		private void Context_MoveUp_Click(object sender, System.EventArgs e)
		{
			m_Sorted = "none";
			lock(lvArticles)
			{
				ArrayList toMove = new ArrayList();
				foreach(int i in lvArticles.SelectedIndices)
				{
					toMove.Add((Article)lvArticles.Items[i].Tag);
				}
				m_ServerManager.MoveArticlesUp(toMove);
			}
			RebuildQueue();
		}

		private void Context_MoveDown_Click(object sender, System.EventArgs e)
		{
			m_Sorted = "none";
			lock(lvArticles)
			{
				ArrayList toMove = new ArrayList();
				foreach(int i in lvArticles.SelectedIndices)
				{
					toMove.Add((Article)lvArticles.Items[i].Tag);
				}
				m_ServerManager.MoveArticlesDown(toMove);
			}
			RebuildQueue();
		}

		private void Context_MoveTop_Click(object sender, System.EventArgs e)
		{
			m_Sorted = "none";
			lock(lvArticles)
			{
				ArrayList toMove = new ArrayList();
				foreach(int i in lvArticles.SelectedIndices)
				{
					toMove.Add((Article)lvArticles.Items[i].Tag);
				}
				m_ServerManager.MoveArticlesTop(toMove);
			}
			RebuildQueue();
		}

		private void Context_MoveBottom_Click(object sender, System.EventArgs e)
		{
			m_Sorted = "none";
			lock(lvArticles)
			{
				ArrayList toMove = new ArrayList();
				foreach(int i in lvArticles.SelectedIndices)
				{
					toMove.Add((Article)lvArticles.Items[i].Tag);
				}
				m_ServerManager.MoveArticlesBottom(toMove);
			}
			RebuildQueue();
		}


		#endregion

		#region Show/Hide Window
		private void ShowWindow()
		{
			if(!m_ChangingState)
			{
				m_ChangingState = true;
				this.Show();
				if (this.WindowState == FormWindowState.Minimized)
				{
					this.WindowState = FormWindowState.Normal;
				}
				SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
				this.Refresh();
				m_ChangingState = false;
			}
		}

		private void HideWindow()
		{
			if(!m_ChangingState)
			{
				m_ChangingState = true;
				if(this.WindowState != FormWindowState.Minimized)
					this.WindowState = FormWindowState.Minimized;
				this.Hide();
				m_ChangingState = false;
			}
		}

		#endregion 

		#region Private Methods
		private void SetIncompleteToQueued()
		{
			lock (lvArticles)
			{
				foreach (ListViewItem lvItem in lvArticles.SelectedItems)
				{
					Article article = (Article)lvItem.Tag;
					if (article.Status == ArticleStatus.Incomplete)
					{
						article.Status = ArticleStatus.Queued;
						m_ServerManager.RebuildQueue();
					}
				}
			}
		}

		private void UpdateArticles()
		{
			this.lvArticles.BeginUpdate();
			lvArticles.Items.Clear();
			//RefreshArticles();
			if(m_ServerManager.m_Articles != null)
				foreach(Article article in m_ServerManager.m_Articles)
				{
					article.UpdateStatus();
					if(!lvArticles.Items.Contains(article.StatusItem))
						lvArticles.Items.Add(article.StatusItem);
				}	
			this.lvArticles.EndUpdate();
		}

		private void RefreshArticles()
		{
			/*if(m_ServerManager.m_Articles != null)
				foreach(Article article in m_ServerManager.m_Articles)
				{
					article.UpdateStatus();
					if(!lvArticles.Items.Contains(article.StatusItem))
						lvArticles.Items.Add(article.StatusItem);
				}*/
		}

		private void XmlAddElement( System.Xml.XmlNode node, string name, string value)
		{
			System.Xml.XmlDocument XmlDoc = node.OwnerDocument;
			System.Xml.XmlElement XmlElement = XmlDoc.CreateElement(name);
			XmlElement.InnerText = value;
			node.AppendChild(XmlElement);
		}

		private void XmlAddAttr( System.Xml.XmlNode node, string name, string value)
		{
			System.Xml.XmlDocument XmlDoc = node.OwnerDocument;
			System.Xml.XmlAttribute XmlAttr = XmlDoc.CreateAttribute(name);
			XmlAttr.Value = value;
			node.Attributes.Append(XmlAttr);
		}

		private void frmMain_Load(object sender, System.EventArgs e)
		{
			try
			{
				Global.m_DataDirectory = Environment.GetEnvironmentVariable("appdata") + @"\nomp\";
				if (Global.m_DataDirectory == @"\nomp\") throw new ArgumentNullException();
			}
			catch
			{
				Global.m_DataDirectory = Global.m_CurrentDirectory;
			}
			Global.m_CacheDirectory = Global.m_DataDirectory + @"\Cache\";
			
			try
			{
				RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders");
				string str = (string)rk.GetValue("Personal");
				if (str == null ) throw new ArgumentNullException();
				Global.m_DownloadDirectory = str + @"\download\";
			}
			catch
			{
				Global.m_DownloadDirectory = Global.m_CurrentDirectory + @"\download\";
			}

			Global.ConnectionLog = new CEventLogEngine( Global.m_DataDirectory + "connection.log");
			Global.ConnectionLog.Enabled = false; // Set to true to have all NNTP trafic logged to connection.log

			if( !System.IO.Directory.Exists( Global.m_CacheDirectory))
				System.IO.Directory.CreateDirectory( Global.m_CacheDirectory);

			if( !System.IO.Directory.Exists( Global.m_DownloadDirectory))
				System.IO.Directory.CreateDirectory( Global.m_DownloadDirectory);

			Decoder.DecodeQueue = new ArrayQueue();
			Decoder.DecoderThread = new System.Threading.Thread( new System.Threading.ThreadStart( Decoder.Decode));
			Decoder.DecoderThread.Priority = System.Threading.ThreadPriority.Lowest;
			Decoder.DecoderThread.Name = "Decoder";
			Decoder.DecoderThread.Start();

			Global.m_Options = new OptionValues(false, true, 15, false, true, 5, true, 6, false, false, "", "", false, false, Global.m_CurrentDirectory, false);	
			if(!LoadOptions(Global.m_DataDirectory + "options.xml"))
			{
			if(!LoadOptions(Global.m_CurrentDirectory + "options.xml"))
				frmMain.LogWriteError(Global.m_CurrentDirectory + "options.xml failed to load");

			}
			m_ServerManager = new ServerManager();
			if(!m_ServerManager.LoadServers(Global.m_DataDirectory + "servers.xml"))
			{
			if(!m_ServerManager.LoadServers(Global.m_CurrentDirectory + "servers.xml"))
				frmMain.LogWriteError(Global.m_CurrentDirectory + "servers.xml failed to load");

			}
			if(System.IO.File.Exists(Global.m_DataDirectory + "nzb-o-matic.xml"))
				ImportNZB(Global.m_DataDirectory + "nzb-o-matic.xml");

			if( Global.m_Options.ConnectOnStart)
			{
				frmMain.LogWriteInfo("Connect on startup enabled.");
				Connect();
			}

			foreach(string str in Global.Args)
			{
				frmMain.LogWriteInfo("Startup parameter: " + str);
				if(str == "/start")
				{
					frmMain.LogWriteInfo("Connect on startup switch detected.");
					Connect();
				}
				if(str == "/exit")
				{
					frmMain.LogWriteInfo("Exit on completion switch detected.");
					Global.m_ExitComplete = true;
					Menu_Main_Options_Exit.Checked = true;
				}
				if(str.EndsWith(".nzb"))
				{
					ImportNZB(str);
				}
			}

			copydata = new CopyData();
			copydata.AssignHandle(this.Handle);
			copydata.Channels.Add("NZBImport");
			copydata.DataReceived += new DataReceivedEventHandler(copydata_DataReceived);

			UpdateServers();

			frmMain.LogWriteInfo(Global.Name + " succesfully started.");
			frmMain.LogWriteInfo("Version: " + Global.Version);
		}

		private object LoadOptionValue(System.Xml.XmlNode node, string tag, System.Type type, object defaultValue)
		{
			if(node == null)
				return null;

			try
			{
				string val = "";
				if(node[tag].InnerText != null)
				{
					val = node[tag].InnerText;
					switch(type.FullName)
					{
						case "System.Boolean":
							return System.Convert.ToBoolean(val);
						case "System.Int16":
							return System.Convert.ToInt16(val);
						case "System.Int32":
							return System.Convert.ToInt32(val);
						case "System.Int64":
							return System.Convert.ToInt64(val);
						case "System.Double":
							return System.Convert.ToDouble(val);
						case "System.Single":
							return System.Convert.ToSingle(val);
						case "System.String":
							return System.Convert.ToString(val);
						case "System.Object":
							return val;
						default:
							return defaultValue;
					}
				}
			}
			catch
			{
				try
				{
					return defaultValue;
				}
				catch
				{
					return null;
				}
			}
			return null;
		}

		private bool LoadOptions(string file)
		{
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			try
			{
				doc.Load(file);
			}
			catch
			{
				return false;
			}

			if(doc.DocumentElement == null)
			{
				MessageBox.Show("Error loading options");
				return false;
			}

			System.Xml.XmlNode options = doc.DocumentElement;
			
			Global.m_Options.AssociateWithNZB = (bool)LoadOptionValue(options, "associate", typeof(bool), false);
			Global.m_Options.DisconnectOnIdle = (bool)LoadOptionValue(options, "disconnectidle", typeof(bool), true);
			Global.m_Options.IdleDelay = (int)LoadOptionValue(options, "idledelay", typeof(int), 15);
			Global.m_Options.AutoPrune = (bool)LoadOptionValue(options, "autoprune", typeof(bool), false);
			Global.m_Options.RetryConnections = (bool)LoadOptionValue(options, "retryconnections", typeof(bool), true);
			Global.m_Options.RetryDelay = (int)LoadOptionValue(options, "retrydelay", typeof(int), 5);
			Global.m_Options.LimitAttempts = (bool)LoadOptionValue(options, "limitattempts", typeof(bool), false);
			Global.m_Options.RetryAttempts = (int)LoadOptionValue(options, "retryattempts", typeof(int), 6);
			Global.m_Options.MinimizeToTray = (bool)LoadOptionValue(options, "minitotray", typeof(bool), false);
			Global.m_Options.ConnectOnStart = (bool)LoadOptionValue(options, "connectonstart", typeof(bool), false);
			Global.m_Options.SavePath = (string)LoadOptionValue(options, "savepath", typeof(string), "");
			Global.m_Options.SaveFolder = (string)LoadOptionValue(options, "savefolder", typeof(string), "");
			Global.m_Options.DeleteNZB = (bool)LoadOptionValue(options, "deletenzb", typeof(bool), false);
			Global.m_Options.MonitorPath = (string)LoadOptionValue(options, "monitorpath", typeof(string), Global.m_CurrentDirectory);
			Global.m_Options.MonitorFolder = (bool)LoadOptionValue(options, "monitorfolder", typeof(bool), false);
			Global.m_Options.PausePar2 = (bool)LoadOptionValue(options, "pausepar2", typeof(bool), false);

			m_TotalDownloadOffset = (double)LoadOptionValue(options, "downloadoffset", typeof(double), 0);

			Menu_Main_SaveWindowStatus.Checked = (bool)LoadOptionValue(options, "savewindowstatus", typeof(bool), true);
			Height = (int)LoadOptionValue(options, "height", typeof(int), 500);
			Width = (int)LoadOptionValue(options, "width", typeof(int), 550);

			if (this.Height < 50)
			{
				this.Height = 500;
			}
			if (this.Width < 175)
			{
				this.Width = 550;
			}

			Panel_Connections.Height = (int)LoadOptionValue(options, "connections_height", typeof(int), 100);
			chServer.Width = (int)LoadOptionValue(options, "connections_width_server", typeof(int), 124);
			chID.Width = (int)LoadOptionValue(options, "connections_width_number", typeof(int), 24);
			chConnStatus.Width = (int)LoadOptionValue(options, "connections_width_status", typeof(int), 160);
			chProgress.Width = (int)LoadOptionValue(options, "connections_width_progress", typeof(int), 124);
			chSpeed.Width = (int)LoadOptionValue(options, "connections_width_speed", typeof(int), 60);

			chArticle.Width = (int)LoadOptionValue(options, "queue_width_article", typeof(int), 160);
			chSize.Width = (int)LoadOptionValue(options, "queue_width_size", typeof(int), 60);
			chParts.Width = (int)LoadOptionValue(options, "queue_width_parts", typeof(int), 60);
			chStatus.Width = (int)LoadOptionValue(options, "queue_width_status", typeof(int), 60);
			chDate.Width = (int)LoadOptionValue(options, "queue_width_date", typeof(int), 60);
			chGroups.Width = (int)LoadOptionValue(options, "queue_width_groups", typeof(int), 60);

			this.Location = new Point(
				(int)LoadOptionValue(options, "xloc", typeof(int), 0), 
				(int)LoadOptionValue(options, "yloc", typeof(int), 0));
			bool maximized = (bool)LoadOptionValue(options, "maximized", typeof(bool), false);
			if (maximized)
			{
				this.WindowState = FormWindowState.Maximized;
			}

			if (Global.m_Options.MonitorFolder &&
				System.IO.Directory.Exists(Global.m_Options.MonitorPath))
			{
				watcher = new System.IO.FileSystemWatcher(Global.m_Options.MonitorPath, "*.nzb");
				this.watchedFileList = new Hashtable();
				watcher.Created += new System.IO.FileSystemEventHandler(watcher_Created);
				watcher.EnableRaisingEvents = true;
			}
			else
			{
				watcher = null;
			}

			return true;
		}

		/*private void m_ImportNZB_Fast( frmProgress frm)
		{
			ArrayList newArticles = new ArrayList();
			System.IO.StreamReader strReader = new System.IO.StreamReader(m_ImportNZB_Filename);
			System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(strReader);
			xmlReader.XmlResolver = null;

			try
			{
				string Directory = System.IO.Path.GetFileNameWithoutExtension(m_ImportNZB_Filename);
				frm.lAction.Text = "Importing NZB file [" + Directory + "]";
				frm.pbProgress.Maximum = (int)strReader.BaseStream.Length;
				frm.Update();

				string Subject = "";
				DateTime Date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
				string Poster = "";
				string ImportFile = "";
				ArrayList aGroups = new ArrayList();

				Article article = null;
				while( xmlReader.Read())
				{
					if( xmlReader.NodeType == System.Xml.XmlNodeType.Element || xmlReader.NodeType == System.Xml.XmlNodeType.EndElement)
					{
						if( xmlReader.Name == "file" && xmlReader.NodeType == System.Xml.XmlNodeType.Element)
						{
							Subject = "";
							Date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
							Poster = "";
							ImportFile = System.IO.Path.GetFileNameWithoutExtension(m_ImportNZB_Filename);

							for( int i = 0; i < xmlReader.AttributeCount; i++)
							{
								if( xmlReader.Name == "subject")
									Subject = xmlReader.Value;
								if( xmlReader.Name == "date")
									Date = Date.AddSeconds(double.Parse(xmlReader.Value));
								if( xmlReader.Name == "poster")
									Poster = xmlReader.Value;
								if( xmlReader.Name == "importfile")
									ImportFile = xmlReader.Value;
							}

							xmlReader.MoveToElement();
						}

						if( xmlReader.Name == "groups" && xmlReader.NodeType == System.Xml.XmlNodeType.Element)
							aGroups.Clear();

						if( xmlReader.Name == "group")
							aGroups.Add(xmlReader.Value);

						if( xmlReader.Name == "groups" && xmlReader.NodeType == System.Xml.XmlNodeType.EndElement)
						{
							string[] Groups = new string[aGroups.Count];
							for( int i = 0; i < aGroups.Count; i++)
								Groups[i] = (string)aGroups[i];

							article = new Article( Subject, Date, Poster, Groups, ImportFile);
						}

						if( xmlReader.Name == "segment" && article != null && xmlReader.NodeType == System.Xml.XmlNodeType.Element)
						{
							int Number = int.Parse(xmlReader.GetAttribute("number"));
							int Bytes = int.Parse(xmlReader.GetAttribute("bytes"));
							string ArticleID = xmlReader.Value;

							article.AddSegment( Number, Bytes, ArticleID);
						}

						if( xmlReader.Name == "file" && xmlReader.NodeType == System.Xml.XmlNodeType.EndElement)
						{
							newArticles.Add( article);
							article = null;
						}
					}

					frm.pbProgress.Value = (int)strReader.BaseStream.Position;
					frm.Update();
				}
			}
			catch
			{
				MessageBox.Show( "Error importing NZB file");
				return;
			}
		}*/
		
		private void m_ImportNZB( frmProgress frm)
		{
			this.lvArticles.BeginUpdate();
			System.Xml.XmlDocument XmlDoc = new System.Xml.XmlDocument();
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			
			try
			{
				using(System.IO.StreamReader sr = new System.IO.StreamReader(m_ImportNZB_Filename))
				{
					using(System.IO.StringWriter sw = new System.IO.StringWriter(sb))
					{
						string line;
						do
						{
							line = sr.ReadLine();
							if(line != null)
								if(line != "" && line != "\n")
									sw.WriteLine(line);
						} while (line != null);
					}
				}
			}
			catch
			{
				MessageBox.Show( "Error importing NZB file");
				return;
			}

			
			try
			{
				XmlDoc.XmlResolver = null;
				System.IO.StringReader xmlsr = new System.IO.StringReader(sb.ToString());
				XmlDoc.Load(xmlsr);

				// Ugly way of removing the XML namespace
				if( XmlDoc.InnerXml.IndexOf( "xmlns=\"http://www.newzbin.com/DTD/2003/nzb\"") > 0)
					XmlDoc.InnerXml = XmlDoc.InnerXml.Replace("xmlns=\"http://www.newzbin.com/DTD/2003/nzb\"", "");
				//just in case
				if( XmlDoc.InnerXml.IndexOf( "xmlns=\"http://www.newzbin.com/DTD/2004/nzb\"") > 0)
					XmlDoc.InnerXml = XmlDoc.InnerXml.Replace("xmlns=\"http://www.newzbin.com/DTD/2004/nzb\"", "");
                // quick hack for newzbin2.es
                Regex rgx = new Regex("xmlns=\"http.*newzbin.*/dtd/.*/nzb\"", RegexOptions.IgnoreCase);
                XmlDoc.InnerXml = rgx.Replace(XmlDoc.InnerXml, "");
			}
			catch
			{
				MessageBox.Show( "Error importing NZB file");
				return;
			}
		
			if( XmlDoc.DocumentElement == null)
			{
				MessageBox.Show( "Error importing NZB file");
				return;
			}

			string Directory = System.IO.Path.GetFileNameWithoutExtension(m_ImportNZB_Filename);

			frm.lAction.Text = "Importing NZB file [" + Directory + "]";
			frm.pbProgress.Maximum = XmlDoc.DocumentElement.SelectNodes("file").Count;
			frm.Update();
			foreach( System.Xml.XmlNode XmlArticle in XmlDoc.DocumentElement.SelectNodes("file"))
			{
				string Subject = XmlArticle.SelectSingleNode("@subject").Value;
				DateTime Date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
				Date = Date.AddSeconds(double.Parse(XmlArticle.SelectSingleNode("@date").Value));
				string Poster = XmlArticle.SelectSingleNode("@poster").Value;
				
				string ImportFile = "";
				if(( XmlArticle.SelectSingleNode("@importfile") != null) && XmlArticle.SelectSingleNode("@importfile").Value != "")
					ImportFile = XmlArticle.SelectSingleNode("@importfile").Value;
				else
					ImportFile = System.IO.Path.GetFileNameWithoutExtension(m_ImportNZB_Filename);

				int i = 0;
				string[] Groups = new string[XmlArticle.SelectNodes("groups/group").Count];
				foreach( System.Xml.XmlNode XmlGroup in XmlArticle.SelectNodes("groups/group"))
				{
					Groups[i] = XmlGroup.InnerText;
					i++;
				}

				Article article = new Article( Subject, Date, Poster, Groups, ImportFile);
				article.ListViewControl = lvArticles;

				article.Status = ArticleStatus.Loading;
				foreach( System.Xml.XmlNode XmlSegment in XmlArticle.SelectNodes("segments/segment"))
				{
					int Number = int.Parse(XmlSegment.SelectSingleNode("@number").Value);
					int Bytes = int.Parse(XmlSegment.SelectSingleNode("@bytes").Value);
					string ArticleID = XmlSegment.InnerText;

					article.AddSegment( Number, Bytes, ArticleID);
				}
				article.Segments.Sort( new SegmentSorter());

				if(article.FinishedParts == article.Segments.Count)
				{
					article.Status = ArticleStatus.DecodeQueued;
					m_ServerManager.AddArticle(article);
					Decoder.DecodeQueue.Enqueue(article);
				}
				else
				{
					if(Global.m_Options.PausePar2)
					{
						if( article.Subject.IndexOf(".vol") != -1 )
						{
							if( article.Subject.IndexOf(".par2") != -1 || article.Subject.IndexOf(".PAR2") != -1)
							{
								article.Status = ArticleStatus.Paused;
							}
						}
						else
						{
							article.Status = ArticleStatus.Queued;
						}
					}			
					else 
					{
						article.Status = ArticleStatus.Queued;
					}
					m_ServerManager.AddArticle(article);
				}

				lvArticles.Items.Add(article.StatusItem);

				frm.pbProgress.Value ++;
				frm.Update();
			}

			try
			{
				if(Global.m_Options.DeleteNZB)
					System.IO.File.Delete(m_ImportNZB_Filename);
			}
			catch
			{
			}
			this.lvArticles.EndUpdate();
		}

		private void ImportNZB(string filename)
		{
			queueContainsNewItems = true;
			m_ImportNZB_Filename = filename;

			frmProgress frm = new frmProgress();
			frm.LongDurationCall += new NZB_O_Matic.frmProgress.DelLongDurationCall(m_ImportNZB);
			frm.ShowDialog(this);
			frm.Close();
			frm.Dispose();
			m_ServerManager.InitializeQueues();
			m_ServerManager.FillQueues();
		}

		private void OpenImportNZB()
		{
			if( OpenFile_ImportNZB.ShowDialog(this) != DialogResult.OK)
				return;

			foreach (string fileName in OpenFile_ImportNZB.FileNames)
			{
				ImportNZB(fileName); //OpenFile_ImportNZB.FileName);
			}
		}

		private void Connect()
		{
			if( Global.m_Connected)
				return;

			this.Menu_Main_File_Connect.Enabled = false;
			this.Menu_Main_File_Disconnect.Enabled = true;
			this.Button_Connect.Enabled = false;
			this.Button_Disconnect.Enabled = true;
			this.Context_Connect.Enabled = false;
			this.Context_Disconnect.Enabled = true;

			this.Button_AddServer.Enabled = false;
			this.Button_EditServer.Enabled = false;
			this.Button_DeleteServer.Enabled = false;
			this.Menu_Main_Edit_AddServer.Enabled = false;
			this.Menu_Main_Edit_EditServer.Enabled = false;
			this.Menu_Main_Edit_DeleteServer.Enabled = false;

			Global.m_Connected = true;
			m_ServerManager.Connect();
		}

		private void Disconnect(bool idle)
		{
			disconnectedOnIdle = idle;

			if( !Global.m_Connected)
				return;

			this.Menu_Main_File_Connect.Enabled = true;
			this.Menu_Main_File_Disconnect.Enabled = false;
			this.Button_Connect.Enabled = true;
			this.Button_Disconnect.Enabled = false;
			this.Context_Connect.Enabled = true;
			this.Context_Disconnect.Enabled = false;

			this.Button_AddServer.Enabled = true;
			this.Button_EditServer.Enabled = true;
			this.Button_DeleteServer.Enabled = true;
			this.Menu_Main_Edit_AddServer.Enabled = true;
			this.Menu_Main_Edit_EditServer.Enabled = true;
			this.Menu_Main_Edit_DeleteServer.Enabled = true;

			Global.m_Connected = false;
			m_ServerManager.Disconnect();
		}

		private void NZBType_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			try
			{
				this.TabControl_Main.SelectedTab = this.TabControl_Main.TabPages[1];
				this.TabControl_Main.Update();
				if( !System.IO.Directory.Exists( Global.m_CurrentDirectory + "nzb"))
					System.IO.Directory.CreateDirectory( Global.m_CurrentDirectory + "nzb");

				if(e.Data.GetDataPresent(DataFormats.FileDrop, true))
				{
					string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
					foreach(string str in files)
						ImportNZB(str);
				}
				else if(e.Data.GetDataPresent("UniformResourceLocator", true))
				{
				
					string url = (string)e.Data.GetData("Text");
					if((url.ToLower().StartsWith("http://") || url.ToLower().StartsWith("file://")) && url.ToLower().EndsWith(".nzb"))
					{
						string filename = url.Substring(url.LastIndexOf("/") + 1);
						System.Net.WebClient wc = new System.Net.WebClient();
						wc.DownloadFile(url, Global.m_CurrentDirectory + "nzb" + "\\" + filename);
						wc.Dispose();
						ImportNZB(filename);
					}
				}
				else if(e.Data.GetDataPresent("FileName"))
				{
					string[] names = (string[])e.Data.GetData("FileName");
					foreach(string name in names)
						ImportNZB(name);
				}
			}
			catch(Exception z)
			{
				frmMain.LogWriteLine(z);
			}
		}

		private void NZBType_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(e.Data.GetDataPresent("UniformResourceLocator", false) || e.Data.GetDataPresent("FileName", false) || e.Data.GetDataPresent(DataFormats.FileDrop, false))
				e.Effect = DragDropEffects.All;
		}

		private void Context_Exit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Global.m_StatusLog.Items.Add("Status log closed: " + System.DateTime.Now.ToString());
			Global.m_AllowLogging = false;
		}

		private void Icon_Tray_Click(object sender, System.EventArgs e)
		{
			ShowWindow();
		}

		private void frmMain_Closed(object sender, System.EventArgs e)
		{
			m_ServerManager.Disconnect();

			Decoder.TerminateDecoder = true;
			Decoder.DecoderThread.Join();

			SaveState();

			//remove tray icon
			Icon_Tray.Dispose();
			ContextMenu_Icon.Dispose();

			// Close connection log
			Global.ConnectionLog.Dispose();
		}

		private void SaveState()
		{
			//file queue
			System.Xml.XmlDocument XmlDoc = new System.Xml.XmlDocument();
			XmlDoc.AppendChild(XmlDoc.CreateElement("nzb"));

			foreach( Article article in m_ServerManager.m_Articles)
			{
				if( article.Status == ArticleStatus.Decoded || article.Status == ArticleStatus.Deleted || article.Status == ArticleStatus.Error)
					continue;

				System.Xml.XmlNode XmlArticle = XmlDoc.CreateElement( "file");

				DateTime Date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
				
				XmlAddAttr( XmlArticle, "subject", article.Subject);
				XmlAddAttr( XmlArticle, "date", ((long)article.Date.Subtract(Date).TotalSeconds).ToString());
				XmlAddAttr( XmlArticle, "poster", article.Poster);
				XmlAddAttr( XmlArticle, "importfile", article.ImportFile);

				System.Xml.XmlNode XmlGroups = XmlDoc.CreateElement( "groups");
				foreach( string group in article.Groups)
				{
					System.Xml.XmlNode XmlGroup = XmlDoc.CreateElement( "group");
					XmlGroup.InnerText = group;
					XmlGroups.AppendChild( XmlGroup);
				}
				XmlArticle.AppendChild( XmlGroups);
				
				System.Xml.XmlNode XmlSegments = XmlDoc.CreateElement( "segments");
				foreach( Segment segment in article.Segments)
				{
					System.Xml.XmlNode XmlSegment = XmlDoc.CreateElement( "segment");
	
					XmlAddAttr( XmlSegment, "bytes", segment.Bytes.ToString());
					XmlAddAttr( XmlSegment, "number", segment.Number.ToString());

					XmlSegment.InnerText = segment.ArticleID;
					XmlSegments.AppendChild( XmlSegment);
				}
				XmlArticle.AppendChild( XmlSegments);

				XmlDoc.DocumentElement.AppendChild(XmlArticle);
			}

			XmlDoc.Save(Global.m_DataDirectory + "nzb-o-matic.xml");

			//servers
			System.Xml.XmlDocument ServerDoc = new System.Xml.XmlDocument();
			System.Xml.XmlDeclaration decleration = ServerDoc.CreateXmlDeclaration("1.0", "utf-8", "");
			ServerDoc.AppendChild(decleration);
			
			//create server groups element
			System.Xml.XmlNode servergroups = ServerDoc.CreateElement("servergroups");

			//loop through each servergroup
			foreach(ServerGroup sgs in m_ServerManager.m_ServerGroups)
			{
				//create servergroup tag
				System.Xml.XmlNode servergroup = ServerDoc.CreateElement("servergroup");
				//create servers tag
				System.Xml.XmlNode servers = ServerDoc.CreateElement("servers");
				//loop through each server
				foreach(Server serv in sgs.Servers)
				{
					//add server tag
					System.Xml.XmlNode snode = ServerDoc.CreateElement("server");
					XmlAddAttr(snode, "enabled", serv.Enabled.ToString());
					
					XmlAddElement(snode, "address", serv.Hostname);
					XmlAddElement(snode, "port", serv.Port.ToString());
					//add login
					System.Xml.XmlNode login = ServerDoc.CreateElement("login");
					if(serv.RequiresLogin)
					{
						XmlAddElement(login, "username", serv.Username);
						XmlAddElement(login, "password", serv.Password);
					}
					snode.AppendChild(login);
					XmlAddElement(snode, "connections", serv.NoConnections.ToString());

					XmlAddElement(snode, "needsgroup", serv.NeedsGroup.ToString());

                    XmlAddElement(snode, "ssl", serv.UseSSL.ToString());

					//add server to servers
					servers.AppendChild(snode);
				}
				//add servers to servergroup
				servergroup.AppendChild(servers);

				//add servergroup to servergroups
				servergroups.AppendChild(servergroup);
			}
			
			ServerDoc.AppendChild(servergroups);
			ServerDoc.Save(Global.m_DataDirectory + "servers.xml");

			//options
			System.Xml.XmlDocument OptionsDoc = new System.Xml.XmlDocument();
			System.Xml.XmlDeclaration dec = OptionsDoc.CreateXmlDeclaration("1.0", "utf-8", "");
			OptionsDoc.AppendChild(dec);

			System.Xml.XmlElement options = OptionsDoc.CreateElement("options");

			XmlAddElement(options, "associate", Global.m_Options.AssociateWithNZB.ToString());
			XmlAddElement(options, "autoprune", Global.m_Options.AutoPrune.ToString());
			XmlAddElement(options, "idledelay", Global.m_Options.IdleDelay.ToString());
			XmlAddElement(options, "limitattempts", Global.m_Options.LimitAttempts.ToString());
			XmlAddElement(options, "minitotray", Global.m_Options.MinimizeToTray.ToString());
			XmlAddElement(options, "retryattempts", Global.m_Options.RetryAttempts.ToString());
			XmlAddElement(options, "retryconnections", Global.m_Options.RetryConnections.ToString());
			XmlAddElement(options, "retrydelay", Global.m_Options.RetryDelay.ToString());
			XmlAddElement(options, "connectonstart", Global.m_Options.ConnectOnStart.ToString());
			XmlAddElement(options, "savepath", Global.m_Options.SavePath.ToString());
			XmlAddElement(options, "savefolder", Global.m_Options.SaveFolder.ToString());
			XmlAddElement(options, "deletenzb", Global.m_Options.DeleteNZB.ToString());
			XmlAddElement(options, "disconnectidle", Global.m_Options.DisconnectOnIdle.ToString());
			XmlAddElement(options, "monitorfolder", Global.m_Options.MonitorFolder.ToString());
			XmlAddElement(options, "monitorpath", Global.m_Options.MonitorPath.ToString());
			XmlAddElement(options, "pausepar2", Global.m_Options.PausePar2.ToString());
			
			XmlAddElement(options, "downloadoffset", ((((double)m_ServerManager.LifetimeBytesReceived / 1024) / 1024) + m_TotalDownloadOffset).ToString());

			XmlAddElement(options, "savewindowstatus", Menu_Main_SaveWindowStatus.Checked.ToString());
			if( Menu_Main_SaveWindowStatus.Checked)
			{
				XmlAddElement(options, "height", Height.ToString());
				XmlAddElement(options, "width", Width.ToString());
				
				int xLoc = this.Location.X;
				int yLoc = this.Location.Y;
				if (xLoc + this.Width < 0)
				{
					xLoc = 0;
				}
				if (yLoc + this.Height < 0)
				{
					yLoc = 0;
				}
				XmlAddElement(options, "xloc", xLoc.ToString());
				XmlAddElement(options, "yloc", yLoc.ToString());
				bool maximized = false;
				if (this.WindowState == FormWindowState.Maximized)
				{
					maximized = true;
				}
				XmlAddElement(options, "maximized", maximized.ToString());

				XmlAddElement(options, "connections_height", Panel_Connections.Height.ToString());
				XmlAddElement(options, "connections_width_server", chServer.Width.ToString());
				XmlAddElement(options, "connections_width_number", chID.Width.ToString());
				XmlAddElement(options, "connections_width_status", chConnStatus.Width.ToString());
				XmlAddElement(options, "connections_width_progress", chProgress.Width.ToString());
				XmlAddElement(options, "connections_width_speed", chSpeed.Width.ToString());

				XmlAddElement(options, "queue_width_article", chArticle.Width.ToString());
				XmlAddElement(options, "queue_width_size", chSize.Width.ToString());
				XmlAddElement(options, "queue_width_parts", chParts.Width.ToString());
				XmlAddElement(options, "queue_width_status", chStatus.Width.ToString());
				XmlAddElement(options, "queue_width_date", chDate.Width.ToString());
				XmlAddElement(options, "queue_width_groups", chGroups.Width.ToString());
			}

			OptionsDoc.AppendChild(options);

			OptionsDoc.Save(Global.m_DataDirectory + "options.xml");
		}

		private void Context_Connect_Click(object sender, System.EventArgs e)
		{
			Connect();
		}

		private void Context_Disconnect_Click(object sender, System.EventArgs e)
		{
			Disconnect(false);
		}

		private void lvArticles_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == System.Windows.Forms.Keys.Delete)
				DeleteQueueItems();
			if(e.KeyCode == System.Windows.Forms.Keys.A && e.Control)
				foreach(ListViewItem lv in lvArticles.Items)
					lv.Selected = true;
		}

		private void DeleteQueueItems()
		{
			this.lvArticles.BeginUpdate();
			for( int i = lvArticles.Items.Count - 1; i >= 0; i--)
				if( lvArticles.Items[i].Selected)
				{
					m_ServerManager.DeleteArticle((Article)lvArticles.Items[i].Tag);
				}
			UpdateArticles();
			this.lvArticles.EndUpdate();
		}

		private void Update_Timer_Tick(object sender, System.EventArgs e)
		{
			//Update connection status items
			RefreshConnectionStatus();
			//Update article status items
			RefreshArticles();

			//retry on connect failure
			if(Global.m_Options.RetryConnections)
				if(Global.m_Connected)
					foreach(ServerGroup sg in m_ServerManager.m_ServerGroups)
						foreach(Server serv in sg.Servers)
							foreach(Connection con in serv.Connections)
								if(con.Failed)
								{
									con.ConnectWait += this.Update_Timer.Interval;
									if(con.ConnectWait >= (Global.m_Options.RetryDelay * 60 * 1000))
										if((con.ConnectAttempts < Global.m_Options.RetryAttempts) || !Global.m_Options.LimitAttempts)
											con.Connect();
								}
			//set connected/disconnected panel
			if(Global.m_Connected && PanConnect.Text != "Connected")
				PanConnect.Text = "Connected";
			else if(!Global.m_Connected && PanConnect.Text != "Disconnected")
				PanConnect.Text = "Disconnected";

			//set download speed panel
			double realSpeed = m_ServerManager.Speed;
			double speed = Math.Round(realSpeed, 2);
			string speedStr = String.Format("{0:0.0} KB/s", speed);
			if(PanSpeed.Text != speedStr)
				PanSpeed.Text = speedStr;

			//set downloaded MB/total MB panel
			double complete = m_ServerManager.CompletedMB;
			double total = m_ServerManager.TotalMB;
			string totalStr = String.Format("{0:0.0}/{1:0.0} Mb", complete, total);
			if(PanFileSize.Text != totalStr)
				PanFileSize.Text = totalStr;

			//set completion percent panel
			double percent = m_ServerManager.Completion;
			string perStr = String.Format("{0:0.00}%", percent);
			if(PanPercent.Text != perStr)
				PanPercent.Text = perStr;

			//set estimated time panel
			for(int i = 0; i < speedHistory.GetLength(0) - 1; i++)
				speedHistory[i] = speedHistory[i+1];
			speedHistory[speedHistory.GetLength(0)-1] = realSpeed;
			double averagespeed = 0;
			for(int i = 0; i < speedHistory.GetLength(0); i++)
				averagespeed += speedHistory[i];
			averagespeed /= speedHistory.GetLength(0);
			string timeStr = "";
			if(total - complete == 0 || averagespeed == 0)
				timeStr = "00d :00h :00m";
			else
			{
				double totalseconds = ((total - complete) * 1024) / averagespeed;
				try
				{
					TimeSpan ts = TimeSpan.FromSeconds(totalseconds + 1);
					timeStr = ts.Days + "d :" + ts.Hours + "h :" + ts.Minutes + "m";
				}
				catch
				{
					timeStr = "Unknown";
				}
				
			}
			
			if(PanDownloadTime.Text != timeStr)
				PanDownloadTime.Text = timeStr;

			//set downloaded total panel
			long totalbytes = m_ServerManager.LifetimeBytesReceived + 1;
			double totalmb = (((double)totalbytes / 1024) / 1024) + m_TotalDownloadOffset;
			double totalgb = totalmb / 1024;
			string downloadtotal = "";
			if(totalgb > 1)
				downloadtotal = String.Format("Total: {0:0.00}Gb", totalgb);
			else
				downloadtotal = String.Format("Total: {0:0.00}Mb", totalmb);

			if(PanDownloaded.Text != downloadtotal)
				PanDownloaded.Text = downloadtotal;

			//disconnect on idle
			if(Global.m_Connected && Global.m_Options.DisconnectOnIdle)
			{
				if(speed == 0)
				{
					idlecount += this.Update_Timer.Interval;
					if(idlecount >= (Global.m_Options.IdleDelay * 60 * 1000))
						Disconnect(true);
				}
			}
			else
			{
				idlecount = 0;
			}

			//set download on completion enable/disable
			if(Global.m_Connected)
			{
				if(this.Menu_Main_Options_Exit.Enabled == false)
					this.Menu_Main_Options_Exit.Enabled = true;
			}
			else if(this.Menu_Main_Options_Exit.Enabled == true)
				this.Menu_Main_Options_Exit.Enabled = false;

			//counter variable, tick = 500ms
			count++;

			//every 5 secs
			if(count % 10 == 0)
			{
				//auto prune
				if(Global.m_Options.AutoPrune)
					m_ServerManager.PruneQueue();

				//exit on completion
				if(Global.m_ExitComplete)
				{
					if(Global.m_Connected)
					{
						if(m_ServerManager.CompletedMB == m_ServerManager.TotalMB)
						{
							bool flag = true;
							foreach(Article article in m_ServerManager.m_Articles)
							{
								if(article.Status != ArticleStatus.Decoded)
								{
									flag = false;
									break;
								}
							}

							if(flag)
								this.Close();
						}
					}
				}
			}

			//every 60 secs
			if(count % 120 == 0 && Global.m_SaveState)
				SaveState();

			//every 5 min fix up the queues
			if (count % 600 == 0)
			{
				//m_ServerManager.RebuildQueue();
			}

			//make sure variable doesn't get too big
			count = count % 12000;

			if (!Global.m_Connected 
				&& disconnectedOnIdle
				&& queueContainsNewItems)
			{
				queueContainsNewItems = false;
				this.Connect();
			}
		}

		private void ResetConnectionStatus()
		{
			lvConnections.Items.Clear();
			RefreshConnectionStatus();
		}

		private void RefreshConnectionStatus()
		{
			foreach(ServerGroup servergroup in m_ServerManager.m_ServerGroups)
				foreach(Server server in servergroup.Servers)
					if(server.Enabled)
						foreach(Connection connection in server.Connections)
						{
							connection.UpdateStatus();
							if(!lvConnections.Items.Contains(connection.StatusItem))
							{
								lvConnections.Items.Add(connection.StatusItem);
							}
						}
		}

		private void frmServer_Add_Closing(object sender, CancelEventArgs e)
		{
			if(((Form_Server)sender).DialogResult == DialogResult.OK)
			{
				ServerValues sv = ((Form_Server)sender).GetServer();
				m_ServerManager.PruneServerGroups();
				
				if(sv.Connections != 0)
				{
					if(m_ServerManager.m_ServerGroups.Count <= sv.Group)
					{
						ServerGroup sg = m_ServerManager.AddServerGroup();
						sg.AddServer(new Server(sv.Host, sv.Port, sv.Connections, sv.Login, sv.User, sv.Password, sv.NeedsGroup, sv.UseSSL));
						UpdateServers();
					}
					else
					{
						((ServerGroup)m_ServerManager.m_ServerGroups[sv.Group]).AddServer(new Server(sv.Host, sv.Port, sv.Connections, sv.Login, sv.User, sv.Password, sv.NeedsGroup, sv.UseSSL));
						UpdateServers();
					}
					
				}
				UpdateServers();
				Global.m_SaveState = true;
			}
		}

		private void frmServer_Edit_Closing(object sender, CancelEventArgs e)
		{
			if(((Form_Server)sender).DialogResult == DialogResult.OK)
			{
				((Form_Server)sender).LastSetServer.ServerGroup.RemoveServer(((Form_Server)sender).LastSetServer);
				frmServer_Add_Closing(sender, e);
				UpdateServers();
				Global.m_SaveState = true;
			}
		}

		private void Menu_Main_Help_About_Click(object sender, System.EventArgs e)
		{
			frmAbout about = new frmAbout();
			about.ShowDialog(this);
			about.Dispose();
		}

		private void Menu_Main_Options_Prefernces_Click(object sender, System.EventArgs e)
		{
			frmOptions options = new frmOptions();
			options.SetOptions(Global.m_Options);
			options.Closing += new CancelEventHandler(Options_Closing);
			options.ShowDialog(this);
		}

		private void Options_Closing(object sender, CancelEventArgs e)
		{
			if(((frmOptions)sender).DialogResult == DialogResult.OK)
			{
				Global.m_Options = ((frmOptions)sender).GetOptions();

				if (Global.m_Options.MonitorFolder &&
					System.IO.Directory.Exists(Global.m_Options.MonitorPath))
				{
					watcher = new System.IO.FileSystemWatcher(Global.m_Options.MonitorPath, "*.nzb");
					this.watchedFileList = new Hashtable();
					watcher.Created += new System.IO.FileSystemEventHandler(watcher_Created);
					watcher.EnableRaisingEvents = true;
				}
				else
				{
					watcher = null;
				}

				if(Global.m_Options.AssociateWithNZB)
				{
					string icon = Application.ExecutablePath + ",0";

					//set NZB-O-Matic data type
					Registry.ClassesRoot.CreateSubKey("NZB-O-Matic").SetValue("", "NZB Document");
					Registry.ClassesRoot.CreateSubKey("NZB-O-Matic").CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
					Registry.ClassesRoot.CreateSubKey("NZB-O-Matic").CreateSubKey("DefaultIcon").SetValue("", icon);

					//set .nzb file extension data
					Registry.ClassesRoot.CreateSubKey(".nzb").SetValue("", "NZB-O-Matic");
					Registry.ClassesRoot.CreateSubKey(".nzb").CreateSubKey("DefaultIcon").SetValue("", icon);
					
					//set application data
					RegistryKey key;
					key = Registry.ClassesRoot.CreateSubKey("Applications").CreateSubKey("NZB-O-Matic.exe");
					key.SetValue("", "NZB-O-Matic");
					key.SetValue("NZB-O-Matic.exe", "NZB-O-Matic.exe");
					key.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + Application.ExecutablePath + "\" \"%1\"");
					key.CreateSubKey("DefaultIcon").SetValue("", icon);
				}
				else
				{
					try
					{
						Registry.ClassesRoot.DeleteSubKeyTree(".nzb");
					}
					catch
					{
					}
					try
					{
						Registry.ClassesRoot.DeleteSubKeyTree("NZB-O-Matic");
					}
					catch
					{
					}
					try
					{
						Registry.ClassesRoot.CreateSubKey("Applications").DeleteSubKeyTree("NZB-O-Matic.exe");
					}
					catch
					{
					}
				}

				try
				{
					PostMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (IntPtr)SPI_SETNONCLIENTMETRICS, IntPtr.Zero);
				}
				catch(Exception z)
				{
					Console.WriteLine(z);
				}

				Global.m_SaveState = true;
			}
		}

		private void frmMain_Resize(object sender, System.EventArgs e)
		{
			if(this.WindowState == FormWindowState.Minimized && Global.m_Options.MinimizeToTray && !m_ChangingState)
				HideWindow();
		}

		private void RebuildQueue()
		{
			lock(lvArticles)
			{
				m_ServerManager.RebuildQueue();
				UpdateArticles();
				lvArticles.Refresh();
			}
			Global.m_SaveState = true;
		}

		private void Pause()
		{
			lock(lvArticles)
			{
				foreach( ListViewItem lvItem in lvArticles.SelectedItems)
				{
					Article article = (Article) lvItem.Tag;
					if( article.Status != ArticleStatus.Paused && article.Status != ArticleStatus.DecodeQueued && article.Status != ArticleStatus.Decoding && article.Status != ArticleStatus.Decoded)
					{
						article.Status = ArticleStatus.Paused;
					}
					else
					{
						if( article.Status == ArticleStatus.Paused)
						{
							article.Status = ArticleStatus.Queued;
						}
					}
				}
				m_ServerManager.RebuildQueue();
			}
		}
		private void Context_Decode_Click(object sender, System.EventArgs e)
		{
			ForceDecode();
		}

		private void ForceDecode()
		{
			lock( lvArticles)
			{
				foreach( ListViewItem lvItem in lvArticles.SelectedItems)
				{
					Article article = (Article) lvItem.Tag;
					if( article.Status != ArticleStatus.DecodeQueued && article.Status != ArticleStatus.Decoding && article.Status != ArticleStatus.Decoded)
					{
						article.Status = ArticleStatus.DecodeQueued;
						Decoder.DecodeQueue.Enqueue(article);
					}
				}
			}
		}

		private void lvArticles_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			lock(lvArticles)
			{
				switch(lvArticles.Columns[e.Column].Text) //using text instead of column number so they can be re-arranged, better way?
				{
						//sort articles here, allows column specific handling
					case "Subject":
						m_ServerManager.m_Articles.Sort(new NZB_O_Matic.ArticleCompareSubject());
						if(m_Sorted == "Subject")
						{
							m_ServerManager.m_Articles.Reverse();
							m_Sorted = "Subject_Reverse";
						}
						else
							m_Sorted = "Subject";							
						break;
					case "Size":
						m_ServerManager.m_Articles.Sort(new NZB_O_Matic.ArticleCompareSize());
						if(m_Sorted == "Size")
						{
							m_ServerManager.m_Articles.Reverse();
							m_Sorted = "Size_Reverse";
						}
						else
							m_Sorted = "Size";
						break;
					case "Parts":
						m_ServerManager.m_Articles.Sort(new NZB_O_Matic.ArticleCompareParts());
						if(m_Sorted == "Parts")
						{
							m_ServerManager.m_Articles.Reverse();
							m_Sorted = "Parts_Reverse";
						}
						else
							m_Sorted = "Parts";
						break;
					case "Status":
						m_ServerManager.m_Articles.Sort(new NZB_O_Matic.ArticleCompareStatus());
						if(m_Sorted == "Status")
						{
							m_ServerManager.m_Articles.Reverse();
							m_Sorted = "Status_Reverse";
						}
						else
							m_Sorted = "Status";
						break;
					case "Date":
						m_ServerManager.m_Articles.Sort(new NZB_O_Matic.ArticleCompareDate());
						if(m_Sorted == "Date")
						{
							m_ServerManager.m_Articles.Reverse();
							m_Sorted = "Date_Reverse";
						}
						else
							m_Sorted = "Date";
						break;
					case "Groups":
						m_ServerManager.m_Articles.Sort(new NZB_O_Matic.ArticleCompareGroups());
						if(m_Sorted == "Groups")
						{
							m_ServerManager.m_Articles.Reverse();
							m_Sorted = "Groups_Reverse";
						}
						else
							m_Sorted = "Groups";
						break;
					default:
						m_Sorted = "none";
						break;
				}
				RebuildQueue();
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			//need a better way than this, quick hack
			switch(this.TabControl_Main.SelectedTab.Name)
			{
				case "TabPage_Servers":
					this.Menu_Main_Edit_AddServer.Enabled = true;
					this.Menu_Main_Edit_EditServer.Enabled = true;
					this.Menu_Main_Edit_DeleteServer.Enabled = true;
					this.Menu_Main_Edit_DeleteArticle.Enabled = false;
					this.Menu_Main_Edit_DecodeArticle.Enabled = false;
					break;
				case "TabPage_Queue":
					this.Menu_Main_Edit_AddServer.Enabled = false;
					this.Menu_Main_Edit_EditServer.Enabled = false;
					this.Menu_Main_Edit_DeleteServer.Enabled = false;
					this.Menu_Main_Edit_DeleteArticle.Enabled = true;
					this.Menu_Main_Edit_DecodeArticle.Enabled = true;
					break;
				case "TabPage_Status":
					this.Menu_Main_Edit_AddServer.Enabled = false;
					this.Menu_Main_Edit_EditServer.Enabled = false;
					this.Menu_Main_Edit_DeleteServer.Enabled = false;
					this.Menu_Main_Edit_DeleteArticle.Enabled = false;
					this.Menu_Main_Edit_DecodeArticle.Enabled = false;
					break;
				default:
					break;
			}
		}

		private void copydata_DataReceived(object sender, DataReceivedEventArgs e)
		{
			try
			{
				string data = ((String)e.Data);
				if(data.EndsWith(".nzb"))
				{
					this.TabControl_Main.SelectedTab = this.TabControl_Main.TabPages[1];
					if (this.WindowState != FormWindowState.Minimized)
					{
						ShowWindow();
					}

					ImportNZB(data);
				}
			}
			catch
			{
			}
		}

		private void lvArticles_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			// TODO
			if(e.CurrentValue == CheckState.Checked)
			{

			}
			if(e.CurrentValue == CheckState.Unchecked)
			{

			}


			this.UpdateArticles();		
		}

		private void watcher_Created(object sender, System.IO.FileSystemEventArgs e)
		{
			if (!this.watchedFileList.Contains(e.FullPath))
			{
				this.watchedFileList.Add(e.FullPath, e.Name);
				// now we need to make sure the file has finished saving
				while (true)
				{
					try 
					{ 
						FileStream fs = File.Open(e.FullPath, FileMode.Open, 
							FileAccess.Read, FileShare.None); 
						fs.Close(); 
						this.ImportNZB(e.FullPath);
						break; 
					} 
					catch 
					{ 
						// If we can't open the file exclusively wait 1.5 second. 
						Thread.Sleep(1500); 
					} 
				}
			}
		}
		#endregion

		private void Context_Pause_Click_1(object sender, System.EventArgs e)
		{
			Pause();
		}

		private void Context_Prune_Click(object sender, System.EventArgs e)
		{
			this.lvArticles.BeginUpdate();
			m_ServerManager.PruneQueue();
			this.lvArticles.EndUpdate();
		}

		private void Context_Delete_Click(object sender, System.EventArgs e)
		{
			DeleteQueueItems();
		}
	}
}
