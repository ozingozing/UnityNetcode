using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChocoOzing;
using Unity.Burst.Intrinsics;

public class HipFireState : PlayerGunActionState
{
	public HipFireState(MyPlayer _player, PlayerGunStateMachine _gunStateMachine, string _animBoolName) : base(_player, _gunStateMachine, _animBoolName)
	{
	}

	public override void Enter()
	{
		base.Enter();

		IsAiming = false;
		//player.Anim.SetBool("Aiming", false);
	}

	public override void Exit()
	{
		base.Exit();
		lastAimPos = player.aimPos;
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if (Input.GetKeyDown(KeyCode.R) && CanReload())
		{
			gunStateMachine.ChangeState(player.ReloadState);
		}
		if (Input.GetKey(KeyCode.Mouse1))
		{
			gunStateMachine.ChangeState(player.AimState);
		}
	}

	public bool CanReload()
	{
		if (player.WeaponManager.ammo.currentAmmo == player.WeaponManager.ammo.clipSize) return false;
		else if (player.WeaponManager.ammo.extraAmmo == 0) return false;
		else return true;
	}

}
