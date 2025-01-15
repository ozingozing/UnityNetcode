using Architecture.AbilitySystem.Controller;
using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
using ChocoOzing.EventBusSystem;
using ChocoOzing.Network;
using ChocoOzing.Utilities;
using QFSW.QC;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ChocoOzing.CoreSystem
{
	public class AbilitySystem : CoreComponent
	{
		[SerializeField] AbilityView view;
		[SerializeField] AbilityData[] startingSOabilities;
		public AbilityController controller;

		ChocoOzing.Network.Vector3Compressor vectorCompressor = new Vector3Compressor(1000f, -1000f);

		private void OnEnable()
		{
			view = GameObject.Find("PlayerStatsUI").GetComponent<AbilityView>();
			controller = new AbilityController.Builder()
				.WithAbilities(startingSOabilities)
				.Build(view);
		}

		public override void OnDestroy()
		{
			if (IsLocalPlayer)
			{
				controller.Clear();
			}
			base.OnDestroy();
		}

		private AbilityData GetTypeData(int idx)
		{
			foreach (AbilityData ability in startingSOabilities)
			{
				if (ability.abilityType == (AbilityType)idx)
					return ability;
				else continue;
			}
			Debug.LogWarning("The same type doesn't exist!!!");
			return null;
		}

		public override void LogicUpdate()
		{
			controller.Update(Time.deltaTime);
		}

		#region AreaOfEffectAction

		AbilityData abilityData;
		[ServerRpc]
		public void AreaOfEffectActionServerRpc(ulong id, int type)
		{
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client))
			{
				GameObject OwnerPlayer = client.PlayerObject.gameObject;
				abilityData = GetTypeData(type);
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

				if(!projectile.IsSpawned)
					projectile.Spawn();
			}
		}

		public void ReturnObject(ulong networkObjectId, Transform pos)
		{
			ParticleActionClientRpc(networkObjectId, (int)abilityData.abilityType, vectorCompressor.PackVector3(pos.position));
		}

		ObjectPool ParticlePool;
		[ClientRpc]
		public void ParticleActionClientRpc(ulong objectId, int type, int Pos)
		{
			Vector3 pos = vectorCompressor.UnpackVector3(Pos).With(y: 2.7f);
			AbilityData abilityData = GetTypeData(type);

			if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObject))
			{
				if (IsServer)
					networkObject.Despawn();
			}

			if (ParticlePool == null)
				ParticlePool = ObjectPool.CreateInstance(abilityData.GetAreaOfEffectData(abilityData.abilityType).particle.GetComponent<PoolableObject>(), 4);
			ParticlePool.GetObject(pos, Quaternion.identity);
		}
		#endregion
	}
}
