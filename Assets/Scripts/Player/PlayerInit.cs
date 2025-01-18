using Architecture.AbilitySystem.Controller;
using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using Invector.vCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerInit : NetworkBehaviour
{
	public NetworkVariable<int> kills = new NetworkVariable<int>();
    public NetworkVariable<int> deaths = new NetworkVariable<int>();
    public NetworkVariable<FixedString128Bytes> Name = new NetworkVariable<FixedString128Bytes>();

	public GameObject Seeker;
	public bool IsDead;
    public TextMeshProUGUI kda;
    public LobbyManager.PlayerCharacter PlayerCharactar;

    [SerializeField] private GameObject[] weaponPrefabs;
    private int currentWeaponIndex = 0;

	public override void OnNetworkSpawn()
	{
		if(IsOwner && IsLocalPlayer)
		{
			//Player Init
			CamManager.Instance.AdsCam.Follow = GetComponent<MyPlayer>().camFollowPos;
			CamManager.Instance.AdsCam.LookAt = GetComponent<MyPlayer>().aimPos;
			CamManager.Instance.ThirdPersonCam.Follow = GetComponent<MyPlayer>().camFollowPos; ;
			CamManager.Instance.ThirdPersonCam.LookAt = GetComponent<MyPlayer>().aimPos;
			GetComponent<MyPlayer>().PlayerActionStart();
		}
		SetWeaponActive(currentWeaponIndex);
		StartCoroutine(WeaponSwape());
		
		if (IsServer)
		{
			kills.Value = 0;
			deaths.Value = 0;

			UpdatePositionClientRpc(NetworkManager.gameObject.GetComponent<SpawnPoint>().GetRandomSpawnPoint());

			GetNameClientRpc();
		}

		base.OnNetworkSpawn();
	}


	// 클라이언트에서 위치를 업데이트하는 RPC
	[ClientRpc]
	public void UpdatePositionClientRpc(Vector3 newPosition)
	{
		if (!gameObject.activeSelf) gameObject.SetActive(!gameObject.activeSelf);
		IsDead = false;
		GetComponent<Rigidbody>().MovePosition(newPosition);
		transform.position = newPosition;
	}


	//자신 이름과 LobbyId를 바로 ServerRpc로 보냄
	[ClientRpc]
    public void GetNameClientRpc()
    {
		ulong playerId = GetComponent<NetworkObject>().OwnerClientId;
		string playerLobbyId = AuthenticationService.Instance.PlayerId;

		if(IsOwner)
			GetNameServerRpc(EditPlayerName.Instance.GetPlayerName(), playerLobbyId);
	}

    //받은 값으로 최신화
	[ServerRpc]
    public void GetNameServerRpc(string Name, string id)
    {
        this.Name.Value = Name;

		List<ulong> targetClientIds = 
			NetworkManager.Singleton.ConnectedClients
			.Where(var => var.Key != NetworkManager.ServerClientId)
			.Select(var => var.Key)
			.ToList();

		SendNameToClientRpc(id);
	}

	private void OnDisable()
	{
		EventBus<PlayerOnSpawnState>.Raise(new PlayerOnSpawnState()
		{
			player = this.gameObject,
			state = ChocoOzing.EventBusSystem.PlayerState.Despawn,
		});
	}

	[ClientRpc]
	public void SendNameToClientRpc(string clientLobbyId)
	{
		PlayerCharactar = InGameManager.Instance.playerDataDictionary.FirstOrDefault(pair => pair.Value.playerLobbyId == clientLobbyId).Value.playerCharacterImage;

		EventBus<PlayerOnSpawnState>.Raise(new PlayerOnSpawnState()
		{
			player = this.gameObject,
			state = ChocoOzing.EventBusSystem.PlayerState.Init,
		});
	}

	IEnumerator WeaponSwape()
	{
		if(!IsOwner) yield break;
		while (true)
		{
			yield return null;
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				EquipWeaponServerRpc(0);
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				EquipWeaponServerRpc(1);
			}
		}
	}

	public void AddKill()
    {
        if (NetworkManager.Singleton.IsServer)
        {
			kills.Value++;
		}
    }

    public void AddDeath()
	{
		if (IsOwner)
		{
			if (IsDead) return;
			IsDead = true;
		}
		
		if (NetworkManager.Singleton.IsServer)
		{
			deaths.Value++;
			RespawnPlayer();
		}
	}

    public void SetWeaponActive(int index)
    {
		if (index >= 0 && index < weaponPrefabs.Length)
		{
			foreach (GameObject item in weaponPrefabs)
			{
				item.SetActive(false);
			}
			currentWeaponIndex = index;
			weaponPrefabs[currentWeaponIndex].SetActive(true);
		}
	}

	[ServerRpc]
	public void TurnOnCurrentWeaponServerRpc() => TurnOnCurrentWeaponClientRpc();

	[ClientRpc]
	void TurnOnCurrentWeaponClientRpc() => weaponPrefabs[currentWeaponIndex].SetActive(true);

	[ServerRpc]
	public void TurnOffCurrentWeaponServerRpc() => TurnOffCurrentWeaponClientRpc();

	[ClientRpc]
	void TurnOffCurrentWeaponClientRpc() => weaponPrefabs[currentWeaponIndex].SetActive(false);

	[ServerRpc]
    public void EquipWeaponServerRpc(int index)
    {
		EquipWeaponClientRpc(index);
	}

    [ClientRpc]
    public void EquipWeaponClientRpc(int index)
    {
        if(index >= 0 && index < weaponPrefabs.Length)
        {
			weaponPrefabs[currentWeaponIndex].SetActive(false);
			currentWeaponIndex = index;
			weaponPrefabs[currentWeaponIndex].SetActive(true);
		}
    }

	// 플레이어가 죽었을 때 리스폰하는 로직 (Respawn 기능)
	public void RespawnPlayer()
	{
		if (IsOwner) IsDead = false;

		// 서버에서 새로운 위치를 할당하고, 플레이어를 리스폰시킴
		Vector3 respawnPosition = NetworkManager.gameObject.GetComponent<SpawnPoint>().GetRandomSpawnPoint();
		UpdatePositionClientRpc(respawnPosition);
	}
}