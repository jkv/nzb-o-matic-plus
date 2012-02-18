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
// File:    Engine.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Windows.Forms;

[Serializable]
public class Engine : IEngine
{
	private bool m_Restart;
	private bool m_Update;
	
	public bool Restart
	{
		get
		{
			return m_Restart;
		}
		set
		{
			m_Restart = value;
		}
	}

	public bool Update
	{
		get
		{
			return m_Update;
		}
		set
		{
			m_Update = value;
		}
	}

	public void Start(string[] args)
	{
		if(Environment.CurrentDirectory != Application.StartupPath)
			Environment.CurrentDirectory = Application.StartupPath;

		NZB_O_Matic.Global.Engine = this;
		NZB_O_Matic.Global.Args = args;
		NZB_O_Matic.Global.frmMain = new NZB_O_Matic.frmMain();
		Application.Run(NZB_O_Matic.Global.frmMain);
	}

	public void Stop()
	{
		NZB_O_Matic.Global.frmMain.Close();
	}
}