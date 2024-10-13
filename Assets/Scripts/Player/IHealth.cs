using UnityEngine;

public interface IHealth
{
	int Health { get; set; }
	public void TakeDamage(int amount, GameObject attacker);
}
