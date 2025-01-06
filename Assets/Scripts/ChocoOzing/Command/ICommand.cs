using Architecture.AbilitySystem;
using Architecture.AbilitySystem.Model;
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
		Task<bool> Execute();
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
				MoveLock = data.moveLock,
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

		public abstract Task<bool> Execute();

		public static T Create<T>(IEntity player) where T : PlayerCommand
		{
			return (T)CreateInstance(typeof(T), player);
		}
	}

	public class Reload : PlayerCommand
	{
		public Reload(IEntity player) : base(player) { }

		public override async Task<bool> Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.Reload();
			player.Player.WeaponManager.ammo.Reload();

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield();
			}

			return true;
		}
	}

	public class ManyReload : PlayerCommand
	{
		public ManyReload(IEntity player) : base(player) { }

		public override async Task<bool> Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ManyReload();

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield();
			}

			return true;
		}
	}

	public class ShotgunReloadAction : PlayerCommand
	{
		public ShotgunReloadAction(IEntity player) : base(player) { }

		public override async Task<bool> Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ShotgunReloadAction();
			player.Player.WeaponManager.ammo.ShotGunReload();
			
			while (Time.time - startTime < animationTime)
				await Task.Yield(); 

			return !(player.Player.WeaponManager.ammo.currentAmmo < player.Player.WeaponManager.ammo.clipSize);
		}
	}

	public class ShotgunSetPos : PlayerCommand
	{
		public ShotgunSetPos(IEntity player) : base(player) { }

		public override async Task<bool> Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ShotgunSetPos();

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield();
			}

			return true;
		}
	}

	public class ShotgunPumpAction : PlayerCommand
	{
		public ShotgunPumpAction(IEntity player) : base(player) { }

		public override async Task<bool> Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.ShotgunPumpAction();

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); 
			}
			return true;
		}
	}

	public class HipFireAction : PlayerCommand
	{
		public HipFireAction(IEntity player) : base(player) { }

		public override async Task<bool> Execute()
		{
			float startTime = Time.time;
			float animationTime = player.AnimationManager.HipFire(); 

			while (Time.time - startTime < animationTime)
			{
				await Task.Yield(); 
			}

			return true;
		}
	}
}
