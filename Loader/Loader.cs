//------------------------------------------------------------------------------
//  <copyright from='2004' to='2005' company='Jerremy Koot and William Archbell'>
//    Copyright (c) Jerremy Koot and William Archbell. All Rights Reserved.
//  </copyright>
//-------------------------------------------------------------------------------
//
// File:    Loader.cs
//
// Purpose: This application connects to NNTP servers and downloads and combines
//          messages that are listed in the NZB files that are supplied by the user.

using System;
using System.Reflection;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Loader
{
	public class Start
	{
		public static Mutex m_RunMutex;

		public static bool m_Update = false;
		public static bool m_Restart = false;
		
		public static Version m_Version;
		public static string m_Name;

		private static AppDomain m_AppDomain;
		private static RemoteLoader m_RemoteLoader;

		[STAThread]
		public static void Main(string[] args)
		{
			if(Environment.CurrentDirectory != Application.StartupPath)
				Environment.CurrentDirectory = Application.StartupPath;

			Console.WriteLine("Creating loader.");

			m_RunMutex = new Mutex(true, "NZB-O-Matic Mutex");
			if(!m_RunMutex.WaitOne(0, false))
			{
				Console.WriteLine("Instance of NZB-O-Matic already running!");
				CopyData cd = new CopyData();
				cd.Channels.Add("NZBImport");
				foreach(string str in args)
					if(str.EndsWith(".nzb"))
						cd.Channels["NZBImport"].Send(str);
				cd.Channels.Remove("NZBImport");						
				return;
			}

			do
			{
				try
				{
					//reset variables
					m_Update = false;
					m_Restart = false;

					//setup appdomain
					AppDomainSetup setup = new AppDomainSetup();
					setup.ApplicationName = "NZB-O-MaticPlus";
					setup.ApplicationBase = Environment.CurrentDirectory;
					setup.ShadowCopyDirectories = Environment.CurrentDirectory;
					setup.ShadowCopyFiles = "true";

					//create appdomain
					m_AppDomain = AppDomain.CreateDomain("Engine Domain", null, setup);
					//create remoteloader in appdomain
					m_RemoteLoader = (RemoteLoader)m_AppDomain.CreateInstanceFromAndUnwrap(System.IO.Path.GetFileName(Application.ExecutablePath), "Loader.RemoteLoader");
					//load assembly in to remoteloader
					m_RemoteLoader.LoadAssembly("Engine");
					//create instance of engine in remoteloader
					m_RemoteLoader.Create();
					//get name and version from engine
					m_Name = m_RemoteLoader.Name;
					m_Version = m_RemoteLoader.Version;
					//start engine in remoteloader
					m_RemoteLoader.Start(args);
					//get variables after engine exits
					m_Restart = m_RemoteLoader.Restart;
					m_Update = m_RemoteLoader.Update;
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
				}

				if(m_Update)
				{
					frmUpdate m_UpdateForm = new frmUpdate();
					Application.Run(m_UpdateForm);
					m_UpdateForm.Dispose();
				}	
			} while(m_Restart);

			Console.WriteLine("Exiting program.");
		}
	}

	[Serializable]
	public class RemoteLoader: MarshalByRefObject, IDisposable
	{
		public Assembly m_Assembly;
		public IEngine m_Engine;

		private bool m_Restart;
		private bool m_Update;
		private string m_Name;
		private Version m_Version;

		public bool Update
		{
			get
			{
				return m_Update;
			}
		}

		public bool Restart
		{
			get
			{
				return m_Restart;
			}
		}

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public Version Version
		{
			get
			{
				return m_Version;
			}
		}

		public bool LoadAssembly(string fullname)
		{
			string path = Path.GetDirectoryName(fullname);
			string filename = Path.GetFileNameWithoutExtension(fullname);

			try
			{
				Console.WriteLine("Loading assembly.");
				m_Assembly = Assembly.Load(filename);
				System.Reflection.AssemblyName m_AssemblyName = m_Assembly.GetName();
				m_Name = m_AssemblyName.Name;		
				m_Version = m_AssemblyName.Version;
				
				return true;

			}
			catch(Exception e)
			{
				Console.WriteLine(e);
				return false;
			}
		}

		public bool Create()
		{
			try
			{
				m_Engine = (IEngine)m_Assembly.CreateInstance("Engine");
				return true;
			}
			catch
			{
				return false;
			}
		}

		public void Start(string[] args)
		{
			if(m_Engine == null)
				return;

			m_Engine.Start(args);
			m_Restart = m_Engine.Restart;
			m_Update = m_Engine.Update;
		}

		public void Stop()
		{
			if(m_Engine != null)
				m_Engine.Stop();
			m_Engine = null;
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
