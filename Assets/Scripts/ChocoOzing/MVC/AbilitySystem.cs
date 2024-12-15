using Architecture.AbilitySystem.Controller;
using Architecture.AbilitySystem.View;
using Unity.Netcode;
using UnityEngine;

namespace Architecture.AbilitySystem
{
	public class AbilitySystem : MonoBehaviour
	{
		[SerializeField] AbilityView view;
		[SerializeField] AbilityData[] startingAbilities;
		AbilityController controller;

		private void Awake()
		{
			controller = new AbilityController.Builder()
				.WithAbilities(startingAbilities)
				.Build(view);
		}

		private void Update()
		{
			controller.Update(Time.deltaTime);
		}
	}
}
