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

		aim.targetOffset = aim.bodyRig.data.offset + new Vector3(0, 35, 0);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		aim.bodyRig.data.offset = Vector3.Lerp(aim.bodyRig.data.offset, aim.targetOffset, Time.deltaTime * aim.rotationSpeed);
		aim.UpdateOffsetServerRpc(aim.bodyRig.data.offset);
		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			aim.SwitchState(aim.Hip);
			aim.bodyRig.data.offset = Vector3.zero;
		}
	}
}
