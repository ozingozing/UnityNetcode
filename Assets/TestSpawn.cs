using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class TestSpawn : MonoBehaviour
{
	[SerializeField] private GameObject playerPrefab;
	public List<ulong> connectedClients = new List<ulong>();	
	private ulong ClientId;

	public void Start()
	{
		NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
		// 반드시 NetworkManager의 Connection Approval을 활성화해야 합니다.
		NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisConnected;
	}

	private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		response.Approved = true;
		response.CreatePlayerObject = false; // 자동 생성 비활성화
	}

	private void OnClientConnected(ulong clientId)
	{
		GameObject.Find("MapManager").GetComponent<WalkerGenerator>().playerCount++;
		connectedClients.Add(clientId);
	}


	private void OnClientDisConnected(ulong clientId)
	{
		connectedClients.Remove(clientId);
	}


	public void SpawnPlayer()
	{
		foreach (ulong id in connectedClients)
		{
			GameObject playerInstance = Instantiate(playerPrefab);
			playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
		}
	}
}
