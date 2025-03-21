using Architecture.AbilitySystem.Model;
using ChocoOzing.EventBusSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using static ChocoOzing.EventBusSystem.LobbyEventArgs;
using static LobbyManager;

public class LobbyUI : MonoBehaviour {


    public static LobbyUI Instance { get; private set; }


    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private Button changeMarineButton;
    [SerializeField] private Button changeNinjaButton;
    [SerializeField] private Button changeZombieButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button changeGameModeButton;
    [SerializeField] private Button startGameButton;

    private void Awake() {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        changeMarineButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Marine);
        });
        changeNinjaButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Ninja);
        });
        changeZombieButton.onClick.AddListener(() => {
            LobbyManager.Instance.UpdatePlayerCharacter(LobbyManager.PlayerCharacter.Zombie);
        });

        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });

        changeGameModeButton.onClick.AddListener(() => {
            LobbyManager.Instance.ChangeGameMode();
        });

        startGameButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.StartGame();
        });
    }

    EventBinding<LobbyEventArgs> eventBinding;
	private void Start() {
        eventBinding = new EventBinding<LobbyEventArgs>(LobbyEvent);
		EventBus<LobbyEventArgs>.Register(eventBinding);

		/*LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;

        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;

        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;*/

		Hide();
    }

	private void OnDestroy()
	{
		eventBinding.Remove(LobbyEvent);
		EventBus<LobbyEventArgs>.Deregister(eventBinding);
        eventBinding = null;
    }

	private void LobbyEvent(LobbyEventArgs e)
	{
		switch (e.state)
		{
			case LobbyState.Joined:
				if(e.lobby != null)
                    UpdateLobby(e.lobby);
				break;

			case LobbyState.Leave:
				ClearLobby();
				Hide();
				break;

			case LobbyState.StartGame:
				Hide();
				break;

			default:
				break;
		}
	}
	
    /*private void UpdateLobby() {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void LobbyManager_OnGameStarted(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLeftLobby(object sender, System.EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }*/


    private void UpdateLobby(Lobby lobby) {
        ClearLobby();

		foreach (Player player in lobby.Players) {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
            );

			lobbyPlayerSingleUI.UpdatePlayer(player);
        }

		changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameModeText.text = lobby.Data[LobbyManager.KEY_GAME_MODE].Value;

        Show();
    }

    private void ClearLobby() {
        foreach (Transform child in container) {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}