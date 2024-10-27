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

	private void Awake()
	{
		Instance = this;
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