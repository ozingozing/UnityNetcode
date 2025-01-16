using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using ChocoOzing.Network;
using ChocoOzing.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Q : CoreComponent, ISkillAction
{
	public AbilityData abilityData { get => AbilityData; set => AbilityData = value; }
	[SerializeField] private AbilityData AbilityData;
	private ChocoOzing.Network.Vector3Compressor vectorCompressor = new Vector3Compressor(1000f, -1000f);

	public void SetAbilityData(AbilityData abilityData) => this.abilityData = abilityData;

	public void Action(PlayerAnimationEvent @evnet)
	{
		if (IsLocalPlayer)
			AreaOfEffectActionServerRpc(@evnet.clientId, (int)@evnet.abilityData.abilityType);
	}

	[ServerRpc]
	public void AreaOfEffectActionServerRpc(ulong id, int type)
	{
		if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client))
		{
			GameObject OwnerPlayer = client.PlayerObject.gameObject;
			abilityData = Core.GetCoreComponent<AbilitySystem>().GetTypeData(type);
			Vector3 PlayerPos = OwnerPlayer.transform.position;
			Vector3 PlayerForward = OwnerPlayer.transform.forward;
			Vector3 StartingPoint = PlayerPos + PlayerForward * 1.5f + abilityData.GetAreaOfEffectData(abilityData.abilityType).start;
			NetworkObject projectile =
					NetworkObjectPool.Singleton.GetNetworkObject(
						abilityData.GetAreaOfEffectData(abilityData.abilityType).prefab,
						StartingPoint,
						Quaternion.identity
					);

			Unit unit = projectile.GetComponent<Unit>();
			unit.FinishAction += ReturnObject;

			//Pathfinding Start
			unit.StartAction(OwnerPlayer);

			if (!projectile.IsSpawned)
				projectile.Spawn();
		}
	}

	public void ReturnObject(NetworkObject networkObjectId, Transform pos)
	{
		CallClientRpc(networkObjectId.NetworkObjectId);
	}

	[ClientRpc]
	public void CallClientRpc(ulong id)
	{
		if (!IsServer)
			CallServerRpc(id);
	}

	[ServerRpc(RequireOwnership = false)]
	public void CallServerRpc(ulong id)
	{
		if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
		{
			if(obj.IsSpawned)
				obj.Despawn();
		}
	}
}
