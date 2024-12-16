using ChocoOzing.EventBusSystem;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

namespace Architecture.AbilitySystem.Controller
{
	public interface ICommand
	{
		void Execute();
	}
	public class AbilityCommand : ICommand
	{
		private readonly AbilityData data;
		public float duration => data.duration;

		public AbilityCommand(AbilityData data)
		{
			this.data = data;
		}

		public void Execute()
		{
			EventBus<PlayerAnimationEvent>.Raise(new PlayerAnimationEvent
			{
				animationHash = data.animationHash,
			});
		}
	}
}
