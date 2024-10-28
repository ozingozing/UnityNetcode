using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using ChocoOzing;
using Cinemachine;

public class AimState : AimBaseState
{
	private Vector3 targetOffset;

	public override void EnterState(AimStateManager aim)
	{
		Debug.Log("Now AimState");
		aim.xAxis = ThirdPersonCamera.Instance.transform.localEulerAngles.y;
		aim.yAxis = ThirdPersonCamera.Instance.transform.localEulerAngles.x;

		aim.IsAiming = true;
		aim.anim.SetBool("Aiming", true);

		aim.UpdateRightHandRigWeightServerRPC(1);
		targetOffset = aim.bodyRig.data.offset + new Vector3(0, 35, 0);
	}

	public override void ExitState(AimStateManager aim)
	{
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		aim.UpdateAdsOffsetServerRpc(targetOffset.y);

		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			aim.UpdateAdsOffsetServerRpc(0);
			aim.UpdateRightHandRigWeightServerRPC(0);

			aim.SwitchState(aim.Hip);
			aim.bodyRig.data.offset = Vector3.zero;
		}
	}
}
