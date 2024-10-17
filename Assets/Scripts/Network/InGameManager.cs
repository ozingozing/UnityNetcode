using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Events;

public class InGameManager : MonoBehaviour
{
	static public InGameManager Instance { get; private set; }

	// Dictionary to store PlayerData with playerId as key
	public Dictionary<string, PlayerData> playerDataDictionary = new Dictionary<string, PlayerData>();
	
	public event EventHandler SetInfoInGame;
	public event EventHandler SetKdaInfo;
	public event EventHandler InGame;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			InGamePlayerUIManager.Instance.gameObject.SetActive(true);
		}
		else if (Input.GetKeyUp(KeyCode.Tab))
		{
			InGamePlayerUIManager.Instance.gameObject.SetActive(false);
		}
	}
}

[System.Serializable]
public class PlayerData
{
	public GameObject playerGO;
	public Transform playerSingleUI;
	public string playerLobbyId;
	public string playerName;
	public Sprite playerCharacterSprite;
	public LobbyManager.PlayerCharacter playerCharacterImage;

	public PlayerData(string playerLobbyId, string playerName, Sprite playerCharacterSprite, LobbyManager.PlayerCharacter playerCharacterImage)
	{
		this.playerLobbyId = playerLobbyId;
		this.playerName = playerName;
		this.playerCharacterSprite = playerCharacterSprite;
		this.playerCharacterImage = playerCharacterImage;
	}
}