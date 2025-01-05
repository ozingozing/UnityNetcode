using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace ChocoOzing.EventBusSystem
{
	public interface IEvent { }

	#region LobbyEvnet
	public enum LobbyState
	{
		Joined,
		Refresh,
		Leave,
		Start
	};
	public struct LobbyEventArgs :IEvent
	{
		public Lobby lobby;
		public LobbyState state;
		public List<Lobby> lobbyList;
	}
	#endregion

	#region Player
	public enum PlayerState
	{
		Init,
		Spawn,
		Despawn,
	}
	public struct PlayerOnSpawnState : IEvent
	{
		public GameObject player;
		public PlayerState state;
	}

	public struct PlayerAnimationEvent : IEvent
	{
		public int animationHash;
		public bool MoveLock;
	}
	#endregion

	public class Events : IEvent{}
}
