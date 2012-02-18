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
// File:    ServerGroup.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.Windows.Forms;

namespace NZB_O_Matic
{
	/// <summary>
	/// Server group is a collection of servers which will all download simultaneously.
	/// If a segment is not found on one server in this group, it will try all others before
	/// the segment gets dropped to the next server group.
	/// </summary>
	public class ServerGroup
	{
		private ServerManager m_ServerManager;

		private bool m_Connected;

		// A segment that needs to be downloaded by any of the servers in the group
		// should be placed in this download queue
		private ArrayQueue m_DownloadQueue;

		public event OnFailedSegmentDelegate OnFailedSegment;
		public OnFailedSegmentDelegate OnFailedSegmentHandler;

		// The list of servers in this group
		private ArrayList m_Servers;

		public bool Connected
		{
			get
			{
				return m_Connected;
			}
		}

		public double Speed
		{
			get
			{
				double d = 0;
				foreach(Server server in m_Servers)
				{
					d += server.Speed;
				}
				return d;
			}
		}

		public long LifetimeBytesReceived
		{
			get
			{
				long l = 0;
				foreach(Server server in m_Servers)
				{
					l += server.LifetimeBytesReceived;
				}
				return l;
			}
		}

		/// <summary>
		/// Gets the server list
		/// </summary>
		public ArrayList Servers
		{
			get
			{
				return m_Servers;
			}
		}

		/// <summary>
		/// Gets/sets the download queue, cant change the queue's once connected
		/// </summary>
		public ArrayQueue DownloadQueue
		{
			get 
			{
				if(m_DownloadQueue == null)
				{
					m_DownloadQueue = new ArrayQueue();
				}
				return m_DownloadQueue;
			}
			set
			{
				m_DownloadQueue = value;
			}
		}

		public ServerManager ServerManager
		{
			get
			{
				return m_ServerManager;
			}
			set
			{
				m_ServerManager = value;
			}
		}

		/// <summary>
		/// Create a new server group
		/// </summary>
		public ServerGroup()
		{
			m_Servers = new ArrayList();
			m_Connected = false;
			OnFailedSegmentHandler = new OnFailedSegmentDelegate(FailedSegmentHandler);
		}

		/// <summary>
		/// Adds a server to the group
		/// </summary>
		public Server AddServer( Server server)
		{
			server.ServerGroup = this;
			server.OnFailedSegment += OnFailedSegmentHandler;
			m_Servers.Add(server);
			return server;
		}

		public Server RemoveServer(Server server)
		{
			if(m_Servers.Contains(server) && server != null)
			{
				server.Disconnect();
				server.StatusItem.Remove();
				server.OnFailedSegment -= OnFailedSegmentHandler;
				server.ServerGroup = null;
				m_Servers.Remove(server);
			}
			return server;
		}

		/// <summary>
		/// Opens the connections on all servers and starts downloading segments
		/// </summary>
		public void Connect()
		{
			m_Connected = true;

			foreach( Server server in m_Servers)
				server.Connect();
		}

		/// <summary>
		/// Closes the connections on all servers and stops all downloads
		/// </summary>
		public void Disconnect()
		{
			foreach( Server server in m_Servers)
				server.Disconnect();

			m_Connected = false;
		}

		private void FailedSegmentHandler(object sender, Segment segment, FailedStatus failedstatus)
		{
			//HANDLE FAILED SEGMENT FOR SERVER
			foreach(Server server in m_Servers)
				if(!segment.FailedServers.Contains(server.Hostname) && server.Enabled)
				{
					server.DownloadQueue.Enqueue(segment);
					return;
				}

			OnFailedSegment(this, segment, failedstatus);
		}

		public bool EnqueueSegment(Segment segment)
		{
			int enabled = 0;
			foreach(Server server in m_Servers)
			{
				if(server.Enabled)
					enabled++;
			}
			if(enabled > 0)
			{
				m_DownloadQueue.Enqueue(segment);
				return true;
			}
			return false;
		}
	}
}
