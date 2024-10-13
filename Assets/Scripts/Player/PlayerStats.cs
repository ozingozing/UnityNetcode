using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public NetworkVariable<int> kills = new NetworkVariable<int>();
    public NetworkVariable<int> deaths = new NetworkVariable<int>();
    public string playerLobbyId;
    public bool IsDead;

    Transform MeshLOD;
    MeshRenderer M4MeshRender;
    [SerializeField] private Transform M4; 
    
	private void Awake()
	{
        playerLobbyId = AuthenticationService.Instance.PlayerId;
        MeshLOD = transform.GetChild(1);
		M4MeshRender = M4.GetComponent<MeshRenderer>();
	}

	private void Start()
	{
        CombatManager.Instance.Respawn += TurnOnMesh;
	}

	public void AddKill()
    {
        if (NetworkManager.Singleton.IsServer) kills.Value++;
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
