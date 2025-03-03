using ChocoOzing.CoreSystem;
using ChocoOzing.CoreSystem.StatSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DamageReceiver : CoreComponent, IHealth
{
	struct ShootAction
	{
		public int amount;
		public GameObject attacker;
		public ShootAction(int amount, GameObject attacker)
		{
			this.amount = amount;
			this.attacker = attacker;
		}
	}
	public Stats Stats => stats ? stats : Core.GetCoreComponent(ref stats);
	private Stats stats;
	Queue<ShootAction> queue = new Queue<ShootAction>();


	public void TakeDamage(int amount, GameObject attacker)
	{
		//TakeDamage(Stats.Decrease(amount, attacker));
		//queue.Enqueue(new ShootAction(amount, attacker));
	}
	[ServerRpc(RequireOwnership = false)]
	public void TakeDamageServerRpc(int amount, ulong attackerId)
	{
		if (NetworkManager.Singleton.ConnectedClients.TryGetValue(attackerId, out var client))
		{
			TakeDamageClientRpc(Stats.Decrease(amount, client.PlayerObject.gameObject));
		}
		//queue.Enqueue(new ShootAction(amount, attacker));
	}

	/*public override void LogicUpdate()
	{
		base.LogicUpdate();
		while (queue.Count > 0)
		{
			ShootAction action = queue.Dequeue();
			TakeDamageClientRpc(Stats.Decrease(action.amount, action.attacker));
		}
	}*/

	[ClientRpc]
	public void TakeDamageClientRpc(float currentHp)
	{
		Stats.DecreaseBroadcast(currentHp, IsLocalPlayer);
	}
}
