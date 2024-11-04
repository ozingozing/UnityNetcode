using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : AimBaseState
{
	private bool hasSwitchedState = false;

	void GunTypeReloadAction(AimStateManager aim)
	{
		switch (aim.GunType)
		{
			case GunType.M4A1:
				//aim.anim.SetTrigger("Reload");
				aim.anim.SetBool("GunReload", true);
				aim.anim.SetInteger("GunType", ((int)GunType.M4A1));
				aim.WeaponManager.ammo.Reload();
				aim.UpdateRigWeightServerRPC(0);
				break;
			case GunType.PumpShotGun:
				aim.anim.SetBool("GunReload", true);
				aim.anim.SetInteger("GunType", ((int)GunType.PumpShotGun)); 
				break;
			default:
				break;
		}
	}

	public override void EnterState(AimStateManager aim)
	{
		/*aim.anim.SetTrigger("Reload");
		aim.anim.SetBool("GunReload", true);
		aim.anim.SetInteger("GunType", 1);
		aim.WeaponManager.ammo.Reload();*/
		Debug.Log(aim.WeaponManager.gameObject.name);
		GunTypeReloadAction(aim);
		hasSwitchedState = false;
	}


	public override void ExitState(AimStateManager aim)
	{
		aim.UpdateRigWeightServerRPC(1);
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		// 3번째 레이어의 애니메이션 상태를 가져옵니다.
		AnimatorStateInfo animStateInfo = aim.anim.GetCurrentAnimatorStateInfo(2);

		// "Reloading" 애니메이션 상태의 진행 상황을 확인합니다.
		if (/*animStateInfo.IsName("Reloading") && */animStateInfo.normalizedTime > 0.9f && !hasSwitchedState)
		{
			aim.SwitchState(aim.Hip); // Reloading이 끝났으므로 상태 전환
			hasSwitchedState = true; // 상태 전환이 한 번만 이루어지도록 설정
		}
	}
}
