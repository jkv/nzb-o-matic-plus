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
// File:    frmProgress.cs
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
	/// <summary>
	/// Summary description for frmProgress.
	/// </summary>
	public class frmProgress : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		public System.Windows.Forms.Label lAction;
		public System.Windows.Forms.ProgressBar pbProgress;
		private System.Timers.Timer tExecuteLongDurationCall;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmProgress()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmProgress));
			this.panel1 = new System.Windows.Forms.Panel();
			this.lAction = new System.Windows.Forms.Label();
			this.pbProgress = new System.Windows.Forms.ProgressBar();
			this.tExecuteLongDurationCall = new System.Timers.Timer();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tExecuteLongDurationCall)).BeginInit();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panel1.Controls.Add(this.lAction);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(272, 18);
			this.panel1.TabIndex = 0;
			// 
			// lAction
			// 
			this.lAction.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lAction.Location = new System.Drawing.Point(0, 0);
			this.lAction.Name = "lAction";
			this.lAction.Size = new System.Drawing.Size(268, 14);
			this.lAction.TabIndex = 0;
			this.lAction.Text = "Importing NZB file";
			// 
			// pbProgress
			// 
			this.pbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbProgress.Location = new System.Drawing.Point(0, 18);
			this.pbProgress.Name = "pbProgress";
			this.pbProgress.Size = new System.Drawing.Size(272, 14);
			this.pbProgress.TabIndex = 1;
			// 
			// tExecuteLongDurationCall
			// 
			this.tExecuteLongDurationCall.AutoReset = false;
			this.tExecuteLongDurationCall.Enabled = true;
			this.tExecuteLongDurationCall.SynchronizingObject = this;
			this.tExecuteLongDurationCall.Elapsed += new System.Timers.ElapsedEventHandler(this.tExecuteLongDurationCall_Elapsed);
			// 
			// frmProgress
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(272, 32);
			this.Controls.Add(this.pbProgress);
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmProgress";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Progress";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmProgress_Closing);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tExecuteLongDurationCall)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		public bool m_AllowClose = false;
		private void frmProgress_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !m_AllowClose;
		}

		//delegates/events for article part retrieval
		public delegate void DelLongDurationCall( frmProgress frm);
		public event DelLongDurationCall LongDurationCall;
		private void tExecuteLongDurationCall_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			m_AllowClose = false;
			try
			{
				if( LongDurationCall != null)
					LongDurationCall( this);
			}
			finally
			{
				m_AllowClose = true;
			}

			Close();
		}
	}
}
