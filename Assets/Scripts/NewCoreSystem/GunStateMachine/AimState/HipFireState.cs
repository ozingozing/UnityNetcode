using UnityEngine;

public class HipFireState : PlayerGunActionState
{
	public HipFireState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public override void Enter()
	{
		base.Enter();
		isAiming.Set(false);
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
			playerStateMachine.ChangeState(player.ReloadState);
		}
		if (Input.GetKey(KeyCode.Mouse1))
		{
			playerStateMachine.ChangeState(player.AimState);
		}
	}

	public bool CanReload()
	{
		if (player.WeaponManager.ammo.currentAmmo == player.WeaponManager.ammo.clipSize) return false;
		else if (player.WeaponManager.ammo.extraAmmo == 0) return false;
		else return true;
	}

}
