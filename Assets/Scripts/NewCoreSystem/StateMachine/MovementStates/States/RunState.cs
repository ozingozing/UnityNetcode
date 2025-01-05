using UnityEngine;

public class RunState : PlayerGroundedState
{
	public RunState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}
	public override void DoChecks()
	{
		base.DoChecks();
	}
	public override void Enter()
	{
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (Input.GetKeyUp(KeyCode.LeftShift)) playerStateMachine.ChangeState(player.WalkState);
		else if (Movement.CurrentVelocity.magnitude < 0.1f) playerStateMachine.ChangeState(player.IdleState);
	}

	public override void PhysicsUpdate()
	{
		base.PhysicsUpdate();
	}
}
