using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace ChocoOzing.EventBusSystem
{
	public interface IEvent { }

	#region LobbyEvnet
	public struct LobbyJoinedEvnetArgs :IEvent
	{
		public Lobby lobby;
	}
	public struct LobbyLeftEventArgs : IEvent{}

	public struct LobbyGameSartEventArgs : IEvent{}

	public struct OnLobbyListChangedEventArgs :IEvent
	{
		public List<Lobby> lobbyList;
	}
	#endregion

	#region Player
	public struct PlayerOnSpawnEvent : IEvent
	{
		public GameObject player;
	}

	public struct PlayerOnDespawnEvent : IEvent
	{
		public GameObject player;
	}

	public struct PlayerAnimationEvent : IEvent
	{
		public int animationHash;
	}

	public struct PlayerBaseAnimationEvent : IEvent
	{
		public int animationHash;
	}
	#endregion

	public class Events : IEvent{}
}
