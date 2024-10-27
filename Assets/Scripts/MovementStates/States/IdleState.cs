using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MovementBaseState
{
	public override void EnterState(MovementStateManager movement)
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
	}
}
