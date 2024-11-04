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
		// 3��° ���̾��� �ִϸ��̼� ���¸� �����ɴϴ�.
		AnimatorStateInfo animStateInfo = aim.anim.GetCurrentAnimatorStateInfo(2);

		// "Reloading" �ִϸ��̼� ������ ���� ��Ȳ�� Ȯ���մϴ�.
		if (/*animStateInfo.IsName("Reloading") && */animStateInfo.normalizedTime > 0.9f && !hasSwitchedState)
		{
			aim.SwitchState(aim.Hip); // Reloading�� �������Ƿ� ���� ��ȯ
			hasSwitchedState = true; // ���� ��ȯ�� �� ���� �̷�������� ����
		}
	}
}
