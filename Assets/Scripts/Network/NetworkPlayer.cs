using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayer : NetworkBehaviour
{
	public static Action<GameObject> OnPlayerSpawn;
	public static Action<GameObject> OnPlayerDespawn;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		OnPlayerSpawn?.Invoke(this.gameObject);

		Vector3 Pos = NetworkManager.gameObject.GetComponent<SpawnPoint>().GetRandomSpawnPoint();
		Debug.Log(Pos);

		GetComponent<Rigidbody>().position = Pos;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		OnPlayerDespawn?.Invoke(this.gameObject);
	}
}
