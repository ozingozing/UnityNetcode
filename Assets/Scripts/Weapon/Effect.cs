using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Effect : MonoBehaviour
{
	private void OnDisable()
	{
		NetworkObject @object = GetComponent<NetworkObject>();
		if(@object.IsSpawned && NetworkManager.Singleton.IsServer)
			GetComponent<NetworkObject>().Despawn();
	}
}
