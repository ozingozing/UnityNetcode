using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;

public class PlayerAbilityState : PlayerState
{
	float crossFadeDuration = 0.1f;

	public PlayerAbilityState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public void SkillAction(PlayerAnimationEvent @event)
	{
		player.IsMove.Set(@event.MoveLock);
		player.Anim.CrossFade(@event.animationHash, crossFadeDuration);
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
		player.IsMove.Set(false);
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if(core.GetCoreComponent<AbilitySystem>().controller.cooltimer.IsFinished)
		{
			playerStateMachine.ChangeState(player.IdleState);
		}
	}
}
