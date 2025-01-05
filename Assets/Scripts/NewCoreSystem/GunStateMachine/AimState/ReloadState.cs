using ChocoOzing.CommandSystem;
using System.Collections.Generic;

public class ReloadState : PlayerGunActionState
{
	public List<ICommandTask> singleCommand;
	public List<ICommandTask> commands;

	public ReloadState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
		singleCommand = new List<ICommandTask>
		{
			PlayerCommand.Create<Reload>(player.GetComponent<IEntity>()),
		};
		commands = new List<ICommandTask>
		{
			PlayerCommand.Create<ManyReload>(player.GetComponent<IEntity>()),
			PlayerCommand.Create<ShotgunReloadAction>(player.GetComponent<IEntity>()),
			PlayerCommand.Create<ShotgunSetPos>(player.GetComponent<IEntity>()),
			PlayerCommand.Create<ShotgunPumpAction>(player.GetComponent<IEntity>()),
		};
	}

	void GunTypeReloadAction()
	{
		switch (player.GunType)
		{
			case GunType.M4A1:
				_ = core.ExecuteCommand(singleCommand);
				break;
			case GunType.PumpShotGun:
				_ = core.ExecuteCommand(commands);
				break;
			default:
				break;
		}
	}

	public override void Enter()
	{
		base.Enter();
		GunTypeReloadAction();
	}

	public override void Exit()
	{
		base.Exit();
		switch (player.GunType)
		{
			case GunType.M4A1:
				player.AnimationManager.HipFire(0.1f);
				break;
			case GunType.PumpShotGun:
				player.AnimationManager.HipFire(0.3f);
				break;
			default:
				break;
		}
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();

		if (isAnimationFinished)
		{
			playerStateMachine.ChangeState(player.HipFireState);
		}
	}
}
