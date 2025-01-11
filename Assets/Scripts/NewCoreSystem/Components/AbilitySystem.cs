using Architecture.AbilitySystem.Controller;
using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
using ChocoOzing.EventBusSystem;
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

		private void OnEnable()
		{
			view = GameObject.Find("PlayerStatsUI").GetComponent<AbilityView>();
			controller = new AbilityController.Builder()
				.WithAbilities(startingSOabilities)
				.Build(view);
		}

		public override void OnDestroy()
		{
			controller.Clear();
			base.OnDestroy();
		}

		public AbilityData GetTypeData(int idx)
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

		[ServerRpc]
		public void AreaOfEffectActionServerRpc(ulong id, int type)
		{
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client))
			{
				GameObject OwnerPlayer = client.PlayerObject.gameObject;

				AbilityData abilityData = GetTypeData(type);
				Vector3 PlayerPos = OwnerPlayer.transform.position;
				Vector3 PlayerForward = OwnerPlayer.transform.forward;

				NetworkObject projectile = 
						NetworkObjectPool.Singleton.GetNetworkObject(
							abilityData.GetAreaOfEffectData(abilityData.abilityType).prefab,
							PlayerPos + PlayerForward * 1.5f + abilityData.GetAreaOfEffectData(abilityData.abilityType).start,
							Quaternion.identity
						);
				if (!projectile.IsSpawned)
					projectile.Spawn();
				/*GameObject projectile = Instantiate(
						abilityData.GetAreaOfEffectData(abilityData.abilityType).prefab,
						PlayerPos + PlayerForward * 1.5f + abilityData.GetAreaOfEffectData(abilityData.abilityType).start,
						Quaternion.identity);*/
				//projectile.GetComponent<NetworkObject>().Spawn();

				//Pathfinding Start
				projectile.GetComponent<Unit>().StartAction(OwnerPlayer);
			}
		}
	}
}
