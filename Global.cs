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
// File:    Global.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace NZB_O_Matic
{
	public enum FailedStatus { NotFound = 1,
							   ServerDisabled = 2,
							   Disconnected = 3 };

	// Segment failure delegate
	public delegate void OnFailedSegmentDelegate(object sender, Segment segment, FailedStatus failedstatus);

	// Segment downloaded delegate
	public delegate void OnDownloadedDelegate(Segment segment);

	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global
	{
		// Start arguments
		public static string[] Args;

		// Main form
		public static frmMain frmMain;

		// Mutex which stops NoM from starting more then once
		public static Mutex m_RunMutex;

		// Are we connected?
		public static bool m_Connected = false;

		// Option values
		public static OptionValues m_Options;

		// Listbox to use for logging
		public static System.Windows.Forms.ListBox m_StatusLog;

		// Does state need to be saved?
		public static bool m_SaveState = false;

		// Allow log to be written to
		public static bool m_AllowLogging = false;

		// Exit on completion?
		public static bool m_ExitComplete = false;

		public static string m_CurrentDirectory = string.Copy(System.Environment.CurrentDirectory) + @"\";

		//penndu
		public static string m_CacheDirectory;
		public static string m_DownloadDirectory;
		public static string m_DataDirectory;

		// Number of collums for status items
		public const int ConnectionListCols = 5;
		public const int ServerListCols = 8;
		public const int ArticleListCols = 6;

		public static string Name;
		public static string Version;

		// Program loader reference
		public static IEngine Engine;

		public static CEventLogEngine ConnectionLog;
		

	}
}
