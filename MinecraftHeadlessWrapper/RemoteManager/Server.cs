using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace MinecraftHeadlessWrapper.RemoteManage
{
	class Server
	{
		private WebSocketServer WsServer;

		public void Start(IPAddress ip, ushort port)
		{
			WsServer = new WebSocketServer(ip, port);
			WsServer.AddWebSocketService<Client>("/remote-manager");
			WsServer.Start();
		}

		public void Stop()
		{
			WsServer.Stop();
		}
	}
}
