using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MinecraftHeadlessWrapper
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ServerInstanceStatus
	{
		Starting,
		Running,
		Stopping,
		Inactive,
		InactiveFail,
		PendingRestart
	}

	public class ServerInstanceInfo
	{
		public string Name;
		public ServerInstanceStatus Status;
		public DateTime StartTime;
		public int MemoryUsage;
		public int MaxMemoryUsage;
	}
}
