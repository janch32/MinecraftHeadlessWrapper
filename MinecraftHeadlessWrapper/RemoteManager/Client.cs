using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace MinecraftHeadlessWrapper.RemoteManage
{
	public class Client : WebSocketBehavior
	{
		private bool Authentificated = false;
		private ServerInstance SubscribedServer = null;

		protected override void OnOpen()
		{
			SendMessage(new Message() { Type = MessageType.AuthRequired });
		}

		protected override void OnClose(CloseEventArgs e)
		{
			if (Authentificated)
			{
				Program.Clients.Remove(this);
			}

			if (SubscribedServer != null)
			{
				SubscribedServer.UnsubscribeClient(this);
			}
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			try
			{
				if (!e.IsText) return;
				var msg = JsonConvert.DeserializeObject<Message>(e.Data);

				if (!Authentificated)
				{
					if (msg.Type == MessageType.AuthRequest)
					{
						var authData = JsonConvert.DeserializeObject<MessageAuth>(e.Data);
						if (authData.Password == Program.Config.RemoteManager.Password &&
							authData.Username == Program.Config.RemoteManager.Username)
						{
							Authentificated = true;
							Program.Clients.Add(this);
							SendMessage(new Message() { Type = MessageType.AuthCorrect });
							SendServerList();
						}
						else
							SendMessage(new Message() { Type = MessageType.AuthIncorrect });
					}
					else
						SendMessage(new Message() { Type = MessageType.AuthRequired });
					return;
				}


				string serverName;
				ServerInstance server;

				switch (msg.Type)
				{
					case MessageType.UnsubscribeServer:
						if (SubscribedServer != null)
							SubscribedServer.UnsubscribeClient(this);
						SubscribedServer = null;
						break;
					case MessageType.SubscribeServer:
						serverName = JsonConvert.DeserializeObject<MessageServerName>(e.Data).Name;
						server = Program.Server.Find(s => s.Name == serverName);
						if (server != null)
						{
							if(SubscribedServer != null)
								SubscribedServer.UnsubscribeClient(this);

							SubscribedServer = server;
							server.SubscribeClient(this);
						}
						break;
					case MessageType.StartServer:
						serverName = JsonConvert.DeserializeObject<MessageServerName>(e.Data).Name;
						server = Program.Server.Find(s => s.Name == serverName);
						if (server != null)
							server.Start();
						break;
					case MessageType.StopServer:
						serverName = JsonConvert.DeserializeObject<MessageServerName>(e.Data).Name;
						server = Program.Server.Find(s => s.Name == serverName);
						if (server != null)
							server.Stop();
						break;
					case MessageType.RestartServer:
						serverName = JsonConvert.DeserializeObject<MessageServerName>(e.Data).Name;
						server = Program.Server.Find(s => s.Name == serverName);
						if (server != null)
							server.Restart();
						break;
					case MessageType.ServerCommand:
						if(SubscribedServer != null)
						{
							string command = JsonConvert.DeserializeObject<MessageServerCommand>(e.Data).Command;
							SubscribedServer.SendCommand(command);
						}
						break;
					case MessageType.RequestServerList:
						SendServerList();
						break;
					default:
						break;
				}
			}
			catch (JsonException jsonEx)
			{
				Console.WriteLine("Message json parse error: " + jsonEx.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Message error: " + ex.ToString());
			}
		}

		public void SendMessage(Message message)
		{
			if (!Authentificated && 
				message.Type != MessageType.AuthCorrect &&
				message.Type != MessageType.AuthIncorrect)
			{
				message = new Message() { Type = MessageType.AuthRequired };
			}

			Send(JsonConvert.SerializeObject(message));
		}

		public void SendServerList()
		{
			if (!Authentificated) return;

			List<ServerInstanceInfo> instanceInfo = new List<ServerInstanceInfo>();

			Program.Server.ForEach(new Action<ServerInstance>(server =>
			{
				instanceInfo.Add(server.GetInfo());
			}));

			SendMessage(new MessageServerList()
			{
				Type = MessageType.ServerList,
				ServersInfo = instanceInfo.ToArray()
			});
		}
	}
}
