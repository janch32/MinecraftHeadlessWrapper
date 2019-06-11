using System.Xml.Serialization;

namespace MinecraftHeadlessWrapper.Config
{
	public class RemoteManager
	{
		[XmlAttribute("address")]
		public string Address = "0.0.0.0";

		[XmlAttribute("port")]
		public ushort Port = 25500;

		[XmlElement("username")]
		public string Username = "";

		[XmlElement("password")]
		public string Password = "";
	}
}
