using Architecture.AbilitySystem.Controller;
using Architecture.AbilitySystem.Model;
using Architecture.AbilitySystem.View;
using ChocoOzing.EventBusSystem;
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
				.WithAbilities(startingSOabilities, Core.Root.GetComponent<MyPlayer>())
				.Build(view);
		}

		public override void LogicUpdate()
		{
			controller.Update(Time.deltaTime);
		}
	}
}
