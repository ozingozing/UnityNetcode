using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : MovementBaseState
{
	public override void EnterState(MovementStateManager movement)
	{
		movement.anim.SetBool("IsRunning", true);
	}

	public override void UpdateState(MovementStateManager movement)
	{
		if (Input.GetKeyUp(KeyCode.LeftShift)) ExitState(movement, movement.Walk);
		else if (movement.dir.magnitude < 0.1f) ExitState(movement, movement.Idle);

		if (movement.inputVertical < 0) movement.currentMoveSpeed = movement.runBackSpeed;
		else movement.currentMoveSpeed = movement.runSpeed;
	}

	void ExitState(MovementStateManager movement, MovementBaseState state)
	{
		movement.anim.SetBool("IsRunning", false);
		movement.SwitchState(state);
	}
}
