using ChocoOzing.CoreSystem;

public class PlayerAbilityState : PlayerState
{
	public PlayerAbilityState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
		player.IsMoveLock.Set(false);
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if(core.GetCoreComponent<AbilitySystem>().controller.cooltimer.IsFinished)
		{
			SetAllStateDefault();
		}
	}

	void SetAllStateDefault()
	{
		player.StateMachine.ChangeState(player.IdleState);
		player.GunStateMachine.ChangeState(player.HipFireState);
	}
}
