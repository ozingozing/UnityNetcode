using Architecture.AbilitySystem.Controller;
using Architecture.AbilitySystem.View;
using Unity.Netcode;
using UnityEngine;

namespace Architecture.AbilitySystem
{
	public class AbilitySystem : MonoBehaviour
	{
		[SerializeField] AbilityView view;
		[SerializeField] AbilityData[] startingSOabilities;
		AbilityController controller;

		private void OnEnable()
		{
			view = GameObject.Find("PlayerStatsUI").GetComponent<AbilityView>();
			controller = new AbilityController.Builder()
				.WithAbilities(startingSOabilities)
				.Build(view);
		}

		private void Update()
		{
			controller.Update(Time.deltaTime);
		}
	}
}
