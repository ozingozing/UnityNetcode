using Architecture.AbilitySystem.Model;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Effect : NetworkBehaviour
{
	public AbilityData abilityData;
	private void OnDisable()
	{
		if (GetComponent<NetworkObject>().IsSpawned && IsServer)
		{
			NetworkObjectPool.Singleton.ReturnNetworkObject(GetComponent<NetworkObject>(), abilityData.GetAreaOfEffectData(abilityData.abilityType).prefab);
		}
	}

	/*public override void OnDestroy()
	{
		if (!GetComponent<NetworkObject>().IsDestroyed() && IsServer)
		{
			GetComponent<NetworkObject>().Despawn();
		}
	}*/
}
