using ChocoOzing.CoreSystem;
using ChocoOzing.CoreSystem.StatSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : CoreComponent, IHealth
{
	private Stats Stats => stats ? stats : core.GetCoreComponent(ref stats);
	private Stats stats;

	public int Health {
		get => throw new System.NotImplementedException();
		set => throw new System.NotImplementedException();
	}

	public void TakeDamage(int amount, GameObject attacker)
	{
		if (amount <= 0f)
		{
			return;
		}
		Stats.Health.Decrease(amount, attacker);
	}
}
