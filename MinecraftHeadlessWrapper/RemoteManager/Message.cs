using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MinecraftHeadlessWrapper.RemoteManage
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum MessageType
	{
		AuthRequired,
		AuthRequest,
		AuthIncorrect,
		AuthCorrect,

		RequestServerList,
		ServerList,
		ServerInfo,
		ServerNewLine,
		ServerConsoleLines,
		ServerCommand,

		//UpdateConfig, // TODO
		SubscribeServer,
		UnsubscribeServer,
		StartServer,
		RestartServer,
		StopServer,
	}

	public class Message
	{
		public MessageType Type;
	}

	public class MessageServerName : Message
	{
		public string Name;
	}

	public class MessageServerList : Message
	{
		public ServerInstanceInfo[] ServersInfo;
	}

	public class MessageServerInfo : Message
	{
		public ServerInstanceInfo Info;
	}

	public class MessageServerCommand : Message
	{
		public string Command;
	}

	public class MessageServerNewLine : Message
	{
		public string NewLine;
	}

	public class MessageServerConsoleLines : Message
	{
		public string[] Lines;
	}

	public class MessageAuth : Message
	{
		public string Username;
		public string Password;
	}
}
