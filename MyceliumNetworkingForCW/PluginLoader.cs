using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MyceliumNetworking
{
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class PluginLoader : BaseUnityPlugin
    {
		static bool initialized;

		void Awake()
		{
			if(initialized)
				return;

			initialized = true;

			RugLogger.Initialize(MyPluginInfo.PLUGIN_GUID);

			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

			RugLogger.Log("MyceliumNetworking Starting " + MyPluginInfo.PLUGIN_VERSION);

			MyceliumNetwork.Initialize();

			// Initialize mod on persistent GameObject
			var go = new GameObject("MyceliumNetworking Persistent");
			go.AddComponent<PersistentGameObject>();
			go.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(go);
		}
	}
}
