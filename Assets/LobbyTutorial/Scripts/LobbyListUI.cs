using ChocoOzing.EventBusSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI : MonoBehaviour {


    public static LobbyListUI Instance { get; private set; }



    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;


    private void Awake() {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);

        refreshButton.onClick.AddListener(RefreshButtonClick);
        createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
    }

    EventBinding<LobbyEventArgs> eventBinding;
	private void Start() {
        eventBinding = new EventBinding<LobbyEventArgs>(LobbyEvent);
		EventBus<LobbyEventArgs>.Register(eventBinding);
        //EventBus<OnLobbyListChangedEventArgs>.Register(new EventBinding<OnLobbyListChangedEventArgs>(LobbyManager_OnLobbyListChanged));
        
        /*LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;*/
    }

	private void OnDestroy()
	{
		eventBinding.Remove(LobbyEvent);
        EventBus<LobbyEventArgs>.Deregister(eventBinding);
        eventBinding = null;
	}

	private void LobbyEvent(LobbyEventArgs lobby)
	{
        switch(lobby.state)
        {
            case LobbyState.Joined:
				Hide();
				break;

            case LobbyState.Leave:
				Show();
				break;

            case LobbyState.Refresh:
                if(lobby.lobbyList != null)
				    UpdateLobbyList(lobby.lobbyList);
				break;

            default:
                break;
        }
	}

    /*private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }*/


    private void UpdateLobbyList(List<Lobby> lobbyList) {
        foreach (Transform child in container) {
            if (child == lobbySingleTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList) {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
            lobbyListSingleUI.UpdateLobby(lobby);
        }
    }

    private void RefreshButtonClick() {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateLobbyButtonClick() {
        LobbyCreateUI.Instance.Show();
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

    private void Show() {
        gameObject.SetActive(true);
    }

}