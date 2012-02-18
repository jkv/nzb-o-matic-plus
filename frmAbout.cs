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
// File:    frmAbout.cs
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
	/// Summary description for frmAbout.
	/// </summary>
	public class frmAbout : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox About_Picture;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmAbout()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmAbout));
			this.About_Picture = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// About_Picture
			// 
			this.About_Picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.About_Picture.Dock = System.Windows.Forms.DockStyle.Fill;
			this.About_Picture.Image = ((System.Drawing.Image)(resources.GetObject("About_Picture.Image")));
			this.About_Picture.Location = new System.Drawing.Point(0, 0);
			this.About_Picture.Name = "About_Picture";
			this.About_Picture.Size = new System.Drawing.Size(498, 295);
			this.About_Picture.TabIndex = 0;
			this.About_Picture.TabStop = false;
			this.About_Picture.Click += new System.EventHandler(this.About_Picture_Click);
			// 
			// frmAbout
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(498, 295);
			this.Controls.Add(this.About_Picture);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmAbout";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About";
			this.ResumeLayout(false);

		}
		#endregion

		private void About_Picture_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
