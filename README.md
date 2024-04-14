# An easy to use networking library for sending custom RPCs through Steam.

[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/RugbugRedfern/Mycelium-Networking-For-Content-Warning/build.yml?style=for-the-badge&logo=github)](https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning/actions/workflows/build.yml)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/RugbugRedfern/MyceliumNetworking?style=for-the-badge&logo=thunderstore&logoColor=white&color=%23328EFF)](https://thunderstore.io/c/content-warning/p/RugbugRedfern/MyceliumNetworking/)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/RugbugRedfern/MyceliumNetworking?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/content-warning/p/RugbugRedfern/MyceliumNetworking/)
[![NuGet Version](https://img.shields.io/nuget/v/RugbugRedfern.MyceliumNetworking.CW?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/RugbugRedfern.MyceliumNetworking.CW)

## A note
Landfall has asked mod developers not to send lots of data through Photon, which is the networking solution mainly used by the game. This is because they have to pay for all the bandwidth that modders use. To solve this issue, this mod was created to be used as an alternative to Photon. With MyceliumNetworking, RPCs can be used in a very similar fashion to Photon without compromising on features.

## Setup
1. Add MyceliumNetworkingForCW.dll as a reference in your Visual Studio solution.
2. Add `using MyceliumNetworking;` at the top of your script.

## Using RPCs

### Defining RPCs
To define an RPC, simply add the [CustomRPC] attribute to a method:

```cs
[CustomRPC]
void ChatMessage(string message)
{
	Debug.Log("Received chat message: " + message);
}
```

To get the player who sent an RPC, add a RPCInfo as the final parameter in your RPC. You don't need to send the info yourself, it will be provided automatically if you include the parameter.
```cs
[CustomRPC]
void ChatMessage(string message, RPCInfo info)
{
	CSteamID sender = info.SenderSteamID;
	string username = SteamFriends.GetFriendPersonaName(sender);
	Debug.Log("Received chat message from " + username + ": " + message);
}
```

You can add as many arbitrary parameters to an RPC as you want, and they will automatically be synced when used.
```cs
[CustomRPC]
void DoSomething(string message, int num, ulong id)
{
	Debug.Log("The message is " + message);
	Debug.Log("The num is " + num);
	Debug.Log("The id is " + id);
}
```

### Calling RPCs
To call an RPC, first register the object with the network. Each mod needs a **unique** ID, which is just a random number you define unique to your mod. This is to make sure mods don't accidentally call an RPC on a separate mod. I recommend you store the modId as a constant somewhere in your program. Deregister the object when it is destroyed (for singletons or static classes, it's fine to never deregister).

```cs
const uint modId = 12345;

void Awake()
{
	MyceliumNetwork.RegisterNetworkObject(this, modId);
}

void OnDestroy()
{
	MyceliumNetwork.DeregisterNetworkObject(this, modId);
}
```

To call the RPC, use `MyceliumNetwork.RPC`. Pass in the mod ID, the name of the RPC you are calling, what kind of reliability you want the RPC to have (leave it at Reliable if you are unsure), and the parameters to pass in the RPC:

```cs
// This will call the RPC named ChatMessage on all clients, with the parameter "Hello World!" being sent.
MyceliumNetwork.RPC(modId, nameof(ChatMessage), ReliableType.Reliable, "Hello World!");
```
You can also call an RPC on a specific player using `MyceliumNetwork.RPCTarget`.
```cs
// This will call the RPC named ChatMessage on only the specified player, with the parameter "Hello World!" being sent.
MyceliumNetwork.RPCTarget(modId, nameof(ChatMessage), targetSteamId, ReliableType.Reliable, "Hello World!");
```

## Using LobbyData and PlayerData
LobbyData and PlayerData are Steam features that allow you to define synced variables associated with the lobby (perfect for config syncing) or individual players. Mycelium provides an easy way to interface with them.

### Lobby Data
To use lobby data, you first need to register the key. This should happen when your **mod starts for the first time**. This step is optional but strongly recommended. It's required if you want the LobbyDataUpdated callback to fire when that key's value is changed.
```cs
void Awake()
{
	MyceliumNetwork.RegisterLobbyDataKey("foo");
}
```

The host can set lobby data using `MyceliumNetwork.SetLobbyData` and passing in a registered key with a value.
```cs
MyceliumNetwork.SetLobbyData("foo", "bar");
```

Values are serialized as strings (Steam requires it), so any type that can be serialized as a string can be used. To serialize custom types, override the `.ToString()` method.
```cs
MyceliumNetwork.SetLobbyData("money", 123);
MyceliumNetwork.SetLobbyData("scoreMultiplier", 12.52f);
MyceliumNetwork.SetLobbyData("greeting", "Hello World!");
```

The LobbyDataUpdated callback will be fired whenever lobby data is updated by the host. It provides a list of the keys of lobby data that were changed.

Any client can access lobby data using `MyceliumNetwork.GetLobbyData`. Pass in the type of the data to automatically cast it.
```cs
string data = MyceliumNetwork.GetLobbyData<string>("foo");
Debug.Log(data); // Hello World

int data = MyceliumNetwork.GetLobbyData<int>("money");
Debug.Log(data); // 123
```

### Player Data
Player data is the same as lobby data, but associated with specific players.

To use player data, you first need to register the key. This should happen when your **mod starts for the first time**. This step is optional but strongly recommended. It's required if you want the PlayerDataUpdated callback to fire when that key's value is changed.
```cs
void Awake()
{
	MyceliumNetwork.RegisterPlayerDataKey("foo");
}
```

Players can then set their own player data using `MyceliumNetwork.SetPlayerData` and passing in a registered key with a value. The same serialization as with LobbyData applies to PlayerData.
```cs
MyceliumNetwork.SetPlayerData("foo", "bar");
```

The PlayerDataUpdated callback will be fired whenever player data is updated. It provides the player whose data was changed, a list of the keys of player data that were changed.

Any client can access player data using `MyceliumNetwork.GetPlayerData`. Pass in the type of the data to automatically cast it.
```cs
string data = MyceliumNetwork.GetPlayerData<string>(MyceliumNetwork.LobbyHost, "foo");
Debug.Log(data); // bar
```

## Using Masks
RPCs are synced using the method name. If you have multiple methods with the same name on different objects, they will all be called when an RPC is fired. But sometimes you only want to run an RPC on a single instance of an object, for example calling an RPC on one player. To do this, you use masks.

Pass the ViewID of the PhotonView on your object in as the mask when registering a network object.

```cs
class PlayerTest : MonoBehaviour
{
	int viewId;

	void Awake()
	{
		viewId = GetComponent<PhotonView>().ViewID;
		MyceliumNetwork.RegisterNetworkObject(this, BasePlugin.MOD_ID, viewId);
	}

	void OnDestroy()
	{
		// you will need to store the viewId locally, because you cannot access a PhotonView in OnDestroy.
		MyceliumNetwork.DeregisterNetworkObject(this, BasePlugin.MOD_ID, viewId);
	}
}
```

Then when calling the RPC, use the Masked variant.

```cs
MyceliumNetwork.RPCMasked(modId, nameof(KillPlayer), ReliableType.Reliable, GetComponent<PhotonView>().ViewID, "You Died!");
```

A masked RPC will only be called on objects with the same mask. Using this, you can make the RPC only be called on a single PhotonView, synced across clients.

## Other Features
Mycelium contains a lot of other small but useful features to make your multiplayer development easier.

### Properties
- `MycelumNetwork.Players`: Access a list of all players in the lobby
- `MycelumNetwork.PlayerCount`: Get the count of players in the lobby
- `MycelumNetwork.MaxPlayers`: Get maximum count of players that can fit in the Steam lobby (this is not the same as the Photon lobby)
- `MycelumNetwork.LobbyHost`: Get the host of the lobby
- `MycelumNetwork.IsHost`: Returns true if the local player is the lobby host
- `MycelumNetwork.Lobby`: Get the SteamID of the lobby
- `MycelumNetwork.InLobby`: Returns true if the local player is currently in a lobby

### Events
- `MycelumNetwork.LobbyCreated`: Called when a lobby is successfully created by the local player
- `MycelumNetwork.LobbyCreationFailed`: Called when a lobby creation is failed by the local player
- `MycelumNetwork.LobbyEntered`: Called when a lobby is entered by the local player
- `MycelumNetwork.LobbyLeft`: Called when a lobby is left by the local player
- `MycelumNetwork.PlayerEntered`: Called whenever another player enters the lobby
- `MycelumNetwork.PlayerLeft`: Called whenever another player leaves the lobby
- `MycelumNetwork.PlayerDataUpdated`: Called whenever a player's data is updated (including the local player)
- `MycelumNetwork.LobbyDataUpdated`: Called when the lobby's data is updated

## Keypress Syncing Demo

To demonstrate how to define and send RPCs, there is a full demo you can access [here](https://github.com/RugbugRedfern/Mycelium-Networking-For-Content-Warning-Demo), but this is the main important script.
It simply demonstrates how to use the RPCs by sending all key presses between players to the console.
```cs
using MyceliumNetworking;
using Steamworks;
using System;
using UnityEngine;

namespace MyceliumNetworkingTest
{
	internal class SyncedGameObject : MonoBehaviour
	{
		void Start()
		{
			MyceliumNetwork.RegisterNetworkObject(this, BasePlugin.MOD_ID);
		}

		void Update()
		{
			foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
			{
				if(Input.GetKeyDown(kcode))
				{
					MyceliumNetwork.RPC(BasePlugin.MOD_ID, nameof(KeyReceived), ReliableType.Reliable, kcode.ToString());
				}
			}
		}

		[CustomRPC]
		public void KeyReceived(string keyPressed, RPCInfo info)
		{
			CSteamID sender = info.SenderSteamID;
			string username = SteamFriends.GetFriendPersonaName(sender);
			Debug.Log("Received key from " + username + ": " + keyPressed);
		}
	}
}
```
![](https://i.ibb.co/DY9P9sn/image.png)

### Need Help?
Join the Content Warning Modding discord and @ me (@rugdev): https://discord.gg/yeGDSm4gFq

## Credits
[![](https://i.ibb.co/pLJ3Zrn/a-mod-by-rugbug.png)](https://www.youtube.com/RugbugRedfern)

### Contributors
- [Rugbug Redfern](https://rugbug.net/)
- Sprinkles
- [zatrit](https://github.com/zatrit)
- [Xilophor](https://github.com/Xilophor)

### Playtesting
- [Dubscr](https://www.youtube.com/dubscr)

## Code License: Attribution-NonCommercial-ShareAlike 4.0 International
https://creativecommons.org/licenses/by-nc-sa/4.0/