using System.Xml.Serialization;

namespace MinecraftHeadlessWrapper.Config
{
	public class Server
	{
		[XmlAttribute("name")]
		public string Name = "";

		[XmlAttribute("minMemAlloc")]
		public uint MinMemAlloc = 0;

		[XmlAttribute("maxMemAlloc")]
		public uint MaxMemAlloc = 0;

		[XmlElement("path")]
		public string Path = "./";

		[XmlElement("args")]
		public string Args = "";

		[XmlElement("executable")]
		public string Executable = "minecraft_server.jar";

		[XmlElement("autoStart")]
		public bool AutoStart = true;

		[XmlElement("restartOnFailure")]
		public bool RestartOnFailure = false;
	}
}
