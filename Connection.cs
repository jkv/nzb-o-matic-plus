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
// File:    Connection.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.Threading;
using System.Net.Sockets;

namespace NZB_O_Matic
{
	/// <summary>
	/// A connection class to handle a NNTP connection.
	/// </summary>
	public class Connection
	{
		private Server m_Server;
		private int m_ID;

		private NNTPClient m_NNTP;

		private string m_Hostname;
		private int m_Port;
        private bool m_UseSSL;

		private bool m_RequiresLogin;
		private string m_Username;
		private string m_Password;

		private bool m_Connected;

		private Thread m_DownloadThread;

		private System.Windows.Forms.ListViewItem m_StatusItem;

		private double m_Speed;
		private bool m_Failed;
		private int m_ConnectAttempts;
		private int m_ConnectWait;
		private DateTime m_LastUpdate;

		private string m_Status;

		/// <summary>
		/// An event to handle failed segments.
		/// </summary>
		public event OnFailedSegmentDelegate OnFailedSegment;

		/// <summary>
		/// A ListViewItem that shows the connection's status.
		/// </summary>
		public System.Windows.Forms.ListViewItem StatusItem
		{
			get
			{
				lock(m_StatusItem)
				{
					return m_StatusItem;
				}
			}
			set
			{
				lock(m_StatusItem)
				{
					m_StatusItem = value;
				}
			}
		}

		/// <summary>
		/// Get the current download speed of the connection.
		/// </summary>
		public double Speed
		{
			get
			{
				return m_Speed;
			}
		}

		/// <summary>
		/// Gets if a connection has failed.
		/// </summary>
		public bool Failed
		{
			get
			{
				return m_Failed;
			}
		}

		/// <summary>
		/// Gets and sets the number of connection attempts made.
		/// </summary>
		public int ConnectAttempts
		{
			get
			{
				return m_ConnectAttempts;
			}
			set
			{
				m_ConnectAttempts = value;
			}
		}

		/// <summary>
		/// Gets and sets the delay to use between connection attempts.
		/// </summary>
		public int ConnectWait
		{
			get
			{
				return m_ConnectWait;
			}
			set
			{
				m_ConnectWait = value;
			}
		}

		/// <summary>
		/// Gets the number of bytes received for the connections lifetime.
		/// </summary>
		public long LifetimeBytesReceived
		{
			get
			{
				return m_LifetimeBytesReceived;
			}
		}

		/// <summary>
		/// Gets and sets the server the connection belongs to.
		/// </summary>
		public Server Server
		{
			get
			{
				return m_Server;
			}
			set
			{
				m_Server = value;
			}
		}

		/// <summary>
		/// Internal function for setting StatusItem sub fields.
		/// </summary>
		/// <param name="num">Item number</param>
		/// <param name="text">Value to set</param>
		private void SetSubItem(int num, object text)
		{
			lock(m_StatusItem)
			{
				if(m_StatusItem.SubItems[num].Text != text.ToString())
					m_StatusItem.SubItems[num].Text = text.ToString();
			}
		}

		/// <summary>
		/// Constructor for Connection
		/// </summary>
		/// <param name="id">Connection id</param>
		/// <param name="hostname">Hostname to connect to</param>
		/// <param name="port">Port to connect to</param>
		/// <param name="requireslogin">Server requires authentication</param>
		/// <param name="username">Username to log in with ("" for none)</param>
		/// <param name="password">Password to log in with ("" for none)</param>
		public Connection( int id, string hostname, int port, bool requireslogin, string username, string password, bool ssl)
		{
			m_ID = id;

			m_Hostname = hostname;
			m_Port = port;
            m_UseSSL = ssl;

			m_RequiresLogin = requireslogin;
			m_Username = username;
			m_Password = password;

			m_Connected = false;
			m_BytesReceivedHistory = new long[16];
			m_TimeReceivedHistory = new long[16];

			m_Status = "Disconnected";

			m_StatusItem = new System.Windows.Forms.ListViewItem();
			while(m_StatusItem.SubItems.Count < Global.ConnectionListCols)
				m_StatusItem.SubItems.Add("");
			m_StatusItem.Tag = this;
		}	
		
		~Connection()
		{
				Disconnect();
		}


		public void UpdateStatus()
		{
			double time;
			try
			{
				time = DateTime.Now.Subtract(m_LastUpdate).Duration().TotalMilliseconds;
			}
			catch
			{
				time = 500;
			}

			double d, t;
				
			// Store the bytes received in the history array
			m_BytesReceivedHistory[m_BytesReceivedHistoryCnt] = m_BytesReceived;
			m_TimeReceivedHistory[m_BytesReceivedHistoryCnt] = System.Convert.ToInt64(time);
			m_BytesReceivedHistoryCnt++;
			if( m_BytesReceivedHistoryCnt >= 16)
				m_BytesReceivedHistoryCnt = 0;
			// Get all the bytes received in the last 4s so we get a nice bytes/second
			d = 0;
			foreach( long BytesReceived in m_BytesReceivedHistory)
				d += BytesReceived;

			t = 0;
			foreach( long TimeReceived in m_TimeReceivedHistory)
				t += TimeReceived;

            if (d > 0)
			    d /= t;
            if (d > 0)
			    d /= 1.024; // Convert to kb/s

			m_Speed = d;

			m_LastUpdate = DateTime.Now;
			m_BytesReceived = 0;

			string progress = "";
			string speed = "";

			// Update the status
			if(Global.m_Connected)
			{
				if(m_TotalBytesReceived == 0
                    || m_Size == 0)
				{
					progress = "0%";
				}
				else
				{
					progress = ((long)(m_TotalBytesReceived / (m_Size / 100))).ToString() + "%";
				}
				speed = String.Format("{0:0.0} Kb/s", d);
			}

			SetSubItem(0, m_Server.Hostname);
			SetSubItem(1, m_ID);
			SetSubItem(2, m_Status);
			if(m_Status.StartsWith("Downloading:"))
			{
				SetSubItem(3, progress);
				SetSubItem(4, speed);
			}
			else
			{
				SetSubItem(3, "");
				SetSubItem(4, "");
			}
		}

		internal void Connect()
		{
			m_ConnectWait = 0;
			m_ConnectAttempts++;

			if(m_Connected)
			{
				m_ConnectAttempts = 0;
				return;
			}

			for( m_BytesReceivedHistoryCnt = 0; m_BytesReceivedHistoryCnt < 16; m_BytesReceivedHistoryCnt++)
			{
				m_BytesReceivedHistory[m_BytesReceivedHistoryCnt] = 0;
				m_TimeReceivedHistory[m_BytesReceivedHistoryCnt] =0;
			}

			m_BytesReceivedHistoryCnt = 0;
			
			m_Connected = true;
			m_Failed = false;

			try
			{
				m_DownloadThread = new Thread( new ThreadStart(this.Initialize));
				m_DownloadThread.Name = m_Hostname + "," + m_Port.ToString() + "," + m_ID.ToString();
				m_DownloadThread.Start();
				m_ConnectAttempts = 0;
			}
			catch
			{
				m_Connected = false;
				m_Failed = true;
			}
		}

		private long m_BytesReceived;

		private int m_BytesReceivedHistoryCnt;
		private long[] m_BytesReceivedHistory;
		private long[] m_TimeReceivedHistory;

		private long m_Size;
		private long m_TotalBytesReceived;

		private long m_LifetimeBytesReceived;

		private bool DownloadProgress( int line, int bytes)
		{
			m_BytesReceived += bytes;
			m_TotalBytesReceived += bytes;
			m_LifetimeBytesReceived += bytes;

			return m_Connected;
		}

		private void Initialize()
		{
			m_NNTP = new NNTPClient();
			m_NNTP.OnReceivedArticleLine += new NZB_O_Matic.NNTPClient.OnReceivedArticleLineDelegate(DownloadProgress);
			m_NNTP.SendTimeout = 60000;
			m_NNTP.ReceiveTimeout = 60000;
			try
			{
				m_Status = "Connecting...";
				m_NNTP.Connect( m_Hostname, m_Port, m_UseSSL);

				if(m_RequiresLogin)
				{
					m_Status = "Authenticating...";
					if(m_NNTP != null)
						m_NNTP.AuthenticateUser( m_Username, m_Password);
				}
			}
			catch(Exception e)
			{
				Global.ConnectionLog.LogLine( "{0}|E|{1}|{2}", System.Threading.Thread.CurrentThread.Name, e.Message, e.ToString());

				m_Status = "Error: " + e.Message;

				try
				{
					m_NNTP.Close();
				}
				catch
				{
				}

				m_Connected = false;
				m_Failed = true;
				return;
			}

			try
			{
				m_NNTP.SetMode(NNTPClient.Mode.MODE_READER);
			}
			catch(Exception e)
			{
				Global.ConnectionLog.LogLine( "{0}|E|{1}|{2}", System.Threading.Thread.CurrentThread.Name, e.Message, e.ToString());
			}

			Handler();
		}

		private void Handler()
		{
			while( m_Connected && m_NNTP.Connected)
			{
				Download();
			}

			try
			{
				if( m_Connected && !m_NNTP.Connected)
				{
					m_Connected = false;
					m_Failed = true;
				}

				if( m_NNTP.Connected)
					m_NNTP.Close();

			}
			catch
			{
			}
		}

		private void Download()
		{
			Segment segment;

			try
			{
				segment = (Segment)m_Server.DownloadQueue.Dequeue();
			}
			catch
			{
				try
				{
					segment = (Segment)m_Server.ServerGroup.DownloadQueue.Dequeue();
				}
				catch
				{
					m_Status = "Waiting";
					segment = null;

					Thread.Sleep( 250);
				}
			}

			if( segment != null)
			{
				//If sever is disabled, pass the segment on
				if(!m_Server.Enabled)
					OnFailedSegment(this, segment, FailedStatus.ServerDisabled);

				m_Status = "Picked up: " + segment.ArticleID;

				// Check if article is flagged as deleted
				if(segment.Article.Status == ArticleStatus.Deleted)
					return;

				segment.Article.IncDownloadCnt(); // This will make the article be marked as 'downloading'
				try
				{
					// Try to open the group needed for the segment
					bool IsGroupValid = false;
					if( m_Server.NeedsGroup)
					{
						foreach( string Group in segment.Article.Groups)
						{
							try
							{
								m_Status = "Opening: " + Group;
								m_NNTP.SelectGroup( Group);
								IsGroupValid = true;
							}
							catch(Exception e)
							{
								Global.ConnectionLog.LogLine( "{0}|E|{1}|{2}", System.Threading.Thread.CurrentThread.Name, e.Message, e.ToString());
								if( m_NNTP.Connected)
								{
									m_Status = "Failed: " + Group;
								}
								else
								{
									m_Status = "Timeout: " + Group;
								}
							}

							if( IsGroupValid)
								break;
						}
					}
					else
					{
						// Server doesnt need a 'GROUP' comment, just assume its right
						IsGroupValid = true;
					}

					// If the group is valid, download the article
					if( IsGroupValid)
					{
						string Article;
						try
						{
							m_LastUpdate = DateTime.Now;
							m_TotalBytesReceived = 0;
							m_Size = segment.Bytes;

							m_Status = "Downloading: [" + segment.Number + "/" + segment.Article.Segments.Count + "] " + segment.Article.Subject;
							Article = m_NNTP.DownloadBody(segment.ArticleID);
						}
						catch(Exception e)
						{
							Global.ConnectionLog.LogLine( "{0}|E|{1}|{2}", System.Threading.Thread.CurrentThread.Name, e.Message, e.ToString());
							Article = "";
						}

						// Did we download the Article correctly ? (if we disconnected half-way, dont save)
						if( Article != "" && m_Connected)
						{
							if(segment.Article.Status != ArticleStatus.Deleted)
							{
								// We did, save the article
								System.IO.StreamWriter sw = new System.IO.StreamWriter( System.IO.Path.GetFullPath(Global.m_CacheDirectory +  segment.ArticleID), false, System.Text.Encoding.GetEncoding("iso-8859-1"));
								sw.Write( Article);
								sw.Close();

								segment.Downloaded = true;
							}
						}
						else
						{
							// No we didnt, add it to the failed queue
							if( m_NNTP.Connected && m_Connected)
							{
								OnFailedSegment(this, segment, FailedStatus.NotFound);

								m_Status = "Failed: " + segment.ArticleID;
								frmMain.LogWriteError( Thread.CurrentThread.Name + " failed " + segment.ArticleID + " [article problem]");
							}
							else
							{
								OnFailedSegment(this, segment, FailedStatus.Disconnected);

								if( m_Connected)
								{
									m_Status = "Timeout";
								}
								else
								{
									m_Status = "Disconnected";
								}

								frmMain.LogWriteError( Thread.CurrentThread.Name + " disconnected");
							}
						}
					}
					else
					{
						// None of the groups are known to this server
						// Queue the segment as failed
						if( m_NNTP.Connected)
						{
							OnFailedSegment(this, segment, FailedStatus.NotFound);
							frmMain.LogWriteError( Thread.CurrentThread.Name + " failed " + segment.ArticleID + " [group problem]");
						}
						else
						{
							OnFailedSegment(this, segment, FailedStatus.Disconnected);
							frmMain.LogWriteError( Thread.CurrentThread.Name + " failed " + segment.ArticleID + " [timeout]");
						}
					}
				}
				finally
				{
					segment.Article.DecDownloadCnt(); // this will mark the article as 'queued'
				}
			}
		}

		internal void Disconnect()
		{
			if(!m_Connected)
				return;

			m_Status = "Disconnected";
			m_Connected = false;
			m_DownloadThread = null;
		}
	}
}
