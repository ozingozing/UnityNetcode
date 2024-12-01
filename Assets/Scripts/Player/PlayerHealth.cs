using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IHealth
{
	// �������̽����� ���ǵ� �Ӽ� ����
	public int Health { get; set; }

	[SerializeField] private string playerLobbyId;

	[SerializeField] private int currentHealth;
	[SerializeField] private int SetHealth;
	PlayerStats playerStats;
	private void Awake()
	{
		playerStats = GetComponent<PlayerStats>();
	}

	private void Start()
	{
		playerStats.deaths.OnValueChanged += Test;
		currentHealth = SetHealth;
	}

	private void Test(int previousValue, int newValue)
	{
		currentHealth = SetHealth;
	}

	public void InitializePlayerHealth(object sender, System.EventArgs e)
	{
		currentHealth = SetHealth;
	}

	public void TakeDamage(int amount, GameObject attacker)
	{
		currentHealth -= amount;
		if (currentHealth <= 0)
		{
			OnDeath(attacker); // �����ڸ� �����Ͽ� ���� ó��
		}
	}

	// �÷��̾� ��� ó��
	private void OnDeath(GameObject attacker)
	{
		/*// �������� clientId ��������
		ulong attackerId = attacker.GetComponent<NetworkObject>().OwnerClientId;

		// �ڽ��� clientId ��������
		ulong victimId = GetComponent<NetworkObject>().OwnerClientId;
*/
		// ų/���� ó��
		CombatManager.Instance.OnPlayerDeath(attacker, gameObject);
	}
}
