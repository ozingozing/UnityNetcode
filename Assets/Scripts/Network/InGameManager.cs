using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InGameManager : MonoBehaviour
{
	static public InGameManager Instance { get; private set; }

	// Dictionary to store PlayerData with playerId as key
	public Dictionary<string, PlayerData> playerDataDictionary = new Dictionary<string, PlayerData>();

	public event EventHandler InGame;

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			InGamePanel.Instance.gameObject.SetActive(true);
			InGame?.Invoke(this, EventArgs.Empty);
		}
		else if (Input.GetKeyUp(KeyCode.Tab))
		{
			InGamePanel.Instance.gameObject.SetActive(false);
		}
	}
}

[System.Serializable]
public class PlayerData
{
	public string playerName;
	public Sprite playerCharacterSprite;
	public LobbyManager.PlayerCharacter playerCharacterImage;

	public PlayerData(string playerName, Sprite playerCharacterSprite, LobbyManager.PlayerCharacter playerCharacterImage)
	{
		this.playerName = playerName;
		this.playerCharacterSprite = playerCharacterSprite;
		this.playerCharacterImage = playerCharacterImage;
	}
}