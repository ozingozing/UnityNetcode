using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChocoOzing;

public class HipFireState : AimBaseState
{
	private Vector3 targetOffset;
	public override void EnterState(AimStateManager aim)
	{
		Debug.Log("Now HipFire");
		aim.IsAiming = false;
		aim.anim.SetBool("Aiming", false);

		aim.headRig.data.offset = aim.headRig.data.offset + new Vector3(0, 35, 0);
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
			aim.headRig.data.offset = Vector3.zero;
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
