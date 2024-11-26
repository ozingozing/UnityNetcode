using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
	public static Action<GameObject> OnPlayerSpawn;
	public static Action<GameObject> OnPlayerDespawn;

	public NetworkVariable<int> kills = new NetworkVariable<int>();
    public NetworkVariable<int> deaths = new NetworkVariable<int>();
    public NetworkVariable<FixedString128Bytes> Name = new NetworkVariable<FixedString128Bytes>();
    
    public bool IsDead;
    public TextMeshProUGUI kda;
    public LobbyManager.PlayerCharacter PlayerCharactar;

    [SerializeField] private GameObject[] weaponPrefabs;
    private int currentWeaponIndex = 0;

	public override void OnNetworkSpawn()
	{
		OnPlayerSpawn?.Invoke(this.gameObject);
		//DefaultWeaponSet
		SetWeaponActive(currentWeaponIndex);
		//DefaultSet

		//���� �ƴϸ� �ĳ���
		if (!IsServer) return;
        kills.Value = 0;
        deaths.Value = 0;

		/*PosSet���� ������ �ȵ�������
		 *���⼭ 1�� �� �۾�*/
		// Ŭ���̾�Ʈ�� ��ġ ����ȭ ��û
		UpdatePositionClientRpc(NetworkManager.gameObject.GetComponent<SpawnPoint>().GetRandomSpawnPoint());

		//NetworkObjectId�� �ִ� Ŭ�����׸� ����
		GetNameClientRpc(
            new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[] {GetComponent<NetworkObject>().OwnerClientId},
                }
            }    
        );

		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		OnPlayerDespawn?.Invoke(this.gameObject);
	}

	// Ŭ���̾�Ʈ���� ��ġ�� ������Ʈ�ϴ� RPC
	[ClientRpc]
	public void UpdatePositionClientRpc(Vector3 newPosition)
	{
		/*GetComponent<Rigidbody>().useGravity = false;
		GetComponent<CapsuleCollider>().enabled = false;*/

		// Ŭ���̾�Ʈ���� ��ġ�� ����
		transform.position = newPosition;
		GetComponent<Rigidbody>().MovePosition(newPosition);

		/*GetComponent<Rigidbody>().useGravity = true;
		GetComponent<CapsuleCollider>().enabled = true;*/
	}


	//�ڽ� �̸��� LobbyId�� �ٷ� ServerRpc�� ����
	[ClientRpc]
    public void GetNameClientRpc(ClientRpcParams clientRpcParams = default)
    {
		ulong playerId = GetComponent<NetworkObject>().OwnerClientId;
		PlayerCharactar = InGameManager.Instance.playerDataDictionary.FirstOrDefault(pair => pair.Value.playerLobbyId == AuthenticationService.Instance.PlayerId).Value.playerCharacterImage;

        GetNameServerRpc(EditPlayerName.Instance.GetPlayerName(), AuthenticationService.Instance.PlayerId);
	}

    //���� ������ �ֽ�ȭ
	[ServerRpc]
    public void GetNameServerRpc(string Name, string id)
    {
        this.Name.Value = Name;

		ulong playerId = GetComponent<NetworkObject>().OwnerClientId;
        PlayerCharactar = InGameManager.Instance.playerDataDictionary.FirstOrDefault(pair => pair.Value.playerLobbyId == id).Value.playerCharacterImage;
	}

	private void Start()
	{
        CombatManager.Instance.Respawn += TurnOnMesh;
	}

	public override void OnDestroy()
	{
		CombatManager.Instance.Respawn -= TurnOnMesh;
		base.OnDestroy();
	}

	private void Update()
	{
        if (!IsOwner) return;

		if (Input.GetKeyDown(KeyCode.Alpha1))
        {
			EquipWeaponServerRpc(0);
		}
        else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			EquipWeaponServerRpc(1);
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
        if (NetworkManager.Singleton.IsServer)
        {
			deaths.Value++;
		}
	}

    [ClientRpc]
    public void TurnOffMeshClientRpc()
    {
        gameObject.SetActive(false);
	}

	public void TurnOnMesh(object sender, System.EventArgs e)
    {
        //TurnOnMeshServerRpc();
        TurnOnMeshClientRpc();
	}

    [ServerRpc]
	public void TurnOnMeshServerRpc()
	{
        TurnOnMeshClientRpc();
	}

    [ClientRpc]
	public void TurnOnMeshClientRpc()
    {
		/*MeshLOD.gameObject.SetActive(true);
		M4MeshRender.enabled = true;*/
		gameObject.SetActive(true);
		IsDead = false;
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

	// �÷��̾ �׾��� �� �������ϴ� ���� (Respawn ���)
	public void RespawnPlayer()
	{
		IsDead = true;
		if (IsServer)
		{
			// �������� ���ο� ��ġ�� �Ҵ��ϰ�, �÷��̾ ��������Ŵ
			Vector3 respawnPosition = NetworkManager.gameObject.GetComponent<SpawnPoint>().GetRandomSpawnPoint();
			UpdatePositionClientRpc(respawnPosition);
		}
	}
}
