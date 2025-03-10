using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Netcode;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

	private void Awake()
	{
		Instance = this;
	}

	public void OnPlayerDeath(GameObject killer, GameObject victim)
	{
		ulong killerId = killer.GetComponentInParent<NetworkObject>().OwnerClientId;
		ulong victimId = victim.GetComponentInParent<NetworkObject>().OwnerClientId;

		var killerStats = NetworkManager.Singleton.ConnectedClients[killerId].PlayerObject.GetComponent<PlayerInit>();
		var victimStats = NetworkManager.Singleton.ConnectedClients[victimId].PlayerObject.GetComponent<PlayerInit>();

		if (killerStats != null)
		{
			killerStats.AddKill();
		}

		if (victimStats != null)
		{
			victimStats.AddDeath();
		}
	}
}
