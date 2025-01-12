using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
	[SerializeField] private Transform spawnedObjectPrefab;
	private Transform spawnedObjectTransform;

	private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
			new MyCustomData
			{
				_int = 56,
				_bool = true,
			},
			NetworkVariableReadPermission.Everyone,
			NetworkVariableWritePermission.Owner
		);

	public struct MyCustomData : INetworkSerializable
	{
		public int _int;
		public bool _bool;
		public FixedString128Bytes _string;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref _int);
			serializer.SerializeValue(ref _bool);
			serializer.SerializeValue(ref _string);
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
		{
			Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue._string);
		};
	}


	private void Update()
	{

		if(!IsOwner) return;

		/*if (Input.GetKeyDown(KeyCode.T))
		{
			spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
			spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
			
			//TestServerRPC(new ServerRpcParams());
			//TestClientRPC(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
			*//*randomNumber.Value = new MyCustomData {
				_int = Random.Range(0, 10),
				_bool = Random.Range(0, 2) == 0 ? false : true,
				_string = "asdasdsad",
			};*//*
		}*/

		if(Input.GetKeyDown(KeyCode.Y))
		{
			Destroy(spawnedObjectTransform.gameObject);
		}

		Vector3 moveDir = Vector3.zero;

		if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
		if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
		if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
		if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

		float moveSpeed = 3f;
		transform.position += moveDir * moveSpeed * Time.deltaTime;
	}

	[ServerRpc]
	private void TestServerRPC(ServerRpcParams serverRpcParams)
	{
		Debug.Log("TestServerRPC " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
	}

	[ClientRpc]
	private void TestClientRPC(ClientRpcParams clientRpcParams)
	{
		Debug.Log("TestClientRPC");
	}
}
