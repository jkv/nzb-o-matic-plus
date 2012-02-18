//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    frmUpdate.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace Loader
{
	/// <summary>
	/// Summary description for frmUpdate.
	/// </summary>
	public class frmUpdate : System.Windows.Forms.Form
	{
		public static string baseurl = "http://www.bunnyhug.net/nomp/";
		public static string[] m_Files = { "Engine.dll" };

		private bool m_AllowLogging = false;
		private System.Windows.Forms.RichTextBox RichText_Log;
		private System.Windows.Forms.Button Button_Ok;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmUpdate()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			m_AllowLogging = true;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmUpdate));
			this.RichText_Log = new System.Windows.Forms.RichTextBox();
			this.Button_Ok = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// RichText_Log
			// 
			this.RichText_Log.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.RichText_Log.Dock = System.Windows.Forms.DockStyle.Top;
			this.RichText_Log.Location = new System.Drawing.Point(0, 0);
			this.RichText_Log.Name = "RichText_Log";
			this.RichText_Log.Size = new System.Drawing.Size(296, 154);
			this.RichText_Log.TabIndex = 0;
			this.RichText_Log.Text = "";
			// 
			// Button_Ok
			// 
			this.Button_Ok.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Button_Ok.Enabled = false;
			this.Button_Ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.Button_Ok.Location = new System.Drawing.Point(0, 157);
			this.Button_Ok.Name = "Button_Ok";
			this.Button_Ok.Size = new System.Drawing.Size(296, 24);
			this.Button_Ok.TabIndex = 1;
			this.Button_Ok.Text = "Ok";
			this.Button_Ok.Click += new System.EventHandler(this.Button_Ok_Click);
			// 
			// frmUpdate
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(296, 181);
			this.ControlBox = false;
			this.Controls.Add(this.Button_Ok);
			this.Controls.Add(this.RichText_Log);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmUpdate";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Updater";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmUpdate_Closing);
			this.Load += new System.EventHandler(this.frmUpdate_Load);
			this.ResumeLayout(false);

		}
		#endregion

		public void WriteLine(object toWrite)
		{
			if(!m_AllowLogging)
				return;

			if(RichText_Log != null)
				RichText_Log.AppendText(toWrite.ToString() + "\n");
		}

		public void UpdateFiles()
		{
			try
			{
				WriteLine("Local version: " + Start.m_Version);

				WebClient wc = new WebClient();
				string versionstr;
				using(System.IO.StreamReader sr = new System.IO.StreamReader(wc.OpenRead(baseurl + "version2.txt")))
				{
					versionstr = sr.ReadLine();
				}
				Version remoteversion = new Version(versionstr);
				WriteLine("Remote version: " + remoteversion);

				if(Start.m_Version < remoteversion)
				{
					foreach(string str in m_Files)
					{
						WriteLine("Updating: " + str);
						wc.DownloadFile(baseurl + str, str);
					}
				}
				wc.Dispose();
				WriteLine("Update complete");
			}
			catch(Exception e)
			{
				WriteLine("Update failed:");
				WriteLine(e);
			}
			this.Button_Ok.Enabled = true;
		}

		private void Button_Ok_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void frmUpdate_Load(object sender, System.EventArgs e)
		{
			ThreadStart updatestart = new ThreadStart(UpdateFiles);
			Thread updatethread = new Thread(updatestart);
			updatethread.Name = "Update";
			updatethread.Start();
		}

		private void frmUpdate_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			m_AllowLogging = false;
		}
	}
}
