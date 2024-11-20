using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

public class InGameManager : MonoBehaviour
{
	static public InGameManager Instance { get; private set; }

	// Dictionary to store PlayerData with playerId as key
	public Dictionary<string, PlayerData> playerDataDictionary = new Dictionary<string, PlayerData>();



	[SerializeField] private GameObject playerPrefab;
	public List<ulong> connectedClients = new List<ulong>();

	public NetworkManager NetworkManager;
	public WalkerGenerator MapManager;
	public SurfaceManager SurfaceManager;
	public CombatManager CombatManager;

	private void Awake()
	{
		Instance = this;

		MapManager = GetComponentInChildren<WalkerGenerator>();
		SurfaceManager = GetComponentInChildren<SurfaceManager>();
		CombatManager = GetComponentInChildren<CombatManager>();
		NetworkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
	}

	private void Start()
	{
		NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
		// 반드시 NetworkManager의 Connection Approval을 활성화해야 합니다.
		NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisConnected;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			InGamePlayerUIManager.Instance.PanelFadeIn();
		}
		else if (Input.GetKeyUp(KeyCode.Tab))
		{
			InGamePlayerUIManager.Instance.PanelFadeOut();
		}
	}

	private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		response.Approved = true;
		response.CreatePlayerObject = false; // 자동 생성 비활성화
	}

	private void OnClientConnected(ulong clientId)
	{
		MapManager.playerCount++;
		connectedClients.Add(clientId);
	}


	private void OnClientDisConnected(ulong clientId)
	{
		connectedClients.Remove(clientId);
	}

	/*여기서는 HostClient를 제외한 Client들은 동기화 작업이 Y축만 적용
	 *그래서 OnNetworkSpawn Callback에서 Host가 아닌 Client들에게
	 *RandomPos 1번 더 호출*/
	public void SpawnPlayer()
	{
		foreach (ulong id in connectedClients)
		{
			GameObject playerInstance =
			Instantiate(
				playerPrefab,
				new Vector3(0, 30, 0),
				Quaternion.identity
			);
			playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
		}
	}
}

[System.Serializable]
public class PlayerData
{
	public Player player;
	public Transform playerSingleUI;
	public string playerLobbyId;
	public string playerName;
	public Sprite playerCharacterSprite;
	public LobbyManager.PlayerCharacter playerCharacterImage;

	public PlayerData(string playerLobbyId, Player player, string playerName, Sprite playerCharacterSprite, LobbyManager.PlayerCharacter playerCharacterImage)
	{
		this.player = player;
		this.playerLobbyId = playerLobbyId;
		this.playerName = playerName;
		this.playerCharacterSprite = playerCharacterSprite;
		this.playerCharacterImage = playerCharacterImage;
	}
}