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
	[BepInPlugin(modGUID, modName, modVersion)]
    public class PluginLoader : BaseUnityPlugin
    {
		const string modGUID = "RugbugRedfern.MyceliumNetworking";
		const string modName = "Mycelium Networking";
		const string modVersion = "1.0.0";
		static bool initialized;

		readonly Harmony harmony = new Harmony(modGUID);

		void Awake()
		{
			if(initialized)
				return;

			initialized = true;

			RugLogger.Initialize(modGUID);

			harmony.PatchAll(Assembly.GetExecutingAssembly());

			RugLogger.Log("MyceliumNetworking Starting " + modVersion);

			MyceliumNetwork.Initialize();

			// Initialize mod on persistent GameObject
			var go = new GameObject("MyceliumNetworking Persistent");
			go.AddComponent<PersistentGameObject>();
			go.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(go);
		}
	}
}
