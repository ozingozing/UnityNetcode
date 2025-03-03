using ChocoOzing.CoreSystem;
using ChocoOzing.CoreSystem.StatSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : CoreComponent
{
	[SerializeField] private GameObject[] deathParicles;

	private Stats Stats => stats ? stats : Core.GetCoreComponent(ref stats);
	private Stats stats;

	public void Die(GameObject attacker)
	{
		CombatManager.Instance.OnPlayerDeath(attacker, gameObject);
	}

	public override void OnNetworkSpawn()
	{
		if (IsServer)
			Stats.OnCurrentValueZero += Die;
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		if (IsServer)
			Stats.OnCurrentValueZero -= Die;
		base.OnNetworkDespawn();
	}
}
