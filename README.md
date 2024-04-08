# An easy to use networking library for sending custom RPCs through Steam.

## A note
Landfall has asked mod developers not to send lots of data through Photon, which is the networking solution mainly used by the game. This is because they have to pay for all the bandwidth that modders use. To solve this issue, this mod was created to be used as an alternative to Photon. With MyceliumNetworking, RPCs can be used in a very similar fashion to Photon without comprimising on features.

## Usage
1. Add MyceliumNetworkingForCW.dll as a reference in your Visual Studio solution.
2. Add `using MyceliumNetworking;` at the top of your script.

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
To call an RPC, first register the object with the network. Each mod needs a **unique** ID, which is just a random number you define unique to your mod. This is to make sure mods don't accidentally call an RPC on a separate mod. I recommend you store the modId as a constant somewhere in your program.

```cs
const uint modId = 12345;

void Start()
{
	MyceliumNetwork.RegisterNetworkObject(this, modId);
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

### Keypress Syncing Demo

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

### Using Masks
Sometimes you only want to run an RPC on a single instance of an object, for example calling an RPC on one player. To do this, you use masks.

Pass the ViewID of the PhotonView on your object in as the mask when registering a network object.

```cs
class PlayerTest : MonoBehaviour
{
	void Start()
	{
		MyceliumNetwork.RegisterNetworkObject(this, BasePlugin.MOD_ID, GetComponent<PhotonView>().ViewID);
	}
}
```

Then when calling the RPC, use the Masked variant.

```cs
MyceliumNetwork.RPCMasked(modId, nameof(KillPlayer), ReliableType.Reliable, GetComponent<PhotonView>().ViewID, "You Died!");
```

A masked RPC will only be called on objects with the same mask. Using this, you can make the RPC only be called on a single PhotonView, synced across clients.

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