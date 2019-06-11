using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;

namespace MinecraftHeadlessWrapper
{
	class Program
	{
		static bool exitSystem = false;
		public static List<ServerInstance> Server = new List<ServerInstance>();
		public static Config.Config Config;
		public static List<RemoteManage.Client> Clients = new List<RemoteManage.Client>();
		public static RemoteManage.Server RemoteServer = new RemoteManage.Server();

		static void Main(string[] args)
		{
			_handler += new EventHandler(Handler);
			SetConsoleCtrlHandler(_handler, true);

			Config = XmlSerializer.Deserialize<Config.Config>(File.ReadAllText("./wrapper.config"));

			foreach (Config.Server s in Config.Server)
			{
				if (string.IsNullOrEmpty(s.Name))
				{
					Console.WriteLine("Failed to parse config - server name attribute is required");
					Environment.Exit(1);
				}
				else
				{
					ServerInstance instance = Server.Find(server => server.Name == s.Name);
					if(instance == null)
					{
						instance = new ServerInstance(s.Name);
						Server.Add(instance);
					}

					instance.Path = s.Path;
					instance.Executable = s.Executable;
					instance.Xms = s.MinMemAlloc;
					instance.Xmx = s.MaxMemAlloc;
					instance.Args = s.Args;
					instance.AutoStart = s.AutoStart;
					instance.RestartOnFailture = s.RestartOnFailure;
				}
			}

			Server.ForEach(new Action<ServerInstance>((s) => 
			{
				if (s.AutoStart) s.Start();
			}));

			
			if(Config.RemoteManager == null)
				Console.WriteLine("Config: remoteManager element not found in config - Remote Manager disabled");
			else if(string.IsNullOrEmpty(Config.RemoteManager.Password) || string.IsNullOrEmpty(Config.RemoteManager.Password))
				Console.WriteLine("Config: Remote Manager username and/or passwrod is empty - Remote Manager disabled");
			else
				RemoteServer.Start(IPAddress.Parse(Config.RemoteManager.Address), Config.RemoteManager.Port);

			while (!exitSystem)
				Thread.Sleep(500);
		}

		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

		private delegate bool EventHandler(CtrlType sig);
		static EventHandler _handler;

		enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		private static bool Handler(CtrlType sig)
		{
			Console.WriteLine("Exiting application");

			RemoteServer.Stop();

			Server.ForEach(new Action<ServerInstance>(s =>
			{
				s.Stop();
			}));

			Server.ForEach(new Action<ServerInstance>(s =>
			{
				s.WaitForExit();
			}));

			exitSystem = true;
			Environment.Exit(0);
			return true;
		}
	}
}
