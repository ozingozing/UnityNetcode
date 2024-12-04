using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : PlayerGroundedState
{
	/*public override void EnterState(MovementStateManager movement)
	{
	}

	public override void ExitState(MovementStateManager movement)
	{
	}

	public override void UpdateState(MovementStateManager movement)
	{
		if (movement.rb.velocity.magnitude > 0.1f)
		{
			movement.SwitchState(Input.GetKey(KeyCode.LeftShift) ? movement.Run : movement.Walk);
		}
	}*/
	public IdleState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
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
		if (Movement.CurrentVelocity.magnitude > 0.1f)
		{
			playerStateMachine.ChangeState(Input.GetKey(KeyCode.LeftShift) ? player.RunState : player.WalkState);
		}
	}

	public override void PhysicsUpdate()
	{
		base.PhysicsUpdate();
	}
}
