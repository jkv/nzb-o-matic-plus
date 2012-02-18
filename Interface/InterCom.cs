//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    InterCom.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

public delegate void DataReceivedEventHandler(object sender,
DataReceivedEventArgs e);

	/// <summary>
	/// A class which wraps using Windows native WM_COPYDATA
	/// message to send interprocess data between applications.
	/// This is a simple technique for interprocess data sends
	/// using Windows.  The alternative to this is to use
	/// Remoting, which requires a network card and a way
	/// to register the Remoting name of an object so it
	/// can be read by other applications.
	/// </summary>
public class CopyData : NativeWindow, IDisposable
{   
	/// <summary>
	/// Event raised when data is received on any of the channels 
	/// this class is subscribed to.
	/// </summary>
	public event DataReceivedEventHandler DataReceived;

	[StructLayout(LayoutKind.Sequential)]
		private struct COPYDATASTRUCT
	{
		public IntPtr dwData;
		public int cbData;
		public IntPtr lpData;
	}
      
	private const int WM_COPYDATA = 0x4A;
	private const int WM_DESTROY = 0x2;

	#region Member Variables
	private CopyDataChannels channels = null;
	private bool disposed = false;
	#endregion

	/// <summary>
	/// Override for a form's Window Procedure to handle WM_COPYDATA
	/// messages sent by other instances of this class.
	/// </summary>
	/// <param name="m">The Windows Message information.</param>
	protected override void WndProc (ref System.Windows.Forms.Message m )
	{
		if (m.Msg == WM_COPYDATA)
		{
			COPYDATASTRUCT cds = new COPYDATASTRUCT();
			cds = (COPYDATASTRUCT) Marshal.PtrToStructure(m.LParam,
				typeof(COPYDATASTRUCT));
			if (cds.cbData > 0)
			{
				byte[] data = new byte[cds.cbData];            
				Marshal.Copy(cds.lpData, data, 0, cds.cbData);
				MemoryStream stream = new MemoryStream(data);
				BinaryFormatter b = new BinaryFormatter();
				CopyDataObjectData cdo = (CopyDataObjectData)
					b.Deserialize(stream);
               
				if (channels.Contains(cdo.Channel))
				{
					DataReceivedEventArgs d = new
						DataReceivedEventArgs(cdo.Channel, cdo.Data, cdo.Sent);
					OnDataReceived(d);
					m.Result = (IntPtr) 1;
				}            
			}
		}
		else if (m.Msg == WM_DESTROY)
		{
			// WM_DESTROY fires before OnHandleChanged and is
			// a better place to ensure that we've cleared 
			// everything up.
			channels.OnHandleChange();
			base.OnHandleChange();
		}
		base.WndProc(ref m);
	}

	/// <summary>
	/// Raises the DataReceived event from this class.
	/// </summary>
	/// <param name="e">The data which has been received.</param>
	protected void OnDataReceived(DataReceivedEventArgs e)
	{
		DataReceived(this, e);
	}

	/// <summary>
	/// If the form's handle changes, the properties associated
	/// with the window need to be cleared up. This override ensures
	/// that it is done.  Note that the CopyData class will then
	/// stop responding to events and it should be recreated once
	/// the new handle has been assigned.
	/// </summary>
	protected override void OnHandleChange ()
	{
		// need to clear up everything we had set.
		channels.OnHandleChange();
		base.OnHandleChange();
	}

	/// <summary>
	/// Gets the collection of channels.
	/// </summary>
	public CopyDataChannels Channels
	{
		get
		{
			return this.channels;
		}
	}

	/// <summary>
	/// Clears up any resources associated with this object.
	/// </summary>
	public void Dispose()
	{
		if (!disposed)
		{
			channels.Clear();
			channels = null;
			disposed = true;
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Constructs a new instance of the CopyData class
	/// </summary>
	public CopyData()
	{
		channels = new CopyDataChannels(this);
	}

	/// <summary>
	/// Finalises a CopyData class which has not been disposed.
	/// There may be a minor resource leak if this class is finalised
	/// after the form it is associated with.
	/// </summary>
	~CopyData()
	{
		Dispose();
	}
}

	/// <summary>
	/// Contains data and other information associated with data
	/// which has been sent from another application.
	/// </summary>
public class DataReceivedEventArgs
{
	private string channelName = "";
	private object data = null;
	private DateTime sent;
	private DateTime received;

	/// <summary>
	/// Gets the channel name that this data was sent on.
	/// </summary>
	public string ChannelName
	{
		get
		{
			return this.channelName;
		}
	}
	/// <summary>
	/// Gets the data object which was sent.
	/// </summary>
	public Object Data
	{
		get
		{
			return this.data;
		}
	}
	/// <summary>
	/// Gets the date and time which at the data was sent
	/// by the sending application.
	/// </summary>
	public DateTime Sent
	{
		get
		{
			return this.sent;
		}
	}
	/// <summary>
	/// Gets the date and time which this data item as
	/// received.
	/// </summary>
	public DateTime Received
	{
		get
		{
			return this.received;
		}
	}
	/// <summary>
	/// Constructs an instance of this class.
	/// </summary>
	/// <param name="channelName">The channel that the data was received from</param>
	/// <param name="data">The data which was sent</param>
	/// <param name="sent">The date and time the data was sent</param>
	internal DataReceivedEventArgs(string channelName, object data, DateTime
		sent)
	{
		this.channelName = channelName;
		this.data = data;
		this.sent = sent;
		this.received = DateTime.Now;
	}
}

	/// <summary>
	/// A strongly-typed collection of channels associated with the CopyData
	/// class.
	/// </summary>
public class CopyDataChannels : DictionaryBase
{
	private NativeWindow owner = null;

	/// <summary>
	/// Returns an enumerator for each of the CopyDataChannel objects
	/// within this collection.
	/// </summary>
	/// <returns>An enumerator for each of the CopyDataChannel objects
	/// within this collection.</returns>
	public new System.Collections.IEnumerator GetEnumerator (  )
	{
		return this.Dictionary.Values.GetEnumerator();
	}

	/// <summary>
	/// Returns the CopyDataChannel at the specified 0-based index.
	/// </summary>
	public CopyDataChannel this[int index]
	{
		get 
		{
			CopyDataChannel ret = null;
			int i = 0;
			foreach (CopyDataChannel cdc in this.Dictionary.Values)
			{
				i++;
				if (i == index)
				{
					ret = cdc;
					break;
				}
			}
			return ret;
		}
	}
	/// <summary>
	/// Returns the CopyDataChannel for the specified channelName
	/// </summary>
	public CopyDataChannel this[string channelName]
	{
		get
		{
			return (CopyDataChannel) this.Dictionary[channelName];
		}
	}
	/// <summary>
	/// Adds a new channel on which this application can send and
	/// receive messages.
	/// </summary>
	public void Add(string channelName)
	{
		CopyDataChannel cdc = new CopyDataChannel(owner, channelName);
		this.Dictionary.Add(channelName , cdc);
	}
	/// <summary>
	/// Removes an existing channel.
	/// </summary>
	/// <param name="channelName">The channel to remove</param>
	public void Remove(string channelName)
	{
		this.Dictionary.Remove(channelName);
	}
	/// <summary>
	/// Gets/sets whether this channel contains a CopyDataChannel
	/// for the specified channelName.
	/// </summary>
	public bool Contains(string channelName)
	{
		return this.Dictionary.Contains(channelName);
	}

	/// <summary>
	/// Ensures the resources associated with a CopyDataChannel
	/// object collected by this class are cleared up.
	/// </summary>
	protected override void OnClear()
	{
		foreach (CopyDataChannel cdc in this.Dictionary.Values)
		{
			cdc.Dispose();
		}
		base.OnClear();
	}

	/// <summary>
	/// Ensures any resoures associated with the CopyDataChannel object
	/// which has been removed are cleared up.
	/// </summary>
	/// <param name="key">The channelName</param>
	/// <param name="data">The CopyDataChannel object which has
	/// just been removed</param>
	protected override void OnRemoveComplete ( Object key , System.Object
		data )
	{
		( (CopyDataChannel) data).Dispose();
		base.OnRemove(key, data);
	}

	/// <summary>
	/// If the form's handle changes, the properties associated
	/// with the window need to be cleared up. This override ensures
	/// that it is done.  Note that the CopyData class will then
	/// stop responding to events and it should be recreated once
	/// the new handle has been assigned.
	/// </summary>
	public void OnHandleChange()
	{
		foreach (CopyDataChannel cdc in this.Dictionary.Values)
		{
			cdc.OnHandleChange();
		}
	}
      
	/// <summary>
	/// Constructs a new instance of the CopyDataChannels collection.
	/// Automatically managed by the CopyData class.
	/// </summary>
	/// <param name="owner">The NativeWindow this collection
	/// will be associated with</param>
	internal CopyDataChannels(NativeWindow owner)
	{
		this.owner = owner;
	}
}

	/// <summary>
	/// A channel on which messages can be sent.
	/// </summary>
public class CopyDataChannel : IDisposable
{
	#region Unmanaged Code
	[DllImport("user32", CharSet=CharSet.Auto)]
	private extern static int GetProp(
		IntPtr hwnd , 
		string lpString);
	[DllImport("user32", CharSet=CharSet.Auto)]
	private extern static int SetProp(
		IntPtr hwnd , 
		string lpString, 
		int hData);
	[DllImport("user32", CharSet=CharSet.Auto)]
	private extern static int RemoveProp(
		IntPtr hwnd, 
		string lpString);
      
	[DllImport("user32", CharSet=CharSet.Auto)]
	private extern static int SendMessage(
		IntPtr hwnd, 
		int wMsg, 
		int wParam,             
		ref COPYDATASTRUCT lParam
		);

	[StructLayout(LayoutKind.Sequential)]
		private struct COPYDATASTRUCT
	{
		public IntPtr dwData;
		public int cbData;
		public IntPtr lpData;
	}
      
	private const int WM_COPYDATA = 0x4A;
	#endregion

	#region Member Variables
	private string channelName = "";
	private bool disposed = false;
	private NativeWindow owner = null;
	private bool recreateChannel = false;
	#endregion

	/// <summary>
	/// Gets the name associated with this channel.
	/// </summary>
	public string ChannelName
	{
		get
		{
			return this.channelName;
		}
	}

	/// <summary>
	/// Sends the specified object on this channel to any other
	/// applications which are listening.  The object must have the
	/// SerializableAttribute set, or must implement ISerializable.
	/// </summary>
	/// <param name="obj">The object to send</param>
	/// <returns>The number of recipients</returns>
	public int Send(object obj)
	{
		int recipients = 0;

		if (disposed)
		{
			throw new InvalidOperationException("Object has been disposed");
		}

		if (recreateChannel) // handle has changed
		{
			addChannel();
		}

		CopyDataObjectData cdo = new CopyDataObjectData(obj, channelName);    
              

		// Try to do a binary serialization on obj.
		// This will throw and exception if the object to
		// be passed isn't serializable.
		BinaryFormatter b = new BinaryFormatter();
		MemoryStream stream = new MemoryStream();
		b.Serialize(stream, cdo);
		stream.Flush();         

		// Now move the data into a pointer so we can send
		// it using WM_COPYDATA:
		// Get the length of the data:
		int dataSize = (int)stream.Length;
		if (dataSize > 0)
		{
			// This isn't very efficient if your data is very large.
			// First we copy to a byte array, then copy to a CoTask 
			// Mem object... And when we use WM_COPYDATA windows will
			// make yet another copy!  But if you're talking about 4K
			// or less of data then it doesn't really matter.
			byte[] data = new byte[dataSize];
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(data, 0, dataSize);
			IntPtr ptrData = Marshal.AllocCoTaskMem(dataSize);
			Marshal.Copy(data, 0, ptrData, dataSize);

			// Enumerate all windows which have the
			// channel name, send the data to each one
			EnumWindows ew = new EnumWindows();
			ew.GetWindows();

			// Send the data to each window identified on
			// the channel:
			foreach(EnumWindowsItem window in ew.Items)
			{
				if (!window.Handle.Equals(this.owner.Handle))
				{
					if (GetProp(window.Handle, this.channelName) != 0)
					{
						COPYDATASTRUCT cds = new COPYDATASTRUCT();
						cds.cbData = dataSize;
						cds.dwData = IntPtr.Zero;
						cds.lpData = ptrData;
						int res = SendMessage(window.Handle, WM_COPYDATA,
							(int)owner.Handle, ref cds);
						recipients += (Marshal.GetLastWin32Error() == 0 ? 1 : 0);
					}
				}
			}

			// Clear up the data:
			Marshal.FreeCoTaskMem(ptrData);
		}
		stream.Close();

		return recipients;
	}

	private void addChannel()
	{
		// Tag this window with property "channelName"
		SetProp(owner.Handle, this.channelName, (int)owner.Handle);

	}
	private void removeChannel()
	{
		// Remove the "channelName" property from this window
		RemoveProp(owner.Handle, this.channelName);
	}

	/// <summary>
	/// If the form's handle changes, the properties associated
	/// with the window need to be cleared up. This method ensures
	/// that it is done.  Note that the CopyData class will then
	/// stop responding to events and it should be recreated once
	/// the new handle has been assigned.
	/// </summary>
	public void OnHandleChange()
	{
		removeChannel();
		recreateChannel = true;
	}

	/// <summary>
	/// Clears up any resources associated with this channel.
	/// </summary>
	public void Dispose()
	{
		if (!disposed)
		{
			if (channelName.Length > 0)
			{
				removeChannel();
			}
			channelName = "";
			disposed = true;
			GC.SuppressFinalize(this);
		}
	}

	/// <summary>
	/// Constructs a new instance of a CopyData channel.  Called
	/// automatically by the CopyDataChannels collection.
	/// </summary>
	/// <param name="owner">The owning native window</param>
	/// <param name="channelName">The name of the channel to
	/// send messages on</param>
	internal CopyDataChannel(NativeWindow owner, string channelName)
	{
		this.owner = owner;
		this.channelName = channelName;
		addChannel();
	}

	~CopyDataChannel()
	{
		Dispose();
	}
}

	/// <summary>
	/// A class which wraps the data being copied, used
	/// internally within the CopyData class objects.
	/// </summary>
[Serializable()]
internal class CopyDataObjectData
{
	/// <summary>
	/// The Object to copy.  Must be Serializable.
	/// </summary>
	public object Data;
	/// <summary>
	/// The date and time this object was sent.
	/// </summary>
	public DateTime Sent;
	/// <summary>
	/// The name of the channel this object is being sent on
	/// </summary>
	public string Channel;

	/// <summary>
	/// Constructs a new instance of this object
	/// </summary>
	/// <param name="data">The data to copy</param>
	/// <param name="channel">The channel name to send on</param>
	/// <exception cref="ArgumentException">If data is not serializable.</exception>
	public CopyDataObjectData(object data, string channel)
	{
		Data = data;
		if (!data.GetType().IsSerializable)
		{
			throw new ArgumentException("Data object must be serializable.",
				"data");
		}
		Channel = channel;
		Sent = DateTime.Now;
	}
}

	
	/// <summary>
	/// Window Style Flags
	/// </summary>
[Flags]
public enum WindowStyleFlags : uint
{
	WS_OVERLAPPED      = 0x00000000,
	WS_POPUP           = 0x80000000,
	WS_CHILD           = 0x40000000,
	WS_MINIMIZE        = 0x20000000,
	WS_VISIBLE         = 0x10000000,
	WS_DISABLED        = 0x08000000,
	WS_CLIPSIBLINGS    = 0x04000000,
	WS_CLIPCHILDREN    = 0x02000000,
	WS_MAXIMIZE        = 0x01000000,
	WS_BORDER          = 0x00800000,
	WS_DLGFRAME        = 0x00400000,
	WS_VSCROLL         = 0x00200000,
	WS_HSCROLL         = 0x00100000,
	WS_SYSMENU         = 0x00080000,
	WS_THICKFRAME      = 0x00040000,
	WS_GROUP           = 0x00020000,
	WS_TABSTOP         = 0x00010000,
	WS_MINIMIZEBOX     = 0x00020000,
	WS_MAXIMIZEBOX     = 0x00010000,
}
   
	/// <summary>
	/// Extended Windows Style flags
	/// </summary>
[Flags]
public enum ExtendedWindowStyleFlags : int
{
	WS_EX_DLGMODALFRAME    = 0x00000001,
	WS_EX_NOPARENTNOTIFY   = 0x00000004,
	WS_EX_TOPMOST          = 0x00000008,
	WS_EX_ACCEPTFILES      = 0x00000010,
	WS_EX_TRANSPARENT      = 0x00000020,

	WS_EX_MDICHILD         = 0x00000040,
	WS_EX_TOOLWINDOW       = 0x00000080,
	WS_EX_WINDOWEDGE       = 0x00000100,
	WS_EX_CLIENTEDGE       = 0x00000200,
	WS_EX_CONTEXTHELP      = 0x00000400,

	WS_EX_RIGHT            = 0x00001000,
	WS_EX_LEFT             = 0x00000000,
	WS_EX_RTLREADING       = 0x00002000,
	WS_EX_LTRREADING       = 0x00000000,
	WS_EX_LEFTSCROLLBAR    = 0x00004000,
	WS_EX_RIGHTSCROLLBAR   = 0x00000000,

	WS_EX_CONTROLPARENT    = 0x00010000,
	WS_EX_STATICEDGE       = 0x00020000,
	WS_EX_APPWINDOW        = 0x00040000,

	WS_EX_LAYERED          = 0x00080000,

	WS_EX_NOINHERITLAYOUT  = 0x00100000, // Disable inheritence of mirroring by children
	WS_EX_LAYOUTRTL        = 0x00400000, // Right to left mirroring

	WS_EX_COMPOSITED       = 0x02000000,
	WS_EX_NOACTIVATE       = 0x08000000
}


#region EnumWindows
	/// <summary>
	/// EnumWindows wrapper for .NET
	/// </summary>
public class EnumWindows
{
	#region Delegates
	private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);
	#endregion

	#region UnManagedMethods
	private class UnManagedMethods
	{
		[DllImport("user32")]
		public extern static int EnumWindows (
			EnumWindowsProc lpEnumFunc, 
			int lParam);
		[DllImport("user32")]
		public extern static int EnumChildWindows (
			IntPtr hWndParent,
			EnumWindowsProc lpEnumFunc, 
			int lParam);
	}
	#endregion

	#region Member Variables
	private EnumWindowsCollection items = null;
	#endregion

	/// <summary>
	/// Returns the collection of windows returned by
	/// GetWindows
	/// </summary>
	public EnumWindowsCollection Items
	{
		get
		{
			return this.items;
		}
	}

	/// <summary>
	/// Gets all top level windows on the system.
	/// </summary>
	public void GetWindows()
	{
		this.items = new EnumWindowsCollection();
		UnManagedMethods.EnumWindows(
			new EnumWindowsProc(this.WindowEnum),
			0);
	}
	/// <summary>
	/// Gets all child windows of the specified window
	/// </summary>
	/// <param name="hWndParent">Window Handle to get children for</param>
	public void GetWindows(
		IntPtr hWndParent)
	{
		this.items = new EnumWindowsCollection();
		UnManagedMethods.EnumChildWindows(
			hWndParent,
			new EnumWindowsProc(this.WindowEnum),
			0);
	}

	#region EnumWindows callback
	/// <summary>
	/// The enum Windows callback.
	/// </summary>
	/// <param name="hWnd">Window Handle</param>
	/// <param name="lParam">Application defined value</param>
	/// <returns>1 to continue enumeration, 0 to stop</returns>
	private int WindowEnum(
		IntPtr hWnd,
		int lParam)
	{
		if (this.OnWindowEnum(hWnd))
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}
	#endregion

	/// <summary>
	/// Called whenever a new window is about to be added
	/// by the Window enumeration called from GetWindows.
	/// If overriding this function, return true to continue
	/// enumeration or false to stop.  If you do not call
	/// the base implementation the Items collection will
	/// be empty.
	/// </summary>
	/// <param name="hWnd">Window handle to add</param>
	/// <returns>True to continue enumeration, False to stop</returns>
	protected virtual bool OnWindowEnum(
		IntPtr hWnd)
	{
		items.Add(hWnd);
		return true;
	}

	#region Constructor, Dispose
	public EnumWindows()
	{
		// nothing to do
	}
	#endregion
}   
#endregion EnumWindows

#region EnumWindowsCollection
	/// <summary>
	/// Holds a collection of Windows returned by GetWindows.
	/// </summary>
public class EnumWindowsCollection : ReadOnlyCollectionBase
{
	/// <summary>
	/// Add a new Window to the collection.  Intended for
	/// internal use by EnumWindows only.
	/// </summary>
	/// <param name="hWnd">Window handle to add</param>
	public void Add(IntPtr hWnd)
	{
		EnumWindowsItem item = new EnumWindowsItem(hWnd);
		this.InnerList.Add(item);
	}

	/// <summary>
	/// Gets the Window at the specified index
	/// </summary>
	public EnumWindowsItem this[int index]
	{
		get
		{
			return (EnumWindowsItem)this.InnerList[index];
		}
	}

	/// <summary>
	/// Constructs a new EnumWindowsCollection object.
	/// </summary>
	public EnumWindowsCollection()
	{
		// nothing to do
	}
}
#endregion      

#region EnumWindowsItem
	/// <summary>
	/// Provides details about a Window returned by the 
	/// enumeration
	/// </summary>
public class EnumWindowsItem
{
	#region Structures
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private struct FLASHWINFO
	{
		public int cbSize;
		public IntPtr hwnd;
		public int dwFlags;
		public int uCount;
		public int dwTimeout;
	}
	#endregion

	#region UnManagedMethods
	private class UnManagedMethods
	{
		[DllImport("user32")]
		public extern static int IsWindowVisible (
			IntPtr hWnd);
		[DllImport("user32", CharSet = CharSet.Auto)]
		public extern static int GetWindowText(
			IntPtr hWnd, 
			StringBuilder lpString, 
			int cch);
		[DllImport("user32", CharSet = CharSet.Auto)]
		public extern static int GetWindowTextLength(
			IntPtr hWnd);
		[DllImport("user32")]
		public extern static int BringWindowToTop (IntPtr hWnd);
		[DllImport("user32")]
		public extern static int SetForegroundWindow (IntPtr hWnd);
		[DllImport("user32")]
		public extern static int IsIconic(IntPtr hWnd);
		[DllImport("user32")]
		public extern static int IsZoomed(IntPtr hwnd);
		[DllImport("user32", CharSet = CharSet.Auto)]
		public extern static int GetClassName (
			IntPtr hWnd, 
			StringBuilder lpClassName, 
			int nMaxCount);
		[DllImport("user32")]
		public extern static int FlashWindow (
			IntPtr hWnd,
			ref FLASHWINFO pwfi);
		[DllImport("user32")]
		public extern static int GetWindowRect (
			IntPtr hWnd, 
			ref RECT lpRect);
		[DllImport("user32", CharSet = CharSet.Auto)]
		public extern static int SendMessage(
			IntPtr hWnd, 
			int wMsg, 
			IntPtr wParam, 
			IntPtr lParam);
		[DllImport("user32", CharSet = CharSet.Auto)]
		public extern static uint GetWindowLong (
			IntPtr hwnd, 
			int nIndex);
		public const int WM_COMMAND = 0x111;
		public const int WM_SYSCOMMAND = 0x112;
            
		public const int SC_RESTORE = 0xF120;
		public const int SC_CLOSE = 0xF060;
		public const int SC_MAXIMIZE = 0xF030;
		public const int SC_MINIMIZE = 0xF020;

		public const int GWL_STYLE = (-16);
		public const int GWL_EXSTYLE = (-20);

		/// <summary>
		/// Stop flashing. The system restores the window to its original state.
		/// </summary>
		public const int FLASHW_STOP = 0;
		/// <summary>
		/// Flash the window caption. 
		/// </summary>
		public const int FLASHW_CAPTION = 0x00000001;
		/// <summary>
		/// Flash the taskbar button.
		/// </summary>
		public const int FLASHW_TRAY = 0x00000002;
		/// <summary>
		/// Flash both the window caption and taskbar button.
		/// </summary>
		public const int FLASHW_ALL = (FLASHW_CAPTION | FLASHW_TRAY);
		/// <summary>
		/// Flash continuously, until the FLASHW_STOP flag is set.
		/// </summary>
		public const int FLASHW_TIMER = 0x00000004;
		/// <summary>
		/// Flash continuously until the window comes to the foreground. 
		/// </summary>
		public const int FLASHW_TIMERNOFG = 0x0000000C;
	}
	#endregion

	/// <summary>
	/// The window handle.
	/// </summary>
	private IntPtr hWnd = IntPtr.Zero;

	/// <summary>
	/// To allow items to be compared, the hash code
	/// is set to the Window handle, so two EnumWindowsItem
	/// objects for the same Window will be equal.
	/// </summary>
	/// <returns>The Window Handle for this window</returns>
	public override System.Int32 GetHashCode()
	{
		return (System.Int32)this.hWnd;
	}

	/// <summary>
	/// Gets the window's handle
	/// </summary>
	public IntPtr Handle
	{
		get
		{
			return this.hWnd;
		}
	}

	/// <summary>
	/// Gets the window's title (caption)
	/// </summary>
	public string Text
	{
		get
		{
			StringBuilder title = new StringBuilder(260, 260);
			UnManagedMethods.GetWindowText(this.hWnd, title, title.Capacity);
			return title.ToString();
		}
	}

	/// <summary>
	/// Gets the window's class name.
	/// </summary>
	public string ClassName
	{
		get
		{
			StringBuilder className = new StringBuilder(260, 260);
			UnManagedMethods.GetClassName(this.hWnd, className,
				className.Capacity);
			return className.ToString();
		}
	}

	/// <summary>
	/// Gets/Sets whether the window is iconic (mimimised) or not.
	/// </summary>
	public bool Iconic
	{
		get
		{
			return ((UnManagedMethods.IsIconic(this.hWnd) == 0) ? false : true);
		}
		set
		{
			UnManagedMethods.SendMessage(
				this.hWnd, 
				UnManagedMethods.WM_SYSCOMMAND, 
				(IntPtr)UnManagedMethods.SC_MINIMIZE,
				IntPtr.Zero);
		}
	}
         
	/// <summary>
	/// Gets/Sets whether the window is maximised or not.
	/// </summary>
	public bool Maximised
	{
		get
		{
			return ((UnManagedMethods.IsZoomed(this.hWnd) == 0) ? false : true);
		}
		set
		{
			UnManagedMethods.SendMessage(
				this.hWnd,
				UnManagedMethods.WM_SYSCOMMAND, 
				(IntPtr)UnManagedMethods.SC_MAXIMIZE,
				IntPtr.Zero);
		}
	}

	/// <summary>
	/// Gets whether the window is visible.
	/// </summary>
	public bool Visible
	{
		get
		{
			return ((UnManagedMethods.IsWindowVisible(this.hWnd) == 0) ? false
				: true);
		}
	}

	/// <summary>
	/// Gets the bounding rectangle of the window
	/// </summary>
	public System.Drawing.Rectangle Rect
	{
		get
		{
			RECT rc = new RECT();
			UnManagedMethods.GetWindowRect(
				this.hWnd,
				ref rc);
			System.Drawing.Rectangle rcRet = new System.Drawing.Rectangle(
				rc.Left, rc.Top,
				rc.Right - rc.Left, rc.Bottom - rc.Top);
			return rcRet;
		}
	}

	/// <summary>
	/// Gets the location of the window relative to the screen.
	/// </summary>
	public System.Drawing.Point Location
	{
		get
		{
			System.Drawing.Rectangle rc = Rect;
			System.Drawing.Point pt = new System.Drawing.Point(
				rc.Left,
				rc.Top);
			return pt;
		}
	}
         
	/// <summary>
	/// Gets the size of the window.
	/// </summary>
	public System.Drawing.Size Size
	{
		get
		{
			System.Drawing.Rectangle rc = Rect;
			System.Drawing.Size sz = new System.Drawing.Size(
				rc.Right - rc.Left,
				rc.Bottom - rc.Top);
			return sz;
		}
	}

	/// <summary>
	/// Restores and Brings the window to the front, 
	/// assuming it is a visible application window.
	/// </summary>
	public void Restore()
	{
		if (Iconic)
		{
			UnManagedMethods.SendMessage(
				this.hWnd, 
				UnManagedMethods.WM_SYSCOMMAND, 
				(IntPtr)UnManagedMethods.SC_RESTORE, 
				IntPtr.Zero);
		}
		UnManagedMethods.BringWindowToTop(this.hWnd);
		UnManagedMethods.SetForegroundWindow(this.hWnd);
	}

	public WindowStyleFlags WindowStyle
	{
		get
		{
			return (WindowStyleFlags)UnManagedMethods.GetWindowLong(
				this.hWnd, UnManagedMethods.GWL_STYLE);
		}
	}
      
	public ExtendedWindowStyleFlags ExtendedWindowStyle
	{
		get
		{
			return (ExtendedWindowStyleFlags)UnManagedMethods.GetWindowLong(
				this.hWnd, UnManagedMethods.GWL_EXSTYLE);
		}
	}

	/// <summary>
	///  Constructs a new instance of this class for
	///  the specified Window Handle.
	/// </summary>
	/// <param name="hWnd">The Window Handle</param>
	public EnumWindowsItem(IntPtr hWnd)
	{
		this.hWnd = hWnd;
	}
}
#endregion