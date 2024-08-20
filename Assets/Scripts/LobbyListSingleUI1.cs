using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI1 : MonoBehaviour
{
    [SerializeField] private TextMeshPro lobbyNameText;
    [SerializeField] private TextMeshPro playersText;
    [SerializeField] private TextMeshPro gameModeText;

    private Lobby lobby;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			LobbyManager1.Instance.JoinLobby(lobby);
		});
	}
}
