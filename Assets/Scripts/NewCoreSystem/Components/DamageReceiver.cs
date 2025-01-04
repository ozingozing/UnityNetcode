using ChocoOzing.CoreSystem;
using ChocoOzing.CoreSystem.StatSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : CoreComponent, IHealth
{
	private Stats Stats => stats ? stats : Core.GetCoreComponent(ref stats);
	private Stats stats;

	public void TakeDamage(int amount, GameObject attacker)
	{
		if (amount <= 0f)
		{
			return;
		}
		Stats.Health.Decrease(amount, attacker);
	}
}
