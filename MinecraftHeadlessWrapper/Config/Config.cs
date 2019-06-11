using System.Xml.Serialization;

namespace MinecraftHeadlessWrapper.Config
{
	[XmlRoot("config")]
	public class Config
	{
		[XmlAttribute("version")]
		public int Version = 1;

		[XmlAttribute("javaPath")]
		public string JavaPath = "java.exe";

		[XmlAttribute("consoleBufferSize")]
		public uint ConsoleBufferSize = 40;

		[XmlArray("serverList")]
		[XmlArrayItem("server")]
		public Server[] Server;

		[XmlElement("remoteManager")]
		public RemoteManager RemoteManager;

		public bool IsRemoteManagerEnabled { get { return RemoteManager != null; } }
	}
}
