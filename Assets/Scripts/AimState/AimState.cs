using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimState : AimBaseState
{
	public override void EnterState(AimStateManager aim)
	{
		Debug.Log("Aim!!!!!!!!");
		aim.IsAiming = true;
		aim.anim.SetBool("Aiming", true);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		if (Input.GetKeyUp(KeyCode.Mouse1)) aim.SwitchState(aim.Hip);
	}
}
