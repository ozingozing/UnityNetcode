using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InGamePanel : MonoBehaviour
{
    public static InGamePanel Instance { get; private set; }

	[SerializeField] private Transform playerSingleTemplate;
	[SerializeField] private Transform container;

	private void Awake()
	{
		Instance = this;
		playerSingleTemplate.gameObject.SetActive(false);
	}

	private void Start()
	{
		InGameManager.Instance.InGame += LobbyManager_OnGameStarted;
	}

	public void LobbyManager_OnGameStarted(object sender, System.EventArgs e)
	{
		UpdateInGamePanel();
	}

	public void UpdateInGamePanel()
	{
		ClearLobby();

		foreach (var key in InGameManager.Instance.playerDataDictionary.Keys)
		{
			Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
			playerSingleTransform.gameObject.SetActive(true);
			LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

			lobbyPlayerSingleUI.UpdatePlayer(key.ToString());
		}
	}

	private void ClearLobby()
	{
		foreach (Transform child in container)
		{
			if (child == playerSingleTemplate) continue;
			else
			{
				Destroy(child.gameObject);
			}
		}
	}
}
