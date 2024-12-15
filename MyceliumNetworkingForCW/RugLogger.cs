using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MyceliumNetworking
{
	public static class RugLogger
	{
		const string MOD_GUID = "RugbugRedfern.MyceliumNetworking";

		public static void Log(object message)
		{
			Debug.Log(MOD_GUID + ": " + message);
		}

		public static void LogError(object message)
		{
			Debug.LogError(MOD_GUID + ": " + message);
		}

		public static void LogWarning(object message)
		{
			Debug.LogWarning(MOD_GUID + ": " + message);
		}
	}
}
