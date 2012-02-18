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
// File:    LogEngine.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Threading;
using System.Collections;
using System.IO;

namespace NZB_O_Matic
{
	public class CEventLogEngine : IDisposable
	{
		protected AutoResetEvent evt;
		protected Queue store;
		protected Thread writeThread = null;
		protected bool bThreadQuit = false;
		protected string filename;
		protected bool bEnabled = false;

		public bool Enabled
		{
			get
			{
				return bEnabled;
			}
			set
			{
				bEnabled = value;
			}
		}

		protected bool isMessage()
		{
			lock (store.SyncRoot)
				if (store.Count > 0) return true;
			return false;
		}

		protected string getMessage()
		{
			lock (store.SyncRoot)
				if (store.Count > 0) return (string)store.Dequeue();
			return null;
		}

		protected void WriteThread()
		{
			StreamWriter sw = null;

			while( !bThreadQuit)
			{
				evt.WaitOne();

				if( !bThreadQuit)
				try 
				{
					sw = new StreamWriter(filename, true, System.Text.Encoding.ASCII);

					lock (store.SyncRoot)
					{
						while (isMessage() == true)
						{
							string sMsg = getMessage();
							sMsg = sMsg.Replace( "\r", "");
							sMsg = sMsg.Replace( "\n", "");
							sw.WriteLine(sMsg);
						}
					}
				
					sw.Close();
				}
				catch (Exception)
				{
					if (sw != null)
						sw.Close();
				}
			}
		}

		public CEventLogEngine(string fileName)
		{
			filename = fileName;
			store = Queue.Synchronized(new Queue(0));
			evt = new AutoResetEvent(false);
			
			writeThread = new Thread(new ThreadStart(WriteThread));
			writeThread.Name = "LogEngine writeThread";
			writeThread.Start();
		}

		~CEventLogEngine()
		{
		    Dispose();
		}

		public void Dispose()
		{
			if (writeThread != null)
			{
				bThreadQuit = true;
				evt.Set();
				writeThread.Join();
				writeThread = null;
			}
		}

		protected void AddMessage(string sText)
		{
			if( bEnabled)
			lock (store.SyncRoot)
			{
				store.Enqueue(sText);
				evt.Set();
			}
		}

		public void LogLine(string sText)
		{
			try 
			{
				AddMessage(sText);
			}
			catch { }
		}

		public void LogLine(string sFormat, params object[] args)
		{
			System.IO.StringWriter sw = new System.IO.StringWriter();
			sw.WriteLine(sFormat, args);
			string sText = sw.ToString();
			
			LogLine(sText);
		}
	}
}