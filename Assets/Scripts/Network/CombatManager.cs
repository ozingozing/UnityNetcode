using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
	public event EventHandler Respawn;

	private void Awake()
	{
		Instance = this;
	}

	public void OnPlayerDeath(GameObject killer, GameObject victim)
	{
		ulong killerId = killer.GetComponentInParent<NetworkObject>().OwnerClientId;
		ulong victimId = victim.GetComponentInParent<NetworkObject>().OwnerClientId;

		DieClientSet(victim);

		if (NetworkManager.Singleton.IsServer)
		{
			var killerStats = NetworkManager.Singleton.ConnectedClients[killerId].PlayerObject.GetComponent<PlayerStats>();
			var victimStats = NetworkManager.Singleton.ConnectedClients[victimId].PlayerObject.GetComponent<PlayerStats>();

			if(killerStats != null)
			{
				killerStats.AddKill();
			}

			if(victimStats != null)
			{
				victimStats.AddDeath();
			}
		}
	}

	public void DieClientSet(GameObject victiom)
	{
		/*victiom.GetComponent<Renderer>().enabled = false;
		victiom.GetComponent<Collider>().enabled = false;*/

		victiom.GetComponent<PlayerStats>().IsDead = true;
		victiom.GetComponent<PlayerStats>().TurnOffMeshClientRpc();
		// ���� �ð� �� Respawn �̺�Ʈ�� �����ϴ� �ڷ�ƾ ȣ��
		StartCoroutine(InvokeRespawnAfterDelay(3f)); // 3�� �Ŀ� Respawn �̺�Ʈ ����
	}

	private IEnumerator InvokeRespawnAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay); // delay ��ŭ ���

		Respawn?.Invoke(this, EventArgs.Empty); // Respawn �̺�Ʈ ����
	}
}
