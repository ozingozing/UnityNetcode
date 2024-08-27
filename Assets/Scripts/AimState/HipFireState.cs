using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HipFireState : AimBaseState
{
	private Vector3 targetOffset;
	public override void EnterState(AimStateManager aim)
	{
		aim.IsAiming = false;
		aim.anim.SetBool("Aiming", false);

		aim.headRig.data.offset = aim.headRig.data.offset + new Vector3(0, 35, 0);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		if (Input.GetKey(KeyCode.Mouse1))
		{
			aim.headRig.data.offset = Vector3.zero;
			aim.SwitchState(aim.Aim);
		}
	}

}
