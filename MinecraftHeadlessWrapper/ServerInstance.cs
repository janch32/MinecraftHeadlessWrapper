using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace MinecraftHeadlessWrapper
{
	public class ServerInstance
	{
		private Process Process = new Process();
		private List<RemoteManage.Client> SubscribedClients = new List<RemoteManage.Client>();
		private Timer StopTimer;
		private bool PendingRestart = false;
		private bool PendingStop = false;
		private bool Starting = false;

		public List<string> Console = new List<string>();
		public string Name = "";
		public string Path;
		public string Executable;
		public string Args = "";
		public uint Xms;
		public uint Xmx;
		public bool AutoStart = true;
		public bool RestartOnFailture = false;

		private bool _isStarted = false;
		public bool IsRunning { get => _isStarted && !Process.HasExited; }


		public ServerInstance(string name)
		{
			Name = name;
			StopTimer = new Timer(30 * 1000);
			StopTimer.Elapsed += ProcessExited;
		}

		private void OutputReceived(object sender, DataReceivedEventArgs e)
		{
			if (Starting && e.Data != null && e.Data.ToLower().Contains("done"))
			{
				Starting = false;
				SendInfoToEveryone();
			}


			System.Console.WriteLine(e.Data);
			Console.Add(e.Data);
			if (Console.Count > Program.Config.ConsoleBufferSize)
				Console.RemoveRange(0, Console.Count - (int)Program.Config.ConsoleBufferSize);

			SendNewLineToSubscribers(e.Data);
		}

		public void SendCommand(string cmd)
		{
			if (IsRunning)
				Process.StandardInput.WriteLine(cmd);
		}

		public void Start()
		{
			if (IsRunning) return;

			PendingRestart = false;
			Console.Clear();
			Process.Dispose();
			Process = new Process();

			ProcessStartInfo startInfo = Process.StartInfo;
			startInfo.Arguments = "";
			if (Xms != 0) startInfo.Arguments += "-Xms" + Xms + "M ";
			if (Xmx != 0) startInfo.Arguments += "-Xmx" + Xmx + "M ";

			startInfo.WorkingDirectory = Path;
			startInfo.FileName = Program.Config.JavaPath;
			startInfo.Arguments += Args + " -Dfile.encoding=UTF-8 -jar " + Executable + " nogui";
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.StandardOutputEncoding = Encoding.UTF8;

			Process.OutputDataReceived += OutputReceived;
			Process.Exited += ProcessExited;
			Process.EnableRaisingEvents = true;
			Process.Start();
			Process.BeginOutputReadLine();
			_isStarted = true;
			Starting = true;
			SendInfoToEveryone();
		}


		public void Stop()
		{
			if (IsRunning && !PendingStop)
			{
				PendingStop = true;
				SendCommand("stop");
				StopTimer.Start();
			}
			SendInfoToEveryone();
		}

		public void Restart()
		{
			PendingRestart = true;
			Stop();
		}

		private void ProcessExited(object sender, EventArgs e)
		{
			PendingStop = false;
			Starting = false;
			StopTimer.Stop();
			if (IsRunning)
				Process.Kill();

			SendInfoToEveryone();

			if (PendingRestart)
				Start();
		}

		public void WaitForExit()
		{
			if (IsRunning)
				Process.WaitForExit();
		}

		private int GetMemoryUsage()
		{
			int mem = 0;

			if (IsRunning)
			{
				mem = (int)(ProcessPerfMon.GetMemoryUsage(Process) / (1024 * 1024));
			}

			return mem;
		}

		public ServerInstanceInfo GetInfo()
		{
			var info = new ServerInstanceInfo
			{
				StartTime = IsRunning ? Process.StartTime : new DateTime(),
				MemoryUsage = GetMemoryUsage(),
				MaxMemoryUsage = (int)Xmx,
				Status = ServerInstanceStatus.Inactive,
				Name = Name
			};

			if (PendingRestart)
				info.Status = ServerInstanceStatus.PendingRestart;
			else if (PendingStop)
				info.Status = ServerInstanceStatus.Stopping;
			else if (IsRunning && Starting)
				info.Status = ServerInstanceStatus.Starting;
			else if (IsRunning)
				info.Status = ServerInstanceStatus.Running;
			else if (_isStarted && Process.ExitCode != 0)
				info.Status = ServerInstanceStatus.InactiveFail;

			return info;
		}

		private void SendInfoToEveryone()
		{
			if (Program.Clients.Count <= 0) return;

			var message = new RemoteManage.MessageServerInfo()
			{
				Type = RemoteManage.MessageType.ServerInfo,
				Info = GetInfo()
			};

			Program.Clients.ForEach(new Action<RemoteManage.Client>(client =>
			{
				client.SendMessage(message);
			}));
		}

		private void SendNewLineToSubscribers(string newLine)
		{
			if (SubscribedClients.Count <= 0) return;

			var message = new RemoteManage.MessageServerNewLine()
			{
				Type = RemoteManage.MessageType.ServerNewLine,
				NewLine = newLine
			};

			SubscribedClients.ForEach(new Action<RemoteManage.Client>(client =>
			{
				client.SendMessage(message);
			}));
		}

		public void SubscribeClient(RemoteManage.Client client)
		{
			if (!SubscribedClients.Contains(client))
			{
				client.SendMessage(new RemoteManage.MessageServerConsoleLines()
				{
					Type = RemoteManage.MessageType.ServerConsoleLines,
					Lines = Console.ToArray()
				});

				SubscribedClients.Add(client);
			}
		}

		public void UnsubscribeClient(RemoteManage.Client client)
		{
			SubscribedClients.Remove(client);
		}
	}
}
