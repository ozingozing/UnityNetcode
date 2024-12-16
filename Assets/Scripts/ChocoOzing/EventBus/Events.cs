using UnityEngine;

namespace ChocoOzing.EventBusSystem
{
	public interface IEvent { }

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

	public class Events : IEvent
	{

	}
}
