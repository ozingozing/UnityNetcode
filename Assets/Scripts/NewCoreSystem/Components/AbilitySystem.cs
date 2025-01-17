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
	public interface ISkillAction
	{
		public AbilityData abilityData { get; }
		public void SetAbilityData(AbilityData abilityData);
		public void Action(PlayerAnimationEvent @evnet);
	}

	public class AbilitySystem : CoreComponent
	{
		[SerializeField]private AbilityView view;
		[SerializeField]private AbilityData[] SO_StartingAbilities;
		public AbilityController controller;

		private List<ISkillAction> skills = new List<ISkillAction>();

		public override void OnNetworkSpawn()
		{
			if (IsLocalPlayer)
			{
				view = GameObject.Find("PlayerStatsUI").GetComponent<AbilityView>();
				controller = new AbilityController.Builder()
					.WithAbilities(SO_StartingAbilities, OwnerClientId)
					.Build(view);


				int idx = 0;
				foreach (var item in transform.GetComponentsInChildren<ISkillAction>())
				{
					item.SetAbilityData(SO_StartingAbilities[idx]);
					skills.Add(item);
					idx++;
				}
			}
			base.OnNetworkSpawn();
		}


		public override void OnDestroy()
		{
			if (IsLocalPlayer)
			{
				controller.Clear();
			}
			base.OnDestroy();
		}


		public override void LogicUpdate()
		{
			if(IsLocalPlayer && controller != null)
				controller.Update(Time.deltaTime);
		}

		public AbilityData GetTypeData(int idx)
		{
			foreach (AbilityData ability in SO_StartingAbilities)
			{
				if (ability.abilityType == (AbilityType)idx)
					return ability;
				else continue;
			}
			Debug.LogWarning("The same type doesn't exist!!!");
			return null;
		}

		#region AreaOfEffectAction
		public void SkillAction(PlayerAnimationEvent @event)
		{
			foreach (var item in skills)
			{
				if (@event.abilityData.abilityType == item.abilityData.abilityType)
					item.Action(@event);
			}
		}
		#endregion
	}
}
