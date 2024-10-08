using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IHealth
{
	// �������̽����� ���ǵ� �Ӽ� ����
	public int Health { get; set; }

	[SerializeField] private int currentHealth;
	[SerializeField] private int SetHealth;
	NetworkObject networkObject;

	private void Awake()
	{
		currentHealth = SetHealth;
		networkObject = GetComponent<NetworkObject>();
	}

	public void TakeDamage(int amount)
	{
		if (currentHealth > 0)
		{
			currentHealth -= amount;
			Debug.Log(currentHealth);
			if (currentHealth <= 0)
			{
				Debug.Log("Dead!!!!!!!!");
				if (IsServer)
					networkObject.Despawn();
			}
		}
	}
}
