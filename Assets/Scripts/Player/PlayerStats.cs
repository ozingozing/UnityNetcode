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
    public NetworkVariable<int> kills = new NetworkVariable<int>();
    public NetworkVariable<int> deaths = new NetworkVariable<int>();
    public NetworkVariable<FixedString128Bytes> Name = new NetworkVariable<FixedString128Bytes>();
    
    public bool IsDead;
    public TextMeshProUGUI kda;
    public LobbyManager.PlayerCharacter PlayerCharactar;

    Transform MeshLOD;
    MeshRenderer M4MeshRender;
    [SerializeField] private Transform M4;

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
        if(!IsServer) return;
        kills.Value = 0;
        deaths.Value = 0;

		GetNameClientRpc(
            new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[] {GetComponent<NetworkObject>().OwnerClientId},
                }
            }    
        );
	}
    [ClientRpc]
    public void GetNameClientRpc(ClientRpcParams clientRpcParams = default)
    {
		ulong playerId = GetComponent<NetworkObject>().OwnerClientId;
		PlayerCharactar = InGameManager.Instance.playerDataDictionary.FirstOrDefault(pair => pair.Value.playerLobbyId == AuthenticationService.Instance.PlayerId).Value.playerCharacterImage;

        GetNameServerRpc(EditPlayerName.Instance.GetPlayerName(), AuthenticationService.Instance.PlayerId);
	}

	[ServerRpc]
    public void GetNameServerRpc(string Name, string id)
    {
        this.Name.Value = Name;

		ulong playerId = GetComponent<NetworkObject>().OwnerClientId;
        PlayerCharactar = InGameManager.Instance.playerDataDictionary.FirstOrDefault(pair => pair.Value.playerLobbyId == id).Value.playerCharacterImage;
	}

	private void Awake()
	{
		MeshLOD = transform.GetChild(1);
		M4MeshRender = M4.GetComponent<MeshRenderer>();
	}

	private void Start()
	{
        CombatManager.Instance.Respawn += TurnOnMesh;
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
		MeshLOD.gameObject.SetActive(false);
		M4MeshRender.enabled = false;
	}

    public void TurnOnMesh(object sender, System.EventArgs e)
    {
        //TurnOnMeshServerRpc();
        TurnOnMeshClientRpc();
	}

    [ServerRpc]
	public void TurnOnMeshServerRpc()
	{
        transform.position = new Vector3(30, 1, 10);
        TurnOnMeshClientRpc();
	}

    [ClientRpc]
	public void TurnOnMeshClientRpc()
    {
		MeshLOD.gameObject.SetActive(true);
		M4MeshRender.enabled = true;

		IsDead = false;
    }
}
