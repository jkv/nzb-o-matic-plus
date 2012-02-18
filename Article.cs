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
// File:    Article.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Forms;

namespace NZB_O_Matic
{
	public class SegmentSorter : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Segment) || y.GetType() != typeof(Segment))
				return 0;

			return (x as Segment).Number - (y as Segment).Number;
		}
	}

	public class ArticleCompareSubject : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Article) || y.GetType() != typeof(Article))
				return 0;

			return System.String.Compare(((Article)x).Subject, ((Article)y).Subject);
		}
	}

	public class ArticleCompareSize : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Article) || y.GetType() != typeof(Article))
				return 0;
			
			if(((Article)x).Size < ((Article)y).Size)
				return -1;
			if(((Article)x).Size == ((Article)y).Size)
				return 0;
			if(((Article)x).Size > ((Article)y).Size)
				return 1;

			return 0;
		}
	}

	public class ArticleCompareParts : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Article) || y.GetType() != typeof(Article))
				return 0;
			
			int xnum = ((Article)x).Segments.Count, ynum = ((Article)y).Segments.Count;
			int xnumdone = 0, ynumdone = 0;
			
			foreach(Segment seg in ((Article)x).Segments)
			{
				if(seg.Downloaded == true)
					xnumdone++;
			}

			foreach(Segment seg in ((Article)y).Segments)
			{
				if(seg.Downloaded == true)
					ynumdone++;
			}

			if(xnum < ynum)
				return -1;
			if(xnum == ynum)
			{
				if(xnumdone < ynumdone)
					return -1;
				if(xnumdone == ynumdone)
					return 0;
				if(xnumdone > ynumdone)
					return 1;
			}
			if(xnum > ynum)
				return 1;

			return 0;
		}
	}

	public class ArticleCompareStatus : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Article) || y.GetType() != typeof(Article))
				return 0;
			
			if(((Article)x).Status < ((Article)y).Status)
				return -1;
			if(((Article)x).Status == ((Article)y).Status)
				return 0;
			if(((Article)x).Status > ((Article)y).Status)
				return 1;

			return 0;
		}
	}

	public class ArticleCompareDate : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Article) || y.GetType() != typeof(Article))
				return 0;
			
			if(((Article)x).Date < ((Article)y).Date)
				return -1;
			if(((Article)x).Date == ((Article)y).Date)
				return 0;
			if(((Article)x).Date > ((Article)y).Date)
				return 1;

			return 0;
		}
	}

	public class ArticleCompareGroups : IComparer
	{
		public int Compare(object x, object y)
		{
			if(x.GetType() != typeof(Article) || y.GetType() != typeof(Article))
				return 0;

			string[] xgroups = ((Article)x).Groups;
			Array.Sort(xgroups);
			string xstr = "";
			foreach(string str in xgroups)
			{
				xstr += str + " ";
			}
			xstr.Remove(xstr.Length - 2, 1);

			string[] ygroups = ((Article)y).Groups;
			Array.Sort(ygroups);
			string ystr = "";
			foreach(string str in ygroups)
			{
				ystr += str + " ";
			}
			ystr.Remove(ystr.Length - 2, 1);

			return System.String.Compare(xstr, ystr);
		}
	}

	public enum ArticleStatus { Loading = 1,
								Paused = 2,
								Queued = 3,
								Downloading = 4,
								DecodeQueued = 5,
								Decoding = 6,
								Decoded = 7,
								Error = 8,
								InternalError = 9,
								Deleted = 10,
								Incomplete = 11,
								};

	public class Article
	{
		private string m_Subject;
		private DateTime m_Date;
		private string m_Poster;
		private string[] m_Groups;

		private int m_Size;
		private ArrayList m_Segments;

		private int m_FinishedParts;

		private System.Windows.Forms.ListViewItem m_StatusItem;
		private ArticleStatus m_Status;

		private string m_Filename;
		private string m_ImportFile;

		private int m_DownloadCnt;

		public OnDownloadedDelegate OnDownloadedHandler;

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

		public ArticleStatus Status
		{
			get
			{
				return m_Status;
			}
			set
			{
				//flag that state has changed
				Global.m_SaveState = true;

				m_Status = value;

				UpdateStatus();
			}
		}

		public string Filename
		{
			get
			{
				return m_Filename;
			}
			set
			{
				m_Filename = value;
			}
		}

		
		public string ImportFile
		{
			get
			{
				return m_ImportFile;
			}
			set
			{
				m_ImportFile = value;
			}
		}
		
		
		public string Subject
		{
			get 
			{
				return m_Subject; 
			}          
		}

		public DateTime Date
		{
			get 
			{
				return m_Date; 
			}          
		}

		public string Poster
		{
			get 
			{
				return m_Poster; 
			}          
		}

		public string[] Groups
		{
			get 
			{
				return m_Groups; 
			}          
		}

		public ArrayList Segments
		{
			get 
			{
				return m_Segments; 
			}          
		}

		public int Size
		{
			get
			{
				return m_Size;
			}
		}

		public int FinishedParts
		{
			get
			{
				return m_FinishedParts;
			}
		}

		
		private delegate void UIDelegate(int num, object text);
		private UIDelegate uiDelegate;
		private ListView _listViewControl;

		public ListView ListViewControl
		{
			set
			{
				_listViewControl = value;
			}
		}

		private Article()
		{
			uiDelegate = new UIDelegate(this.SetSubItem);
		}

		public Article( string Subject, DateTime Date, string Poster, string[] Groups, string ImportFile)
			: this()
		{
			m_Subject = Subject;
			m_Date = Date;
			m_Poster = Poster;
			m_Groups = Groups;
			m_Segments = new ArrayList();
			m_Status = ArticleStatus.Loading;
			m_Filename = "";
			m_Size = 0;
			m_FinishedParts = 0;
			m_DownloadCnt = 0;
			m_ImportFile = ImportFile;

			m_StatusItem = new System.Windows.Forms.ListViewItem();
			while(m_StatusItem.SubItems.Count < Global.ArticleListCols)
				m_StatusItem.SubItems.Add("");
			m_StatusItem.Tag = this;

			UpdateStatus();

			OnDownloadedHandler = new OnDownloadedDelegate(DownloadedHandler);
		}

		public void IncDownloadCnt()
		{
			lock( this)
			{
				m_DownloadCnt++;

				Status = ArticleStatus.Downloading;
			}
		}

		public void DecDownloadCnt()
		{
			lock( this)
			{
				if( m_DownloadCnt > 0) // In theory this should never happen
					m_DownloadCnt--;

				if( m_DownloadCnt == 0)
					if( Status == ArticleStatus.Downloading)
						Status = ArticleStatus.Queued;
			}
		}

		public void AddSegment( int Number, int Bytes, string ArticleID)
		{
			Segment segment = new Segment( this, Number, Bytes, ArticleID);
			segment.Article = this;
			m_Size += segment.Bytes;
			m_Segments.Add( segment);

			if(segment.Downloaded)
				DownloadedHandler(segment);
			else
				segment.OnDownloaded += OnDownloadedHandler;
		}

		public void DownloadedHandler(Segment segment)
		{
			segment.OnDownloaded -= OnDownloadedHandler;
			segment.Article.IncreaseFinishedParts();
		}

		public void IncreaseFinishedParts()
		{
			lock( this)
			{
				m_FinishedParts++;
			}

			if(m_FinishedParts == m_Segments.Count)
				if(m_Status != ArticleStatus.Loading)
				{
					m_Status = ArticleStatus.DecodeQueued;
					Decoder.DecodeQueue.Enqueue(this);
				}

			UpdateStatus();
		}

		public void UpdateStatus()
		{
			SetSubItem(0, m_Subject);
			SetSubItem(1, m_Size);
			SetSubItem(2, m_FinishedParts.ToString() + "/" + m_Segments.Count.ToString());
			switch(m_Status)
			{
				case ArticleStatus.Decoded:
					SetSubItem(3, "Complete");
					break;
				case ArticleStatus.DecodeQueued:
					SetSubItem(3, "Decode queued");
					break;
				case ArticleStatus.Decoding:
					SetSubItem(3, "Decoding");
					break;
				case ArticleStatus.Deleted:
					SetSubItem(3, "Deleted");
					break;
				case ArticleStatus.Error:
					SetSubItem(3, "Error");
					break;
				case ArticleStatus.Incomplete:
					SetSubItem(3, "Incomplete");
					break;
				case ArticleStatus.InternalError:
					SetSubItem(3, "Internal Error");
					break;
				case ArticleStatus.Loading:
					SetSubItem(3, "Loading");
					break;
				case ArticleStatus.Queued:
					SetSubItem(3, "Queued");
					break;
				case ArticleStatus.Downloading:
					SetSubItem(3, "Downloading");
					break;
				case ArticleStatus.Paused:
					SetSubItem(3, "Paused");
					break;
				default:
					SetSubItem(3, "");
					break;
			}
			SetSubItem(4, m_Date);
			string groups = "";
			foreach(string str in m_Groups)
				groups += ", " + str;
			groups = groups.Substring(2);
			SetSubItem(5, groups);
		}

		private void SetSubItem(int num, object text)
		{
			if ((this._listViewControl != null) && (this._listViewControl.InvokeRequired))
			{
				object[] parms = { num, text };
				this._listViewControl.Invoke(this.uiDelegate, parms);
			}
			else
			{
				lock (m_StatusItem)
				{
					if (m_StatusItem.SubItems[num].Text != text.ToString())
						m_StatusItem.SubItems[num].Text = text.ToString();
				}
			}
		}
	}

	public class Segment
	{
		private Article m_Article;

		private int m_Number;
		private int m_Bytes;
		private string m_ArticleID;

		private bool m_Downloaded;

		private StringCollection m_FailedServers;

		public event OnDownloadedDelegate OnDownloaded;

		public StringCollection FailedServers
		{
			get
			{
				return m_FailedServers;
			}
		}

		public void AddFailedServer(string host)
		{
			if(!m_FailedServers.Contains(host))
				m_FailedServers.Add(host);
		}

		public bool Downloaded
		{ 
			get
			{
				return m_Downloaded;
			}
			set
			{
				m_Downloaded = value;
				if(OnDownloaded != null)
					OnDownloaded(this);
			}
		}
	
		public string ArticleID
		{
			get
			{
				return m_ArticleID;
			}
		}

		public int Bytes
		{
			get
			{
				return m_Bytes;
			}
		}

		public int Number
		{
			get
			{
				return m_Number;
			}
		}

		public Article Article
		{
			get
			{
				return m_Article;
			}
			set
			{
				m_Article = value;
			}
		}

		public Segment( Article article, int number, int bytes, string articleid)
		{
			m_FailedServers = new StringCollection();

			m_Number = number;
			m_Bytes = bytes;
			m_ArticleID = articleid;
			m_Article = article;

			Downloaded = System.IO.File.Exists( System.IO.Path.GetFullPath(Global.m_CacheDirectory + articleid));
		}
	}
}

