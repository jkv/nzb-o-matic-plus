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
// File:    Server.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.Threading;

namespace NZB_O_Matic
{
	/// <summary>
	/// Server is a collection of connections which will all download simultaneously.
	/// </summary>
	public class Server
	{
		private ServerGroup m_ServerGroup;

		private string m_Hostname;
		private int m_Port;

		private bool m_RequiresLogin;
		private string m_Username;
		private string m_Password;

		private bool m_NeedsGroup;
        private bool m_UseSSL;

		private bool m_Connected;
		private int m_NoConnections;
		private ArrayList m_Connections;
		private ArrayQueue m_DownloadQueue;
		private bool m_Enabled;

		public event OnFailedSegmentDelegate OnFailedSegment;
		public OnFailedSegmentDelegate OnFailedSegmentHandler;

		private System.Windows.Forms.ListViewItem m_StatusItem;

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

		public bool Connected
		{
			get
			{
				return m_Connected;
			}
		}

		public string Hostname
		{
			get 
			{
				return m_Hostname;
			}
		}

		public int Port
		{
			get 
			{
				return m_Port;
			}
		}

		public int NoConnections
		{
			get 
			{
				return m_NoConnections;
			}
		}

		public bool RequiresLogin
		{
			get 
			{
				return m_RequiresLogin;
			}
		}

		public string Username
		{
			get 
			{
				return m_Username;
			}
		}

		public string Password
		{
			get 
			{
				return m_Password;
			}
		}

		public ServerGroup ServerGroup
		{
			get
			{
				return m_ServerGroup;
			}
			set
			{
				m_ServerGroup = value;
			}
		}

		public double Speed
		{
			get
			{
				double d = 0;
				foreach( Connection connection in m_Connections)
				{
					d += connection.Speed;
				}
				return d;
			}
		}

		public long LifetimeBytesReceived
		{
			get
			{
				long l = 0;
				foreach( Connection connection in m_Connections)
				{
					l += connection.LifetimeBytesReceived;
				}
				return l;
			}
		}

		public bool Enabled
		{
			get
			{
				return m_Enabled;
			}
			set
			{
				m_Enabled = value;
			}
		}

		public bool NeedsGroup
		{
			get
			{
				return m_NeedsGroup;
			}
			set
			{
				m_NeedsGroup = value;
			}
		}

        public bool UseSSL
        {
            get
            {
                return m_UseSSL;
            }
            set
            {
                m_UseSSL = value;
            }
        }

		/// <summary>
		/// Gets/sets the download queue
		/// </summary>
		public ArrayQueue DownloadQueue
		{
			get 
			{
				return m_DownloadQueue;
			}
			set
			{
				m_DownloadQueue = value;
			}
		}

		public ArrayList Connections
		{
			get 
			{
				return m_Connections;
			}
		}

		/// <summary>
		/// Create a new server, each server must belong to a servergroup
		/// </summary>
		/// <param name="hostname">Hostname of the server</param>
		/// <param name="port">Port of the server</param>
		/// <param name="noconnections">No. of connections</param>
		/// <param name="requireslogin">Requires login</param>
		/// <param name="username">If login is required, username</param>
		/// <param name="password">If login is required, password</param>
		public Server( string hostname, int port, int noconnections, bool requireslogin, string username, string password, bool needsgroup, bool usessl)
		{
			m_Enabled = true;

			m_Hostname = hostname;
			m_Port = port;

			m_RequiresLogin = requireslogin;
			m_Username = username;
			m_Password = password;

			m_NeedsGroup = needsgroup;
            m_UseSSL = usessl;

			m_NoConnections = noconnections;
			m_Connections = new ArrayList();
			
			m_DownloadQueue = new ArrayQueue();

			m_StatusItem = new System.Windows.Forms.ListViewItem();

			while(m_StatusItem.SubItems.Count < Global.ServerListCols)
				m_StatusItem.SubItems.Add("");
			m_StatusItem.Tag = this;

			OnFailedSegmentHandler = new OnFailedSegmentDelegate(FailedSegmentHandler);

			for( int i = 0; i < noconnections; i ++)
			{
				Connection connection = new Connection(i+1, hostname, Port, RequiresLogin, Username, Password, m_UseSSL);
				connection.Server = this;
				connection.OnFailedSegment += OnFailedSegmentHandler;
				m_Connections.Add( connection);
			}

			m_Connected = false;
		}

		~Server()
		{
			foreach(Connection connection in m_Connections)
			{
				if(connection.StatusItem.ListView != null)
					connection.StatusItem.Remove();
				connection.Disconnect();
				connection.OnFailedSegment -= OnFailedSegmentHandler;
			}
			m_Connections.Clear();
			if(m_StatusItem.ListView != null)
				m_StatusItem.Remove();
		}

		public void UpdateStatus()
		{
			SetSubItem(0, m_ServerGroup.ServerManager.m_ServerGroups.IndexOf(m_ServerGroup) + 1);
			SetSubItem(1, m_Hostname);
			SetSubItem(2, m_Port);
			SetSubItem(3, m_NoConnections);
			SetSubItem(4, m_RequiresLogin);
			SetSubItem(5, m_Username);

			string star = "";
			for(int i = 0; i < m_Password.Length; i++)
				star += "*";
			SetSubItem(6, star);

            string ssl = "";
            if (m_UseSSL)
            {
                ssl = "SSL";
            }
            SetSubItem(7, ssl);

			lock(m_StatusItem)
			{
				if(m_StatusItem.Checked != m_Enabled)
					m_StatusItem.Checked = m_Enabled;
			}
		}

		private void SetSubItem(int num, object text)
		{
			lock(m_StatusItem)
			{
				if(m_StatusItem.SubItems[num].Text != text.ToString())
					m_StatusItem.SubItems[num].Text = text.ToString();
			}
		}

		public void FailedSegmentHandler(object sender, Segment segment, FailedStatus failedstatus)
		{
			//HANDLE FAILED SEGMENT FOR CONNECTION
			if( failedstatus != FailedStatus.Disconnected)
				segment.AddFailedServer(m_Hostname);
			OnFailedSegment(this, segment, failedstatus);
		}

		/// <summary>
		/// Opens the connections on the server and starts all downloads
		/// </summary>
		public void Connect()
		{
			if(!m_Enabled)
				return;

			m_Connected = true;

			foreach( Connection connection in m_Connections)
				connection.Connect();
		}

		/// <summary>
		/// Closes the connections on the server and stops all downloads
		/// </summary>
		public void Disconnect()
		{
			foreach( Connection connection in m_Connections)
				connection.Disconnect();

			m_Connected = false;
		}
	}
}
