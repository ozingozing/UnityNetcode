using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using ChocoOzing.Network;
using ChocoOzing.Utilities;
using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Q : CoreComponent, ISkillAction, IDeleteNetworkObj
{
	public AbilityButton myButton;
	public AbilityData abilityData
	{
		get => AbilityData;
		set => AbilityData = value;
	}
	[SerializeField] private AbilityData AbilityData;

	public bool isHoldAction
	{
		get => IsHoldAction;
		set => IsHoldAction = value;
	}
	[SerializeField] private bool IsHoldAction = false;


	private ChocoOzing.Network.Vector3Compressor vectorCompressor = new Vector3Compressor(1000f, -1000f);

	public void SetAbilityData(AbilityData abilityData, AbilityButton myButton)
	{
		this.myButton = myButton;
		this.abilityData = abilityData;
	}

	public void Action(PlayerAnimationEvent @evnet)
	{
		if (IsLocalPlayer)
		{
			isHoldAction = abilityData.isHoldAction ? true : false;
			AreaOfEffectActionServerRpc(@evnet.clientId);
		}
	}

	NetworkObject debugObj;
	[Command]
	public void DebugSpawn()
	{
		debugObj = NetworkObjectPool.Singleton.GetNetworkObject(
						abilityData.GetAreaOfEffectData(abilityData.abilityType).prefab,
						NetworkManager.gameObject.GetComponent<SpawnPoint>().GetRandomSpawnPoint(),
						Quaternion.identity);
		Unit unit = debugObj.GetComponent<Unit>();
		unit.pathFindingFinishAction += DoFinishingWorkClientRpc;

		//Pathfinding Start
		unit.StartActionTest(gameObject);

		if (!debugObj.IsSpawned)
			debugObj.Spawn();
	}
	[Command]
	public void DebugDelete()
	{
		if (debugObj.IsSpawned)
			debugObj.Despawn();
	}

	[ServerRpc]
	public void AreaOfEffectActionServerRpc(ulong id)
	{
		if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client))
		{
			GameObject OwnerPlayer = client.PlayerObject.gameObject;
			Vector3 PlayerPos = OwnerPlayer.transform.position;
			Vector3 PlayerForward = OwnerPlayer.transform.forward;
			Vector3 StartingPoint = PlayerPos + PlayerForward * 1.5f + abilityData.GetAreaOfEffectData(abilityData.abilityType).start;

			float checkDistance = 2;
			if (Physics.Raycast(PlayerPos, PlayerForward, out RaycastHit hit, checkDistance, LayerMask.GetMask("Wall")))
			{
				StartingPoint = PlayerPos - PlayerForward * 2 + abilityData.GetAreaOfEffectData(abilityData.abilityType).start;
			}

			NetworkObject projectile =
					NetworkObjectPool.Singleton.GetNetworkObject(
						abilityData.GetAreaOfEffectData(abilityData.abilityType).prefab,
						StartingPoint,
						Quaternion.identity
					);

			Unit unit = projectile.GetComponent<Unit>();
			unit.pathFindingFinishAction += DoFinishingWorkClientRpc;

			//Pathfinding Start
			unit.StartAction(OwnerPlayer);

			if (!projectile.IsSpawned)
				projectile.Spawn();
		}
	}

	[ClientRpc]
	public void DoFinishingWorkClientRpc(NetworkObjectReference networkObject)
	{
		if (networkObject.TryGet(out NetworkObject foundObject))
		{
			foundObject.GetComponent<Unit>().ActionCall(DeleteRequestServerRpc, foundObject.NetworkObjectId);
		}
	}

	int cntFinishAction = 0;
	[ServerRpc(RequireOwnership = false)]
	public void DeleteRequestServerRpc(ulong id)
	{
		if (++cntFinishAction == NetworkManager.Singleton.ConnectedClientsList.Count - 1)
		{
			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var obj))
			{
				if (obj.IsSpawned)
				{
					obj.Despawn();
					cntFinishAction = 0;
				}
			}
		}
	}
}
