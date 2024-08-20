using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListUI1 : MonoBehaviour
{
    public static LobbyListUI1 Instance {  get; private set; }

    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createLobbyButton;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);
        refreshButton.onClick.AddListener(RefreshButtonClick);
        createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
    }

	private void Start()
	{
        LobbyManager1.Instance.OnLobbyListChanged += LobbyManager1_OnLobbyListChanged;
        LobbyManager1.Instance.OnJoinedLobby += LobbyManager1_OnJoinedLobby;
	}

	// Update is called once per frame
	void Update()
    {
        
    }

    private void RefreshButtonClick()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateLobbyButtonClick()
    {
        //LobbyCreateUI
    }

    private void LobbyManager1_OnJoinedLobby(object sender, LobbyManager1.LobbyEventArgs e)
    {
        Hide();
    }

    private void LobbyManager1_OnLobbyListChanged(object sender, LobbyManager1.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in container)
        {
            if (child == lobbySingleTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
            lobbySingleTransform.gameObject.SetActive(true);
            //LobbyList
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
