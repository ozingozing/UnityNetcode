using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipFireState : AimBaseState
{
	public override void EnterState(AimStateManager aim)
	{
		Debug.Log("HipFire!!!!!!!!");
		aim.IsAiming = false;
		aim.anim.SetBool("Aiming", false);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		if (Input.GetKey(KeyCode.Mouse1)) aim.SwitchState(aim.Aim);
	}

}
