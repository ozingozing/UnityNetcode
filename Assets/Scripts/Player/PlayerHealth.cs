using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour, IHealth
{
	// 인터페이스에서 정의된 속성 구현
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
			OnDeath(attacker); // 공격자를 전달하여 죽음 처리
		}
	}

	// 플레이어 사망 처리
	private void OnDeath(GameObject attacker)
	{
		/*// 공격자의 clientId 가져오기
		ulong attackerId = attacker.GetComponent<NetworkObject>().OwnerClientId;

		// 자신의 clientId 가져오기
		ulong victimId = GetComponent<NetworkObject>().OwnerClientId;
*/
		// 킬/데스 처리
		Debug.Log("Death!!!!!!!!!!!");
		CombatManager.Instance.OnPlayerDeath(attacker, gameObject);
	}
}
