using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : MovementBaseState
{
	public override void EnterState(MovementStateManager movement)
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
	}
}
