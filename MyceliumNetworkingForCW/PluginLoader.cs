using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MyceliumNetworking
{
	[ContentWarningPlugin("RugbugRedfern.MyceliumNetworking", VERSION, false)]
	public class PluginLoader
    {
		const string VERSION = "1.0.14";

		static bool initialized;

		static PluginLoader()
		{
			if(initialized)
				return;

			initialized = true;

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

			RugLogger.Log("MyceliumNetworking Starting " + VERSION);

			MyceliumNetwork.Initialize();

			// Initialize mod on persistent GameObject
			var go = new GameObject("MyceliumNetworking Persistent");
			go.AddComponent<PersistentGameObject>();
			go.hideFlags = HideFlags.HideAndDontSave;
			GameObject.DontDestroyOnLoad(go);
		}
	}
}
