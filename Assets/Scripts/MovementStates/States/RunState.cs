using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : MovementBaseState
{
	public override void EnterState(MovementStateManager movement)
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
	}
}
