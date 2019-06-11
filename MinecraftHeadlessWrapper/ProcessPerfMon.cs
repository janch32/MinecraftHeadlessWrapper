using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftHeadlessWrapper
{
	class ProcessPerfMon
	{
		private static PerformanceCounter _PerfMem = new PerformanceCounter("Process", "Working Set - Private", "");
		public static float GetMemoryUsage(Process process)
		{
			string name;

			try
			{
				name = GetProcessInstanceName(process.Id);
			}
			catch (Exception)
			{
				return 0;
			}

			_PerfMem.InstanceName = name;
			return _PerfMem.NextValue();
		}

		private static PerformanceCounter _PerfId = new PerformanceCounter("Process", "ID Process", "");
		private static string GetProcessInstanceName(int pid)
		{
			var cat = new PerformanceCounterCategory("Process");

			string[] instances = cat.GetInstanceNames();
			foreach (string instance in instances)
			{
				_PerfId.InstanceName = instance;
				int val = (int)_PerfId.RawValue;
				if (val == pid)
					return instance;
			}

			throw new Exception("Could not find performance counter instance name for current process.");
		}
	}
}
