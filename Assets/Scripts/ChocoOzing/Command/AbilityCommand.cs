using Architecture.AbilitySystem;
using ChocoOzing.EventBusSystem;

namespace ChocoOzing.CommandSystem
{
	public interface ICommand
	{
		void Execute();
	}

	//can make any CommandClass using ICommand
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
