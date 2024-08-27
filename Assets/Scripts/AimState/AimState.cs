using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class AimState : AimBaseState
{
	private Vector3 targetOffset;
	private float rotationSpeed = 10;

	public override void EnterState(AimStateManager aim)
	{
		aim.IsAiming = true;
		aim.anim.SetBool("Aiming", true);

		aim.ChangeRightHandRigWeight(1);
		aim.UpdateRightHandRigWeightServerRPC(1);
		targetOffset = aim.bodyRig.data.offset + new Vector3(0, 35, 0);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		aim.bodyRig.data.offset = Vector3.Lerp(aim.bodyRig.data.offset, targetOffset, Time.deltaTime * rotationSpeed);
		aim.UpdateAdsOffsetServerRpc(aim.bodyRig.data.offset);
		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			aim.ChangeRightHandRigWeight(0);
			aim.UpdateRightHandRigWeightServerRPC(0);

			aim.SwitchState(aim.Hip);
			aim.bodyRig.data.offset = Vector3.zero;
		}
	}
}
