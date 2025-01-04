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

	private void OnEnable()
	{
		Stats.Health.OnCurrentValueZero += Die;
	}

	private void OnDestroy()
	{
		Stats.Health.OnCurrentValueZero -= Die;
	}
}
