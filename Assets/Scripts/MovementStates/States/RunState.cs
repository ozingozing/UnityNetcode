using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : PlayerGroundedState
{
	/*public override void EnterState(MovementStateManager movement)
	{
		//movement.anim.SetBool("IsRunning", true);
	}

	public override void ExitState(MovementStateManager movement)
	{
	}

	public override void UpdateState(MovementStateManager movement)
	{
		if (Input.GetKeyUp(KeyCode.LeftShift)) movement.SwitchState(movement.Walk);
		else if (movement.rb.velocity.magnitude < 0.1f) movement.SwitchState(movement.Idle);

		if (movement.inputVertical < 0) movement.currentMoveSpeed = movement.runBackSpeed;
		else movement.currentMoveSpeed = movement.runSpeed;
	}*/
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
		//movement.anim.SetBool("IsRunning", true);
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

		/*if (inputVertical < 0) currentMoveSpeed = runBackSpeed;
		else currentMoveSpeed = runSpeed;*/
	}

	public override void PhysicsUpdate()
	{
		base.PhysicsUpdate();
	}
}
