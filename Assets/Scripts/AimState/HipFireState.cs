using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChocoOzing;

public class HipFireState : AimBaseState
{
	public override void EnterState(AimStateManager aim)
	{
		aim.IsAiming = false;
		aim.anim.SetBool("Aiming", false);
	}

	public override void ExitState(AimStateManager aim)
	{
		aim.lastAimPos = aim.aimPos;
	}

	public override void UpdateSatate(AimStateManager aim)
	{
		if(Input.GetKeyDown(KeyCode.R) && CanReload(aim))
		{
			aim.SwitchState(aim.Reload);
		}
		if (Input.GetKey(KeyCode.Mouse1))
		{
			aim.SwitchState(aim.Aim);
		}
	}

	public bool CanReload(AimStateManager aim)
	{
		if (aim.WeaponManager.ammo.currentAmmo == aim.WeaponManager.ammo.clipSize) return false;
		else if (aim.WeaponManager.ammo.extraAmmo == 0) return false;
		else return true;
	}

}
