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

	NetworkObject networkObject;


	private void Awake()
	{
		networkObject = GetComponent<NetworkObject>();
	}

	private void Start()
	{
		CombatManager.Instance.Respawn += InitializePlayerHealth;
		currentHealth = SetHealth;
	}

	public void InitializePlayerHealth(object sender, System.EventArgs e)
	{
		currentHealth = SetHealth;
	}

	/*Test Func*/
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
		Debug.Log("Death!!!!!!!!!!!");
		CombatManager.Instance.OnPlayerDeath(attacker, gameObject);
	}
}
