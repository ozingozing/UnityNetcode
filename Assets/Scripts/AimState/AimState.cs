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
		aim.xAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.y;
		aim.yAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.x;

		aim.IsAiming = true;
		aim.anim.SetBool("Aiming", true);
	}

	public override void ExitState(AimStateManager aim)
	{
	}

	public override void UpdateSatate(AimStateManager aim)
	{

		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			aim.SwitchState(aim.Hip);
		}
	}
}
