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
// File:    ServerManger.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.Xml;
using System.Windows.Forms;

namespace NZB_O_Matic
{
	/// <summary>
	/// Summary description for ServerManager.
	/// </summary>
	public class ServerManager
	{
		public ArrayList m_Articles;
		public ArrayList m_ServerGroups;
		public ArrayList m_DisabledServers;

		public OnFailedSegmentDelegate OnFailedSegmentHandler;

		public long LifetimeBytesReceived
		{
			get
			{
				long l = 0;
				foreach(ServerGroup servergroup in m_ServerGroups)
					l += servergroup.LifetimeBytesReceived;
				return l;
			}
		}

		public double Speed
		{
			get
			{
				double d = 0;
				foreach( ServerGroup servergroup in m_ServerGroups)
					d += servergroup.Speed;

				return d;
			}
		}

		public double Completion
		{
			get
			{
				double done = 0;
				double totseg = 0;
				for(int i = 0; i < m_Articles.Count; i++)
				{
					if (((Article)m_Articles[i]).Status != ArticleStatus.Paused &&
						((Article)m_Articles[i]).Status != ArticleStatus.Deleted )
					{
						for(int j = 0; j < ((Article)m_Articles[i]).Segments.Count; j++)
						{
							totseg++;
							if(((Segment)((Article)m_Articles[i]).Segments[j]).Downloaded)
								done++;
						}
					}
				}
				if(done == 0)
					return 0;

				return (done/totseg)*100.0;
			}
		}

		public double TotalMB
		{
			get
			{
				double total = 0;
				for(int i = 0; i < m_Articles.Count; i++)
				{
					if (((Article)m_Articles[i]).Status != ArticleStatus.Paused &&
						((Article)m_Articles[i]).Status != ArticleStatus.Deleted)
					total += ((Article)m_Articles[i]).Size;
				}
				return ((total/1024)/1024);
			}
		}

		public double CompletedMB
		{
			get
			{
				double total = 0;
				for(int i = 0; i < m_Articles.Count; i++)
				{
					if (((Article)m_Articles[i]).Status != ArticleStatus.Paused &&
						((Article)m_Articles[i]).Status != ArticleStatus.Deleted)
					{
						double tot = 0;
						double done = 0;
						for(int j = 0; j < ((Article)m_Articles[i]).Segments.Count; j++)
						{
							tot++;
							if(((Segment)((Article)m_Articles[i]).Segments[j]).Downloaded)
								done++;
						}
						total += (((Article)m_Articles[i]).Size * (done/tot));
					}
				}
				return ((total/1024)/1024);
			}
		}

		public double RemainingMB
		{
			get
			{
				return (TotalMB - CompletedMB);
			}
		}

		public void FailedSegmentHandler(object sender, Segment segment, FailedStatus failedstatus)
		{
			//HANDLE FAILED SEGMENT FROM SERVER GROUP
			for(int i = m_ServerGroups.IndexOf((ServerGroup)sender) + 1; i < m_ServerGroups.Count; i++)
			{
				if(m_ServerGroups[i] != null)
				{
					((ServerGroup)m_ServerGroups[i]).DownloadQueue.Enqueue(segment);
					return;
				}
			}
			
			segment.Article.Status = ArticleStatus.Incomplete;
			frmMain.LogWriteError("Segment not found on any servers: " + segment.ArticleID);
			return;
		}

		public ServerManager()
		{
			m_Articles = ArrayList.Synchronized( new ArrayList());
			m_ServerGroups = ArrayList.Synchronized( new ArrayList());
			m_DisabledServers = ArrayList.Synchronized( new ArrayList());

			OnFailedSegmentHandler = new OnFailedSegmentDelegate(FailedSegmentHandler);
		}

		public bool LoadServers(string file)
		{
			XmlDocument XmlDoc = new XmlDocument();
			try
			{
				XmlDoc.Load(System.IO.Path.GetFullPath(file));
			}
			catch
			{
				return false;
			}

			if( XmlDoc.DocumentElement == null)
				return false;

			if( XmlDoc.DocumentElement.Name == "servergroups")
			{
				foreach( XmlNode XmlGroup in XmlDoc.SelectNodes( "servergroups/servergroup"))
				{
					ServerGroup servergroup = AddServerGroup();

					foreach( XmlNode XmlServer in XmlGroup.SelectNodes( "servers/server"))
					{
						string Address = XmlServer.SelectSingleNode("address").InnerText;
						int Port = int.Parse(XmlServer.SelectSingleNode("port").InnerText);
						int Connections = int.Parse(XmlServer.SelectSingleNode("connections").InnerText);
						bool RequiresLogin = XmlServer.SelectSingleNode("login").HasChildNodes;
						string Username = "";
						string Password = "";
						if( RequiresLogin)
						{
							Username = XmlServer.SelectSingleNode("login/username").InnerText;
							Password = XmlServer.SelectSingleNode("login/password").InnerText;
						}

						bool NeedsGroup = true;
						if( XmlServer.SelectSingleNode("needsgroup") != null)
						{
							NeedsGroup = (XmlServer.SelectSingleNode("needsgroup").InnerText.ToLower() == "true");
						}

                        bool UseSSL = false;
                        if (XmlServer.SelectSingleNode("ssl") != null)
                        {
                            UseSSL = (XmlServer.SelectSingleNode("ssl").InnerText.ToLower() == "true");
                        }

						Server server = servergroup.AddServer(new Server(Address, Port, Connections, RequiresLogin, Username, Password, NeedsGroup, UseSSL));
						if(XmlServer.Attributes.GetNamedItem("enabled") != null)
							if(!bool.Parse(XmlServer.Attributes.GetNamedItem("enabled").InnerText))
								DisableServer(server);
					}
				}
			}
			return true;
		}

		public void InitializeQueues()
		{
			foreach( ServerGroup servergroup in m_ServerGroups)
			{
				if( servergroup.DownloadQueue == null)
					servergroup.DownloadQueue = new ArrayQueue();

				servergroup.DownloadQueue.Clear();
			
				foreach( Server server in servergroup.Servers)
				{
					if( server.DownloadQueue == null)
						server.DownloadQueue = new ArrayQueue();

					server.DownloadQueue.Clear();
				}
			}
		}

		public void FillQueues()
		{
			// This is the function that you'd change to fill with more inteligence	for pre-sorting
			foreach(Article article in m_Articles)
				if(article.Status == ArticleStatus.Queued || 
					     article.Status == ArticleStatus.Downloading)
			//don't forget article beeing donloading
					foreach(Segment segment in article.Segments)
						if(!segment.Downloaded)
						{
							if( segment.FailedServers != null)
                                segment.FailedServers.Clear();

							EnqueueSegment(segment);
						}
		}

		private void EnqueueSegment(Segment segment)
		{
			for(int i = 0; i < m_ServerGroups.Count; i++)
			{
				if(((ServerGroup)m_ServerGroups[i]).EnqueueSegment(segment))
					return;
			}
		}

		public void RebuildQueue()
		{
			InitializeQueues();
			FillQueues();
		}

		public void AddArticle(Article article)
		{
			m_Articles.Add(article);
			RebuildQueue();
		}

		public void MoveArticlesTop(ArrayList toMove)
		{
			toMove.Reverse();
			foreach(Article article in toMove)
			{
				m_Articles.Remove(article);
				if(m_Articles.Count > 0)
					m_Articles.Insert(0, article);
				else
					m_Articles.Add(article);				
			}
			RebuildQueue();
		}

		public void MoveArticlesUp(ArrayList toMove)
		{
			foreach(Article article in toMove)
			{
				int index = m_Articles.IndexOf(article);
				m_Articles.Remove(article);
				if(m_Articles.Count <= 0)
					m_Articles.Add(article);
				else if(index - 1 >= 0)
					m_Articles.Insert(index - 1, article);
				else if(index - 1 < 0)
					m_Articles.Insert(0, article);
			}
			RebuildQueue();
		}

		public void MoveArticlesDown(ArrayList toMove)
		{
			toMove.Reverse();
			foreach(Article article in toMove)
			{
				int index = m_Articles.IndexOf(article);
				m_Articles.Remove(article);
				if(m_Articles.Count <= 0 || index + 1 >= m_Articles.Count)
					m_Articles.Add(article);
				else
					m_Articles.Insert(index + 1, article);
			}
			RebuildQueue();
		}

		public void MoveArticlesBottom(ArrayList toMove)
		{
			foreach(Article article in toMove)
			{
				m_Articles.Remove(article);
				m_Articles.Add(article);
			}
			RebuildQueue();
		}

		public void DeleteArticle(Article article)
		{
			ArrayList segments = (ArrayList)article.Segments.Clone();

			if(Decoder.DecodeQueue.Contains(article))
				Decoder.DecodeQueue.Remove(article);

			foreach(ServerGroup servergroup in m_ServerGroups)
				foreach(Segment segment in segments)
				{
					if(servergroup.DownloadQueue != null)
						if(servergroup.DownloadQueue.Contains(segment))
							servergroup.DownloadQueue.Remove(segment);

					foreach(Server server in servergroup.Servers)
						if(server.DownloadQueue != null)
							if(server.DownloadQueue.Contains(segment))
								server.DownloadQueue.Remove(segment);
				}

			if(m_Articles.Contains(article))
			{
				article.Status = ArticleStatus.Deleted;
				article.StatusItem.Remove();
				foreach(Segment segment in segments)
				{
					if(segment.Downloaded)
					{
						try
						{
/*							if(System.IO.File.Exists(System.IO.Path.GetFullPath("Cache\\" + segment.ArticleID)))
								System.IO.File.Delete(System.IO.Path.GetFullPath("Cache\\" + segment.ArticleID));
*/
							if(System.IO.File.Exists(System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID)))
								System.IO.File.Delete(System.IO.Path.GetFullPath(Global.m_CacheDirectory + segment.ArticleID));
						}
						catch(Exception q)
						{
							Console.WriteLine(q);
						}
					}
				}
				article.Segments.Clear();
				m_Articles.Remove(article);
				return;
			}
			RebuildQueue();
		}

		public void Connect()
		{
			RebuildQueue();

			foreach(ServerGroup servergroup in m_ServerGroups)
			{
				foreach(Server server in servergroup.Servers)
				{
					foreach(Connection connection in server.Connections)
					{
						connection.ConnectAttempts = 0;
						connection.ConnectWait = 0;
					}
				}
				servergroup.Connect();
			}
		}

		public void Disconnect()
		{
			foreach(ServerGroup servergroup in m_ServerGroups)
				servergroup.Disconnect();
		}

		public void PruneQueue()
		{
			bool exit = false;
			while(!exit)
			{
				exit = true;
				foreach(Article article in m_Articles)
				{
					if(article.Status == ArticleStatus.Decoded || article.Status == ArticleStatus.Deleted)
					{
						article.StatusItem.Remove();
						m_Articles.Remove(article);
						exit = false;
						break;
					}
				}
			}
		}

		public ServerGroup AddServerGroup()
		{
			PruneServerGroups();
			ServerGroup servergroup = new ServerGroup();
			servergroup.ServerManager = this;
			servergroup.OnFailedSegment += OnFailedSegmentHandler;
			m_ServerGroups.Add(servergroup);
			return servergroup;
		}

		public ServerGroup RemoveServerGroup(ServerGroup servergroup)
		{
			if(m_ServerGroups.Contains(servergroup))
				m_ServerGroups.Remove(servergroup);
			if(servergroup != null)
			{
				servergroup.ServerManager = null;
				servergroup.OnFailedSegment -= OnFailedSegmentHandler;
			}
			return servergroup;
		}


		public void PruneServerGroups()
		{
			bool exit = false;

			while(!exit)
			{
				exit = true;
				for(int i = 0; i < m_ServerGroups.Count; i++)
				{
					if(m_ServerGroups[i] == null || ((ServerGroup)m_ServerGroups[i]).Servers.Count == 0)
					{
						RemoveServerGroup((ServerGroup)m_ServerGroups[i]);
						exit = false;
						break;
					}
				}
			}
		}

		
		public static void DisableServer(Server server)
		{
			if(!server.Enabled)
				return;

			if(server.Connected)
				server.Disconnect();

			server.Enabled = false;
			server.StatusItem.BackColor = System.Drawing.Color.Gray;
		}

		public static void EnableServer(Server server)
		{
			if(server.Enabled)
				return;

			server.Enabled = true;
			server.StatusItem.BackColor = System.Drawing.Color.White;

			if(Global.m_Connected && !server.Connected)
				server.Connect();
		}		
	}
}
