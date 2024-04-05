using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MyceliumNetworking
{
	public enum ReliableType { Unreliable, Reliable, UnreliableNoDelay };

	public static class MyceliumNetwork
	{
		const int CHANNEL = 120;

		/// <summary>
		/// The list of all players in this lobby.
		/// The order is not guaranteed to be consistent for all players.
		/// </summary>
		public static CSteamID[] Players { get; private set; } = new CSteamID[0];

		/// <summary>
		/// The number of players in this lobby
		/// </summary>
		public static int PlayerCount => SteamMatchmaking.GetNumLobbyMembers(Lobby);

		/// <summary>
		/// The maximum number of players in this lobby
		/// </summary>
		public static int MaxPlayers => SteamMatchmaking.GetLobbyMemberLimit(Lobby);

		/// <summary>
		/// The owner of this lobby.
		/// </summary>
		public static CSteamID LobbyHost => SteamMatchmaking.GetLobbyOwner(Lobby);

		/// <summary>
		/// Are you the host of this lobby?
		/// </summary>
		public static bool IsHost => SteamMatchmaking.GetLobbyOwner(Lobby) == SteamUser.GetSteamID();

		/// <summary>
		/// The current lobby
		/// </summary>
		public static CSteamID Lobby;

		/// <summary>
		/// Called when a lobby is created by the local player
		/// </summary>
		public static Action LobbyCreated;

		/// <summary>
		/// Called when a lobby is entered by the local player
		/// </summary>
		public static Action LobbyEntered;

		/// <summary>
		/// Called when a lobby is left by the local player
		/// </summary>
		public static Action LobbyLeft;

		/// <summary>
		/// Called when lobby creation has failed on the local player
		/// </summary>
		public static Action<EResult> LobbyCreationFailed;

		/// <summary>
		/// Called a player enters the lobby
		/// </summary>
		public static Action<CSteamID> PlayerEntered;

		/// <summary>
		/// Called when a player leaves the lobby
		/// </summary>
		public static Action<CSteamID> PlayerLeft;

		static Callback<LobbyEnter_t> _c2;
		static Callback<LobbyCreated_t> _c3;
		static Callback<LobbyChatUpdate_t> _c4;

		public static void Initialize()
		{
			_c2 = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
			_c3 = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			_c4 = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		}

		static void OnLobbyEnter(LobbyEnter_t param)
		{
			RugLogger.Log($"Entering lobby {param.m_ulSteamIDLobby}");

			Lobby = new CSteamID(param.m_ulSteamIDLobby);

			RefreshPlayerList();

			LobbyEntered?.Invoke();
		}

		static void OnLobbyCreated(LobbyCreated_t param)
		{
			RugLogger.Log($"Created lobby {param.m_eResult}");

			if(param.m_eResult == EResult.k_EResultOK)
			{
				Lobby = new CSteamID(param.m_ulSteamIDLobby);

				RefreshPlayerList();

				LobbyCreated?.Invoke();
			}
			else
			{
				LobbyCreationFailed?.Invoke(param.m_eResult);
			}
		}

		// Called whenever a user leaves or joins our lobby
		static void OnLobbyChatUpdate(LobbyChatUpdate_t param)
		{
			var steamID = new CSteamID(param.m_ulSteamIDUserChanged);

			RefreshPlayerList();

			switch((EChatMemberStateChange)param.m_rgfChatMemberStateChange)
			{
				case EChatMemberStateChange.k_EChatMemberStateChangeEntered:
					PlayerEntered?.Invoke(steamID);
					break;
				case EChatMemberStateChange.k_EChatMemberStateChangeLeft:
				case EChatMemberStateChange.k_EChatMemberStateChangeDisconnected:
				case EChatMemberStateChange.k_EChatMemberStateChangeKicked:
				case EChatMemberStateChange.k_EChatMemberStateChangeBanned:
					PlayerLeft?.Invoke(steamID);
					break;
			}
		}

		/// <summary>
		/// Get an updated version of the player list and cache it in the Players array.
		/// It's called whenever a player leaves or joins the room, so you shouldn't need to call it.
		/// </summary>
		static void RefreshPlayerList()
		{
			Players = new CSteamID[SteamMatchmaking.GetNumLobbyMembers(Lobby)];
			for(int i = 0; i < Players.Length; i++)
			{
				CSteamID id = SteamMatchmaking.GetLobbyMemberByIndex(Lobby, i);
				Players[i] = id;
			}
		}

		#region RPCs
		/// <summary>
		/// Send a RPC over the network to another player
		/// </summary>
		public static void RPCTargetMasked(uint modId, string methodName, CSteamID target, ReliableType reliable, int mask, params object[] parameters)
		{
			var msg = new Message(modId, methodName, mask);

			var handler = GetMessageHandlers(modId, methodName)[0];

			int targetParameterCount = handler.Parameters.Length;

			if(handler.TakesInfo)
			{
				targetParameterCount--;
			}

			if(targetParameterCount != parameters.Length)
			{
				throw new Exception($"RPC call {modId}: {methodName} has an invalid number of parameters (it has {parameters.Length}, but it should have {targetParameterCount})");
			}

			for(int i = 0; i < targetParameterCount; i++)
			{
				if(handler.Parameters[i].ParameterType != parameters[i].GetType())
				{
					throw new Exception($"RPC call {modId}: {methodName} has a mismatched parameter type ({parameters[i].GetType()} should be {handler.Parameters[i].ParameterType}) for {handler.Parameters[i].Name}");
				}

				msg.WriteObject(handler.Parameters[i].ParameterType, parameters[i]);
			}

			SendBytes(msg.ToArray(), target, reliable);
		}

		/// <summary>
		/// Send a RPC over the network to another player
		/// </summary>
		public static void RPCTarget(uint modId, string methodName, CSteamID target, ReliableType reliable, params object[] parameters)
		{
			RPCTargetMasked(modId, methodName, target, reliable, 0, parameters);
		}

		/// <summary>
		/// Send a RPC over the network to all other players
		/// </summary>
		public static void RPCMasked(uint modId, string methodName, ReliableType reliable, int mask, params object[] parameters)
		{
			for(int i = 0; i < Players.Length; i++)
			{
				RPCTargetMasked(modId, methodName, Players[i], reliable, mask, parameters);
			}
		}

		/// <summary>
		/// Send a RPC over the network to all other players
		/// </summary>
		public static void RPC(uint modId, string methodName, ReliableType reliable, params object[] parameters)
		{
			RPCMasked(modId, methodName, reliable, 0, parameters);
		}
		#endregion

		/// <summary>
		/// Send a byte array over the network to a specific player
		/// </summary>
		/// <param name="data">The data to send</param>
		/// <param name="target">The target player to send the data to</param>
		/// <param name="reliable">True for reliable but slow, false for unreliable but fast</param>
		static void SendBytes(byte[] data, CSteamID target, ReliableType reliable)
		{
			if(data.Length > Message.MaxSize)
			{
				RugLogger.LogError($"<b>MyceliumNetwork</b> Size of message ({data.Length} bytes) is greater than the max allowed size ({Message.MaxSize}).");
				return;
			}

			if(target == SteamUser.GetSteamID())
			{
				var msg = new Message(data);

				HandleMessage(msg, target);
				return;
			}

			SteamNetworkingIdentity id = new SteamNetworkingIdentity();
			id.SetSteamID(target);

			GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr pData = pinnedArray.AddrOfPinnedObject();

			int sendFlags = 0;
			switch(reliable)
			{
				case ReliableType.Unreliable:
					sendFlags = Steamworks.Constants.k_nSteamNetworkingSend_Unreliable;
					break;
				case ReliableType.Reliable:
					sendFlags = Steamworks.Constants.k_nSteamNetworkingSend_Reliable;
					break;
				case ReliableType.UnreliableNoDelay:
					sendFlags = Steamworks.Constants.k_nSteamNetworkingSend_UnreliableNoDelay;
					break;
			}

			var result = SteamNetworkingMessages.SendMessageToUser(ref id, pData, (uint)data.Length, sendFlags | Steamworks.Constants.k_nSteamNetworkingSend_AutoRestartBrokenSession, CHANNEL);
			if(result != EResult.k_EResultOK)
			{
				RugLogger.LogError($"Error sending message to user {target}: {result}");
			}

			pinnedArray.Free();
		}

		static List<MessageHandler> GetMessageHandlers(uint modID, string methodName)
		{
			if(rpcs.TryGetValue(modID, out var methods))
			{
				if(methods.TryGetValue(methodName, out var handlers))
				{
					return handlers;
				}
				else
				{
					throw new Exception($"The method ({modID}: {methodName}) was not found");
				}
			}
			else
			{
				throw new Exception($"The mod Id {modID} was not found (loaded mods: {string.Join(",", rpcs.Keys.ToArray())})");
			}
		}

		static void HandleMessage(Message message, CSteamID sender)
		{
			try
			{
				var handlers = GetMessageHandlers(message.ModID, message.MethodName);

				foreach(var messageHandler in handlers)
				{
					if(messageHandler.Mask == message.Mask)
					{
						var msgParams = new object[messageHandler.Parameters.Length];

						var count = messageHandler.TakesInfo ? msgParams.Length - 1 : msgParams.Length;

						for(int i = 0; i < count; i++)
						{
							msgParams[i] = message.ReadObject(messageHandler.Parameters[i].ParameterType);
						}

						if(messageHandler.TakesInfo)
						{
							msgParams[messageHandler.Parameters.Length - 1] = new RPCInfo(sender);
						}

						messageHandler.Method.Invoke(messageHandler.Target, msgParams);
					}
				}
			}
			catch(Exception ex)
			{
				string destination;

				try
				{
					destination = message.GetDestination();
				}
				catch(Exception destinationEx)
				{
					destination = destinationEx.Message;
				}

				Debug.LogError($"Error executing RPC from {(sender == SteamUser.GetSteamID() ? "local loopback" : sender.ToString())} ({destination}): {ex.Message} {ex.StackTrace}");
			}
		}

		const int MAX_MESSAGES = 500;
		static IntPtr[] outMessages = new IntPtr[MAX_MESSAGES];

		/// <summary>
		/// Call to recieve any pending messages from other players
		/// </summary>
		public static void ReceiveMessages()
		{
			int count = SteamNetworkingMessages.ReceiveMessagesOnChannel(CHANNEL, outMessages, MAX_MESSAGES);

			for(int i = 0; i < count; i++)
			{
				IntPtr outMessage = outMessages[i];

				int messageSize = Marshal.SizeOf(outMessage);
				if(messageSize > Message.MaxSize)
				{
					Debug.LogError($"Ignored message because its size was above the max ({messageSize}/{Message.MaxSize})");
					SteamNetworkingMessage_t.Release(outMessages[i]);
					continue;
				}

				SteamNetworkingMessage_t steamNetworkingMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(outMessage);

				CSteamID sender = steamNetworkingMessage.m_identityPeer.GetSteamID();

				// Ignore them if their steam id is null (happens sometimes idk)
				if(sender == null)
				{
					SteamNetworkingMessage_t.Release(outMessages[i]);
					continue;
				}

				// Ignore them if they're not in the lobby
				if(!Players.Contains(sender))
				{
					SteamNetworkingMessage_t.Release(outMessages[i]);
					continue;
				}

				byte[] managedArray = new byte[steamNetworkingMessage.m_cbSize];
				Marshal.Copy(steamNetworkingMessage.m_pData, managedArray, 0, steamNetworkingMessage.m_cbSize);

				var message = new Message(managedArray);

				try
				{
					HandleMessage(message, sender);
				}
				catch(Exception ex)
				{
					string destination;

					try
					{
						destination = message.GetDestination();
					}
					catch(Exception destinationEx)
					{
						destination = destinationEx.Message;
					}

					Debug.LogError($"Error executing RPC from {sender} ({destination}): {ex.Message} {ex.StackTrace}");
				}

				SteamNetworkingMessage_t.Release(outMessages[i]);
			}
		}

		#region Message Handlers
		static Dictionary<uint, Dictionary<string, List<MessageHandler>>> rpcs = new Dictionary<uint, Dictionary<string, List<MessageHandler>>>();

		public static void RegisterNetworkObject(object obj, uint modId, int mask = 0)
		{
			var t = obj.GetType();

			int registeredMethods = 0;

			foreach(var method in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				// Get the attributes attached to the method
				var attributes = method.GetCustomAttributes(false).OfType<CustomRPCAttribute>().ToArray();

				// If there are any attributes
				if(attributes.Any())
				{
					var parameters = method.GetParameters();

					if(!rpcs.ContainsKey(modId))
					{
						rpcs[modId] = new Dictionary<string, List<MessageHandler>>();
					}

					if(!rpcs[modId].ContainsKey(method.Name))
					{
						rpcs[modId][method.Name] = new List<MessageHandler>();
					}

					rpcs[modId][method.Name].Add(new MessageHandler
					{
						Target = obj,
						Method = method,
						Parameters = parameters,
						TakesInfo = parameters.Length > 0 && parameters[parameters.Length - 1].ParameterType == typeof(RPCInfo),
						Mask = mask
					});

					registeredMethods++;
				}
			}

			RugLogger.Log($"Registered {registeredMethods} CustomRPCs for {modId}: {obj}.");
		}

		class MessageHandler
		{
			public object Target;
			public MethodInfo Method;
			public ParameterInfo[] Parameters;
			public bool TakesInfo;
			public int Mask;
		}
		#endregion
	}

	public struct RPCInfo
	{
		public CSteamID SenderSteamID;

		public RPCInfo(CSteamID senderSteamId)
		{
			SenderSteamID = senderSteamId;
		}
	}
}
