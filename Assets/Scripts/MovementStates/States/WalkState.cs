using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : PlayerGroundedState
{
	/*public override void EnterState(MovementStateManager movement)
	{
		//movement.anim.SetBool("IsWalking", true);
	}

	public override void ExitState(MovementStateManager movement)
	{
		//movement.anim.SetBool("IsWalking", false);
	}

	public override void UpdateState(MovementStateManager movement)
	{
		if (Input.GetKey(KeyCode.LeftShift)) movement.SwitchState(movement.Run);
		else if (movement.rb.velocity.magnitude < 0.1f) movement.SwitchState(movement.Idle);

		if (movement.inputVertical < 0) movement.currentMoveSpeed = movement.walkBackSpeed;
		else movement.currentMoveSpeed = movement.walkSpeed;
	}*/

	public WalkState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public override void DoChecks()
	{
		base.DoChecks();
	}
	public override void Enter()
	{
		base.Enter();
		//movement.anim.SetBool("IsWalking", true);
	}

	public override void Exit()
	{
		base.Exit();
		//movement.anim.SetBool("IsWalking", false);
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (Input.GetKey(KeyCode.LeftShift)) playerStateMachine.ChangeState(player.RunState);
		else if (Movement.CurrentVelocity.magnitude < 0.1f) playerStateMachine.ChangeState(player.IdleState);

		/*if (inputVertical < 0) currentMoveSpeed = walkBackSpeed;
		else currentMoveSpeed = walkSpeed;*/
	}

	public override void PhysicsUpdate()
	{
		base.PhysicsUpdate();
	}
}
