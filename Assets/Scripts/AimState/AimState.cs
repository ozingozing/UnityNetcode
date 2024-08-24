using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimState : AimBaseState
{
	private Vector3 targetOffset;
	private float rotationSpeed = 10;

	public override void EnterState(AimStateManager aim)
	{
		Debug.Log("Aim!!!!!!!!");
		aim.IsAiming = true;
		aim.anim.SetBool("Aiming", true);

		aim.UpdateRightHandRigWeightServerRPC(1);
		targetOffset = aim.bodyRig.data.offset + new Vector3(0, 35, 0);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		aim.UpdateAdsOffsetServerRpc(Vector3.Lerp(aim.bodyRig.data.offset, targetOffset, Time.deltaTime * rotationSpeed));
		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			aim.UpdateRightHandRigWeightServerRPC(0);
			aim.SwitchState(aim.Hip);
			aim.bodyRig.data.offset = Vector3.zero;
		}
	}
}
