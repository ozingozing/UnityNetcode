using Architecture.AbilitySystem;
using ChocoOzing.EventBusSystem;
using System.Threading.Tasks;
using UnityEngine;
using static System.Activator;

namespace ChocoOzing.CommandSystem
{
	public interface ICommand
	{
		void Execute();
	}

	public interface ICommandTask
	{
		Task Execute();
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

	public abstract class PlayerCommand : ICommandTask
	{
		protected readonly IEntity player;

		protected PlayerCommand(IEntity player)
		{
			this.player = player;
		}

		public abstract Task Execute();

		public static T Create<T>(IEntity player) where T : PlayerCommand
		{
			return (T)CreateInstance(typeof(T), player);
		}
	}

	public class Reload : PlayerCommand
	{
		public Reload(IEntity player) : base(player) { }

		public override async Task Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.Reload(); // �ִϸ��̼� �ð�

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); // �� �����Ӹ��� ���
			}
		}
	}

	public class ManyReload : PlayerCommand
	{
		public ManyReload(IEntity player) : base(player) { }

		public override async Task Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ManyReload(); // �ִϸ��̼� �ð�

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); // �� �����Ӹ��� ���
			}
		}
	}

	public class ShotgunReloadAction : PlayerCommand
	{
		public ShotgunReloadAction(IEntity player) : base(player) { }

		public override async Task Execute()
		{
			float startTime;
			float animationTime;

			while (player.Player.WeaponManager.ammo.currentAmmo < player.Player.WeaponManager.ammo.clipSize)
			{
				animationTime = player.AnimationManager.ShotgunReloadAction(); // �ִϸ��̼� �ð�
				startTime = Time.time;
				while (Time.time - startTime < animationTime)
					await Task.Yield(); // �� �����Ӹ��� ���
			}
		}
	}

	public class ShotgunSetPos : PlayerCommand
	{
		public ShotgunSetPos(IEntity player) : base(player) { }

		public override async Task Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ShotgunSetPos(); // �ִϸ��̼� �ð�

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); // �� �����Ӹ��� ���
			}
		}
	}

	public class ShotgunPumpAction : PlayerCommand
	{
		public ShotgunPumpAction(IEntity player) : base(player) { }

		public override async Task Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ShotgunPumpAction(); // �ִϸ��̼� �ð�

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); // �� �����Ӹ��� ���
			}
		}
	}

	public class HipFireAction : PlayerCommand
	{
		public HipFireAction(IEntity player) : base(player) { }

		public override async Task Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.HipFire(); // �ִϸ��̼� �ð�

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); // �� �����Ӹ��� ���
			}
		}
	}
}
