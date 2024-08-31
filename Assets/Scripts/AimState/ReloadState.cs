using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : AimBaseState
{
	private bool hasSwitchedState = false;

	public override void EnterState(AimStateManager aim)
	{
		aim.anim.SetTrigger("Reload");
		aim.WeaponManager.ammo.Reload();
		aim.UpdateRightHandRigWeightServerRPC(0);
		hasSwitchedState = false;
	}

	public override void ExitState(AimStateManager aim)
	{
		aim.UpdateRightHandRigWeightServerRPC(1);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		// 3번째 레이어의 애니메이션 상태를 가져옵니다.
		AnimatorStateInfo animStateInfo = aim.anim.GetCurrentAnimatorStateInfo(2);

		// "Reloading" 애니메이션 상태의 진행 상황을 확인합니다.
		if (animStateInfo.IsName("Reloading") && animStateInfo.normalizedTime > 0.9f && !hasSwitchedState)
		{
			aim.SwitchState(aim.Hip); // Reloading이 끝났으므로 상태 전환
			hasSwitchedState = true; // 상태 전환이 한 번만 이루어지도록 설정
		}
	}
}
