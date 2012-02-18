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
// File:    NNTP.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Collections;
using System.Security.Authentication;

namespace NZB_O_Matic
{
	public class NNTPClient : TcpClient
	{

		private string m_Group;
		private int m_FirstMessage;
		private int m_LastMessage;
		private int m_EstimatedMessageCount;

		private bool m_PostingIsAllowed;

		private bool m_Connected = false;

		//variables/enums for stuff I added/changed
		private int m_ArticlePointer = 0;
		public enum Mode { MODE_READER, MODE_STREAM };
		private enum ArticlePart { WHOLE, HEAD, BODY };

        private string m_Hostname;
        private Stream m_Stream;
        private bool m_UseSSL = false;

		/// <summary>
		/// Is posting to the server allowed.
		/// </summary>
		public bool PostingAllowed
		{
			get
			{
				return m_PostingIsAllowed;
			}
		}

		/// <summary>
		/// Is the underlying socket still connected?
		/// </summary>
		public new bool Connected
		{
			get
			{
				return Client.Connected && m_Connected;
			}
		}

		private string ReadLine( StreamReader r)
		{
			try
			{
				return r.ReadLine();
			}
			catch( IOException io)
			{
				if( io.InnerException is SocketException)
				{
					SocketException se = (SocketException)io.InnerException;

					if( se.ErrorCode == 10060)
						Close();

					throw se;
				}
				else
					throw io;
			}
		}

		private void WriteLine( StreamWriter w, string s)
		{
			try
			{
				w.WriteLine( s);
			}
			catch( IOException io)
			{
				if( io.InnerException is SocketException)
				{
					SocketException se = (SocketException)io.InnerException;

					if( se.ErrorCode == 10060)
						Close();

					throw se;
				}
				else
					throw io;
			}
		}

		/// <summary>
		/// Open a new connection to a NNTP server.
		/// </summary>
		/// <param name="hostname">Hostname of server.</param>
		/// <param name="port">Port to connect to on server.</param>
		public void Connect( string hostname, int port, bool ssl)
		{
			frmMain.LogWriteInfo("Connecting to (" + hostname + ":" + port + ")");
            m_Hostname = hostname;
            m_UseSSL = ssl;
			base.Connect( hostname, port);
			m_Connected = true;

			Stream s = GetStream();

			StreamReader r = new StreamReader( s);

			string response;
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);

			if( !response.StartsWith("2"))
			{
				m_Connected = false;
				base.Close(); // Close this connection

				frmMain.LogWriteInfo("Failed to connect to (" + hostname + ":" + port + ")");
				throw new Exception( response);
			}

			if( response.StartsWith("200") )
				m_PostingIsAllowed = true;

			if( response.StartsWith("201") )
				m_PostingIsAllowed = false;

			m_Group = "";

			frmMain.LogWriteInfo("Connected to (" + hostname + ":" + port + ") with code " + response.Split(' ')[0]);
		}

		/// <summary>
		/// Close connection to NNTP server.
		/// </summary>
		public new void Close()
		{
			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			w.AutoFlush = true;

			// Quit NNTP
			Global.ConnectionLog.LogLine( "{0}|W|{1}", System.Threading.Thread.CurrentThread.Name, "QUIT");
			try
			{
				WriteLine( w, "QUIT");
				WriteLine( w, "QUIT");
				WriteLine( w, "QUIT");
				WriteLine( w, "QUIT");
			}
			catch
			{
			}

			m_Connected = false;
			base.Close();
		}

		
		/// <summary>
		/// Authenticate user with connected server.
		/// </summary>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		public void AuthenticateUser( string username, string password)
		{
			string response;

			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			StreamReader r = new StreamReader( s);

			if(w == null || r == null)
				throw new Exception("Stream could not be opened.");

			w.AutoFlush = true;

			frmMain.LogWriteInfo("Authenticating with server.");
			Global.ConnectionLog.LogLine( "{0}|W|{1} {2}", System.Threading.Thread.CurrentThread.Name, "AUTHINFO USER ", username);
			WriteLine( w, "AUTHINFO USER " + username);
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
			frmMain.LogWriteInfo("Server responded with code: " + response.Split(' ')[0].Trim());
			frmMain.LogWriteInfo("Server message: " + response.Remove(0, response.Split(' ')[0].Length).Trim());
			Global.ConnectionLog.LogLine( "{0}|W|{1} **********", System.Threading.Thread.CurrentThread.Name, "AUTHINFO PASS ");
			WriteLine( w, "AUTHINFO PASS " + password);
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
			frmMain.LogWriteInfo("Server responded with code: " + response.Split(' ')[0].Trim());
			frmMain.LogWriteInfo("Server message: " + response.Remove(0, response.Split(' ')[0].Length).Trim());

			if(response.StartsWith("482") || response.StartsWith("502"))
				throw new Exception("Authentication failed");

			if(!response.StartsWith( "281"))
			{
				frmMain.LogWriteError("Standard authentication failed, trying generic.");
				Global.ConnectionLog.LogLine( "{0}|W|{1} {2} **********", System.Threading.Thread.CurrentThread.Name, "AUTHINFO GENERIC ", username);
				WriteLine( w, "AUTHINFO GENERIC " + username + " " + password);
				response = ReadLine( r);
				Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
				frmMain.LogWriteInfo("Server responded with code: " + response.Split(' ')[0].Trim());
				frmMain.LogWriteInfo("Server message: " + response.Remove(0, response.Split(' ')[0].Length).Trim());
				if(response.StartsWith("502"))
					throw new Exception("Authentication failed");
				if(!response.StartsWith( "281"))
				{
					frmMain.LogWriteError("Generic authentication failed, trying simple.");
					Global.ConnectionLog.LogLine( "{0}|W|{1}", System.Threading.Thread.CurrentThread.Name, "AUTHINFO SIMPLE");
					WriteLine( w, "AUTHINFO SIMPLE");
					response = ReadLine( r);
					Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
					frmMain.LogWriteInfo("Server responded with code: " + response.Split(' ')[0].Trim());
					frmMain.LogWriteInfo("Server message: " + response.Remove(0, response.Split(' ')[0].Length).Trim());
					Global.ConnectionLog.LogLine( "{0}|W|{1} **********", System.Threading.Thread.CurrentThread.Name, username);
					WriteLine( w, username + " " + password);
					response = ReadLine( r);
					Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
					frmMain.LogWriteInfo("Server responded with code: " + response.Split(' ')[0].Trim());
					frmMain.LogWriteInfo("Server message: " + response.Remove(0, response.Split(' ')[0].Length).Trim());
					if(response.StartsWith("452"))
						throw new Exception("Authentication failed");
					if(!response.StartsWith("250"))
					{
					}
					else
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				return;
			}

			frmMain.LogWriteError("Client failed to authenticate with server.");
			throw new Exception(response);
		}

		/// <summary>
		/// Select a newsgroup on connected server.
		/// </summary>
		/// <param name="group"></param>
		public void SelectGroup( string group)
		{
			// No need to reselect the group if we're already in it
			if( group == m_Group)
				return;
			
			string response;

			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			StreamReader r = new StreamReader( s);
			w.AutoFlush = true;

			Global.ConnectionLog.LogLine( "{0}|W|{1} {2}", System.Threading.Thread.CurrentThread.Name, "GROUP", group);
			WriteLine( w, "GROUP " + group);
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
			if( response.StartsWith( "211"))
			{
				string[] MessageNumbers;
				MessageNumbers = response.Split( ' ');

				m_EstimatedMessageCount = int.Parse( MessageNumbers[1]);
				m_FirstMessage = int.Parse( MessageNumbers[2]);
				m_LastMessage = int.Parse( MessageNumbers[3]);
				m_Group = group;
				frmMain.LogWriteInfo("Group selection (" + group + ") succeded with code: " + response.Split(' ')[0]);
			}
			else
			{
				m_EstimatedMessageCount = -1;
				m_FirstMessage = -1;
				m_LastMessage = -1;
				m_Group = "";
				frmMain.LogWriteError("Group selection (" + group + ") failed with code: " + response.Split(' ')[0]);
				throw new Exception( response);
			}
		}

		/// <summary>
		/// Download an entire article.
		/// </summary>
		/// <param name="articlenum">Number of article to download.</param>
		/// <returns></returns>
		public string DownloadArticle( int articlenum)
		{
			return GetArticlePart("ARTICLE " + articlenum, ArticlePart.WHOLE);
		}

		/// <summary>
		/// Download an entire article.
		/// </summary>
		/// <param name="articleid">Unique ID of article to download.</param>
		/// <returns></returns>
		public string DownloadArticle( string articleid)
		{
			articleid = articleid.Trim();
			if(articleid.StartsWith("<") && articleid.EndsWith(">"))
				return GetArticlePart("ARTICLE " + articleid, ArticlePart.WHOLE);

			return GetArticlePart("ARTICLE <" + articleid + ">", ArticlePart.WHOLE);
		}

		/// <summary>
		/// Download the currently selected article.
		/// </summary>
		/// <returns></returns>
		public string DownloadArticle()
		{
			if(m_Group == "")
			{
				frmMain.LogWriteError("No newsgroup selected.");
				throw new Exception("No newsgroup selected.");
			}
			if(m_ArticlePointer <= 0)
			{
				frmMain.LogWriteError("Current pointer to article is not valid.");
				throw new Exception("Current pointer to article is not valid.");
			}

			return GetArticlePart("ARTICLE", ArticlePart.WHOLE);
		}

		/// <summary>
		/// Download an article header.
		/// </summary>
		/// <param name="articlenum">Number of article header to download.</param>
		/// <returns></returns>
		public string DownloadHeader( int articlenum)
		{
			return GetArticlePart("HEAD " + articlenum, ArticlePart.HEAD);
		}

		/// <summary>
		/// Download an article header.
		/// </summary>
		/// <param name="articlenum">Unique ID of article header to download.</param>
		/// <returns></returns>
		public string DownloadHeader( string articleid)
		{
			articleid = articleid.Trim();
			if(articleid == "")
			{
				frmMain.LogWriteError("Failed to retrieve header: Empty article ID was selected.");
				throw new Exception("Failed to retrieve header: Empty article ID was selected.");
			}
			if(articleid.StartsWith("<") && articleid.EndsWith(">"))
				return GetArticlePart("HEAD " + articleid, ArticlePart.HEAD);

			return GetArticlePart("HEAD <" + articleid + ">", ArticlePart.HEAD);
		}

		/// <summary>
		/// Download header of currently selected article.
		/// </summary>
		/// <returns></returns>
		public string DownloadHeader()
		{
			if(m_Group == "")
			{
				frmMain.LogWriteError("Failed to retrieve header: No newsgroup selected.");
				throw new Exception("Failed to retrieve header: No newsgroup selected.");
			}
			if(m_ArticlePointer <= 0)
			{
				frmMain.LogWriteError("Failed to retrieve header: Current pointer to article is not valid.");
				throw new Exception("Failed to retrieve header: Current pointer to article is not valid.");
			}

			return GetArticlePart("HEAD", ArticlePart.HEAD);
		}

		/// <summary>
		/// Download an article body.
		/// </summary>
		/// <param name="articlenum">Number of article body to download.</param>
		/// <returns></returns>
		public string DownloadBody( int articlenum)
		{
			return GetArticlePart("BODY " + articlenum, ArticlePart.BODY);
		}

		/// <summary>
		/// Download an article body.
		/// </summary>
		/// <param name="articlenum">Unique ID of article body to download.</param>
		/// <returns></returns>
		public string DownloadBody( string articleid)
		{
			articleid = articleid.Trim();
			if(articleid == "")
			{
				frmMain.LogWriteError("Failed to retrieve body: Empty article ID was selected.");
				throw new Exception("Failed to retrieve body: Empty article ID was selected.");
			}
			if(articleid.StartsWith("<") && articleid.EndsWith(">"))
				return GetArticlePart("BODY " + articleid, ArticlePart.BODY);

			return GetArticlePart("BODY <" + articleid + ">", ArticlePart.BODY);
		}

		/// <summary>
		/// Download header of currently selected article.
		/// </summary>
		/// <returns></returns>
		public string DownloadBody()
		{
			if(m_Group == "")
			{
				frmMain.LogWriteError("Failed to retrieve body: No newsgroup selected.");
				throw new Exception("Failed to retrieve body: No newsgroup selected.");
			}
			if(m_ArticlePointer <= 0)
			{
				frmMain.LogWriteError("Failed to retrieve body: Current pointer to article is not valid.");
				throw new Exception("Failed to retrieve body: Current pointer to article is not valid.");
			}

			return GetArticlePart("BODY", ArticlePart.BODY);
		}

		//delegates/events for article part retrieval
		public delegate bool OnReceivedArticleLineDelegate( int line, int bytes);
		public event OnReceivedArticleLineDelegate OnReceivedArticleLine;

		/// <summary>
		/// Internal function for retrieving article parts.
		/// </summary>
		/// <param name="towrite">String to write to server.</param>
		/// <returns></returns>
		private string GetArticlePart(string towrite, ArticlePart artpart)
		{
			string response;

			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			StreamReader r = new StreamReader( s, System.Text.Encoding.GetEncoding("iso-8859-1"));
			w.AutoFlush = true;

			Global.ConnectionLog.LogLine( "{0}|W|{1}", System.Threading.Thread.CurrentThread.Name, towrite);
			WriteLine( w, towrite);
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);

			//check positive responses
			if( ( response.StartsWith( "220") && artpart == ArticlePart.WHOLE) || 
				( response.StartsWith( "221") && artpart == ArticlePart.HEAD)  || 
				( response.StartsWith( "222") && artpart == ArticlePart.BODY))
			{
				bool Done = false;
				int LineCount = 0;

				StringBuilder Body = new StringBuilder( 256*1024 );
				do
				{
					response = ReadLine( r);

					if( OnReceivedArticleLine != null)
						Done = !OnReceivedArticleLine( LineCount+1, response.Length);

					if( response == ".")
					{
						Done = true;
					}
					else
					{
						if( response.StartsWith( ".."))
							response = response.Remove(0,1);

						Body.Append( response);
						Body.Append( "\r\n");

						LineCount++;
					}
				} while( !Done);

				return Body.ToString();
			}

			bool validResponse = true;
			try
			{
				int.Parse( response.Split(' ')[0]);
			}
			catch
			{
				validResponse = false;
			}

			if( !validResponse)
			{
				frmMain.LogWriteError("The server gave back an invalid response, closing connection.");
				Close();
			}

			if(artpart == ArticlePart.WHOLE)
				frmMain.LogWriteError("Failed to retrieve article with code: " + response.Split(' ')[0]);
			else if(artpart == ArticlePart.HEAD)
				frmMain.LogWriteError("Failed to retrieve article header with code: " + response.Split(' ')[0]);
			else if(artpart == ArticlePart.BODY)
				frmMain.LogWriteError("Failed to retrieve article body with code: " + response.Split(' ')[0]);

			throw new Exception("Unexpected response from server: " + response);
		}

		/// <summary>
		/// Set NNTP mode values.
		/// </summary>
		/// <param name="toSet">Mode to be set.</param>
		public void SetMode(Mode toSet)
		{
			string response;

			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			StreamReader r = new StreamReader( s);
			w.AutoFlush = true;

			switch(toSet)
			{
				case Mode.MODE_READER: //apparently this should be the first thing passed after 
					//connect and authentication, possible performance boost
					frmMain.LogWriteInfo("Setting client mode to reader.");
					Global.ConnectionLog.LogLine( "{0}|W|{1}", System.Threading.Thread.CurrentThread.Name, "MODE READER");
					WriteLine( w, "MODE READER");
					response = ReadLine( r);
					Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);

					if(response.StartsWith("200"))
					{
						frmMain.LogWriteInfo("Client mode was succesfully set to reader with code: " + response.Split(' ')[0]);
						m_PostingIsAllowed = true;
						return;
					}
					else if(response.StartsWith("201"))
					{
						frmMain.LogWriteInfo("Client mode was succesfully set to reader with code: " + response.Split(' ')[0]);
						m_PostingIsAllowed = false;
						return;
					}
					throw new Exception("Unexpected response from server: " + response);
				case Mode.MODE_STREAM: //this is for news servers, but added for completion (it's in RFC)
					return;
			}

			frmMain.LogWriteInfo("An invalid client mode was specified to be set.");
			throw new Exception("Specified mode to be set is invalid.");
		}

		/// <summary>
		/// Get a list of available newsgroups from connected server.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetNewsgroups()
		{
			string response;
			ArrayList groups = new ArrayList();

			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			StreamReader r = new StreamReader( s);
			w.AutoFlush = true;

			frmMain.LogWriteInfo("Retrieving newsgroups from server.");

			Global.ConnectionLog.LogLine( "{0}|W|{1}", System.Threading.Thread.CurrentThread.Name, "LIST");
			WriteLine( w, "LIST");
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
			
			if(!response.StartsWith("215"))
			{
				frmMain.LogWriteError("An unexpected response was recieved from the server: " + response);
				throw new Exception("Unexpected response from server: " + response);
			}
			
			do
			{
				response = ReadLine( r);
				if( response != null)
				{
					groups.Add(response.Trim());
				}
			} while( response != null);
			
			frmMain.LogWriteInfo("Groups were succesfully recieved, total count: " + groups.Count);
			return groups;
		}

		/// <summary>
		/// Check if an article exists on connected server.
		/// </summary>
		/// <param name="articlenum">Number of article to check.</param>
		/// <returns></returns>
		public bool ArticleExists(int articlenum)
		{
			return StatArticle("STAT " + articlenum);
		}

		/// <summary>
		/// Check if an article exists on connected server.
		/// </summary>
		/// <param name="articleid">Unique ID of article to check.</param>
		/// <returns></returns>
		public bool ArticleExists(string articleid)
		{
			articleid = articleid.Trim();
			if(articleid == "")
			{
				frmMain.LogWriteError("Failed to perform STAT: Empty article ID was selected.");
				throw new Exception("Failed to perform STAT: Empty article ID was selected.");
			}
			if(articleid.StartsWith("<") && articleid.EndsWith(">"))
			{
				return StatArticle("STAT " + articleid);
			}
			return StatArticle("STAT <" + articleid + ">");
		}

		/// <summary>
		/// Check if currently selected article exists on connected server.
		/// </summary>
		/// <returns></returns>
		public bool ArticleExists()
		{
			if(m_Group == "")
			{
				frmMain.LogWriteError("Failed to perform STAT: No newsgroup selected.");
				throw new Exception("Failed to perform STAT: No newsgroup selected.");
			}
			if(m_ArticlePointer <= 0)
			{
				frmMain.LogWriteError("Failed to perform STAT: Current pointer to article is not valid.");
				throw new Exception("Failed to perform STAT: Current pointer to article is not valid.");
			}

			return StatArticle("STAT");
		}

		/// <summary>
		/// Internal function to perform STAT on an article.
		/// </summary>
		/// <param name="towrite">String to write to server.</param>
		/// <returns></returns>
		private bool StatArticle(string towrite)
		{
			string response;
			ArrayList groups = new ArrayList();

			Stream s = GetStream();
			StreamWriter w = new StreamWriter( s);
			StreamReader r = new StreamReader( s);
			w.AutoFlush = true;

			Global.ConnectionLog.LogLine( "{0}|W|{1}", System.Threading.Thread.CurrentThread.Name, towrite);
			WriteLine( w, towrite);
			response = ReadLine( r);
			Global.ConnectionLog.LogLine( "{0}|R|{1}", System.Threading.Thread.CurrentThread.Name, response);
			if(response.StartsWith("2"))
			{
				return true;
			}
			return false;
		}

        public new Stream GetStream()
        {

            if (m_Stream == null || !Connected)
            {

                if (! m_UseSSL)
                {
                    m_Stream = base.GetStream();
                }
                else
                {

                    SslStream s = new SslStream(base.GetStream(), true);

                    try
                    {
                        s.AuthenticateAsClient(m_Hostname);
                        m_Stream = s;

                    }
                    catch (AuthenticationException e)
                    {
                        Console.WriteLine("SSL Connection Failure: {0}", e.Message);
                        Close();
                        m_Stream = null;
                    }

                }
            }

            return m_Stream;
        }
	}
}
