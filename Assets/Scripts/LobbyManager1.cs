using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager1 : MonoBehaviour
{
	[HideInInspector] public static LobbyManager1 Instance { get; private set; }

	private Lobby hostLobby;
	private Lobby joinedLobby;
	private float heartbeatTimer;
	private float lobbyUpdateTimer;
	private string playerName;

	public event EventHandler<LobbyEventArgs> OnJoinedLobby;
	public event EventHandler<LobbyEventArgs> OnJoinedLobbyUpdate;
	public event EventHandler<LobbyEventArgs> OnKickedFromLobby;
	public event EventHandler<LobbyEventArgs> OnLobbyGameModeChanged;
	public class LobbyEventArgs : EventArgs
	{
		public Lobby lobby;
	}

	public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
	public class OnLobbyListChangedEventArgs : EventArgs
	{
		public List<Lobby> lobbyList;
	}

	public enum GameMode
	{
		CaptureTheFlag,
		Conquest
	}

	public enum PlayerCharacter
	{
		Marine,
		Ninja,
		Zombie
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		HandleLobbyHeartbeat();
		HandleLobbyPollForUpdatesAsync();
	}

	public async void Authenticate(string playerName)
	{
		this.playerName = playerName;
		InitializationOptions initializationOptions = new InitializationOptions();
		initializationOptions.SetProfile(playerName);

		await UnityServices.InitializeAsync(initializationOptions);

		AuthenticationService.Instance.SignedIn += () =>
		{
			Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);

			RefreshLobbyList();
		};
		await AuthenticationService.Instance.SignInAnonymouslyAsync();

		Debug.Log(playerName);
	}

	private async void RefreshLobbyList()
	{
		try
		{
			QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions();
			queryLobbiesOptions.Count = 25;

			queryLobbiesOptions.Filters = new List<QueryFilter>
			{
				new QueryFilter(
						field: QueryFilter.FieldOptions.AvailableSlots, 
						op: QueryFilter.OpOptions.GT, 
						value: "0"
					),
			};

			queryLobbiesOptions.Order = new List<QueryOrder>
			{
				new QueryOrder(
						asc: false,
						field: QueryOrder.FieldOptions.Created
					),
			};

			QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();

			OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results});
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	private async void HandleLobbyHeartbeat()
	{
		if (hostLobby != null)
		{
			heartbeatTimer -= Time.deltaTime;
			if (heartbeatTimer < 0f)
			{
				float heartbeatTimerMax = 15;
				heartbeatTimer = heartbeatTimerMax;

				await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
			}
		}
	}

	private async void HandleLobbyPollForUpdatesAsync()
	{
		if (joinedLobby != null)
		{
			lobbyUpdateTimer -= Time.deltaTime;
			if (lobbyUpdateTimer < 0f)
			{
				float lobbyUpdateTimerMax = 1.1f;
				lobbyUpdateTimer = lobbyUpdateTimerMax;

				Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
				joinedLobby = lobby;
			}
		}
	}

	[Command]
	private async void CreateLobby()
	{
		try
		{
			string lobbyName = "MyLobby";
			int maxPlayers = 4;
			CreateLobbyOptions createLobbyoptions = new CreateLobbyOptions
			{
				//IsPrivate = true,
				IsPrivate = false,
				Player = GetPlayer(),
				Data = new Dictionary<string, DataObject>
				{
					{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") },
					{"Map", new DataObject(DataObject.VisibilityOptions.Public, "de_dust2") }
				}
			};

			Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyoptions);

			hostLobby = lobby;
			joinedLobby = hostLobby;

			Debug.Log("Create Lobby! "
				+ lobby.Name + " "
				+ lobby.MaxPlayers
				+ " " + lobby.Id + " " + lobby.LobbyCode);

			PrintPlayers(hostLobby);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private async void ListLobbies()
	{
		try
		{
			QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
			{
				Count = 25,
				Filters = new List<QueryFilter> {
					new QueryFilter(
						QueryFilter.FieldOptions.AvailableSlots,
						"0",
						QueryFilter.OpOptions.GT
					),
				},
				Order = new List<QueryOrder> {
					new QueryOrder(
						false,
						QueryOrder.FieldOptions.Created
					)
				}
			};
			QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

			Debug.Log("Lobbies found: " + queryResponse.Results.Count);
			foreach (Lobby lobby in queryResponse.Results)
			{
				Debug.Log($"{lobby.Name} : {lobby.MaxPlayers}; Lobby.Data: {lobby.Data["GameMode"].Value}");
			}
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private async void JoinLobbyByCode(string lobbyCode)
	{
		try
		{
			JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
			{
				Player = GetPlayer()
			};

			Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
			joinedLobby = lobby;

			Debug.Log($"Joined Lobby with code!! : {lobbyCode}");

			PrintPlayers(lobby);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}

	}

	[Command]
	private async void QuickJoinLobby()
	{
		try
		{
			await LobbyService.Instance.QuickJoinLobbyAsync();
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	public async void JoinLobby(Lobby lobby)
	{
		Player player = GetPlayer();

		joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(
			lobby.Id,
			new JoinLobbyByIdOptions
			{
				Player = player 
			});

		OnJoinedLobby?.Invoke(this, new LobbyEventArgs { lobby = lobby});
	}

	[Command]
	private void PrintPalyer()
	{
		PrintPlayers(joinedLobby);
	}

	private Player GetPlayer()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
					{
						{"PlayerName",  new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
					}
		};
	}

	private void PrintPlayers(Lobby lobby)
	{
		Debug.Log($"Player in Lobby : {lobby.Name}; Lobby.Data: {lobby.Data["GameMode"].Value}");
		Debug.Log($"Map : {lobby.Data["Map"].Value}");
		foreach (Player player in lobby.Players)
		{
			Debug.Log($"{player.Id} {player.Data["PlayerName"].Value}");
		}
	}

	[Command]
	private async void UpdateLobbyGameMode(string gameMode)
	{
		try
		{
			hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
			{
				Data = new Dictionary<string, DataObject>
				{
					{"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) },
				}
			});

			joinedLobby = hostLobby;

			PrintPlayers(hostLobby);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private async void UpdatePlayerName(string newPlayerName)
	{
		try
		{
			playerName = newPlayerName;
			await LobbyService.Instance.UpdatePlayerAsync(
						joinedLobby.Id,
						AuthenticationService.Instance.PlayerId,
						new UpdatePlayerOptions
						{
							Data = new Dictionary<string, PlayerDataObject>
							{
								{"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
							}
						}
					);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private void LeaveLobby()
	{
		try
		{
			LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private void KickPlayer()
	{
		try
		{
			LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private async void MigrateLobbyHost()
	{
		try
		{
			hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
			{
				HostId = joinedLobby.Players[1].Id,
			});

			joinedLobby = hostLobby;

			PrintPlayers(hostLobby);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}

	[Command]
	private async void DeleteLobby()
	{
		try
		{
			await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);
		}
	}
}
