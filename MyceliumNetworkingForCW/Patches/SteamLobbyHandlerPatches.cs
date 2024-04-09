using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyceliumNetworking.Patches
{
	[HarmonyPatch(typeof(SteamLobbyHandler))]
	internal class SteamLobbyHandlerPatches
	{
		[HarmonyPatch(nameof(SteamLobbyHandler.LeaveLobby))]
		[HarmonyPostfix]
		static void LeaveLobbyPatch()
		{
			RugLogger.Log("Patching SteamLobbyHandler.LeaveLobby");
			MyceliumNetwork.OnLobbyLeft();
		}
	}
}
