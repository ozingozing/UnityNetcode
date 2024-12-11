using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using ChocoOzing;
using Cinemachine;
using Unity.Burst.Intrinsics;

public class AimState : PlayerGunActionState
{
	public AimState(MyPlayer _player, PlayerGunStateMachine _gunStateMachine, string _animBoolName) : base(_player, _gunStateMachine, _animBoolName)
	{
	}

	public override void Enter()
	{
		base.Enter();
		xAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.y;
		yAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.x;

		IsAiming = true;
		//anim.SetBool("Aiming", true);
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			gunStateMachine.ChangeState(player.HipFireState);
			//aim.SwitchState(aim.Hip);
		}
	}
}
