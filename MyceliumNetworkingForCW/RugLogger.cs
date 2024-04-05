using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyceliumNetworking
{
	public static class RugLogger
	{
		public static ManualLogSource logSource;

		public static void Initialize(string modGUID)
		{
			logSource = Logger.CreateLogSource(modGUID);
		}

		public static void Log(object message)
		{
			logSource.LogInfo(message);
		}

		public static void LogError(object message)
		{
			logSource.LogError(message);
		}

		public static void LogWarning(object message)
		{
			logSource.LogWarning(message);
		}
	}
}
