using ChocoOzing;
using ChocoOzing.CommandSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class ReloadState : PlayerGunActionState
{
	public ICommandTask singleCommand;
	public List<ICommandTask> commands;
	bool canChangeState = false;
	public ReloadState(MyPlayer _player, PlayerGunStateMachine _gunStateMachine, string _animBoolName) : base(_player, _gunStateMachine, _animBoolName)
	{
		singleCommand = PlayerCommand.Create<Reload>(player.GetComponent<IEntity>());

		commands = new List<ICommandTask>
		{
			PlayerCommand.Create<ManyReload>(player.GetComponent<IEntity>()),
			PlayerCommand.Create<ShotgunReloadAction>(player.GetComponent<IEntity>()),
			PlayerCommand.Create<ShotgunSetPos>(player.GetComponent<IEntity>()),
			PlayerCommand.Create<ShotgunPumpAction>(player.GetComponent<IEntity>()),
		};
	}

	public override void ShotGunReloadAction()
	{
		base.ShotGunReloadAction();
		if (player.WeaponManager.ammo.currentAmmo < player.WeaponManager.ammo.clipSize)
			player.WeaponManager.ammo.ShotGunReload();
	}

	public override void MagIn()
	{
		base.MagIn();
		player.WeaponManager.audioSource.PlayOneShot(player.WeaponManager.ammo.magInSound);
	}
	public override void MagOut()
	{
		base.MagOut();
		player.WeaponManager.audioSource.PlayOneShot(player.WeaponManager.ammo.magOutSound);
	}
	public override void ReleaseSlide()
	{
		base.ReleaseSlide();
		player.WeaponManager.audioSource.PlayOneShot(player.WeaponManager.ammo.releaseSlideSound);
	}
	public override void ReloadFinish()
	{
		base.ReloadFinish(); 
		//player.Anim.SetBool("IsReloading", false);
		canChangeState = true;
	}

	void GunTypeReloadAction()
	{
		switch (player.GunType)
		{
			case GunType.M4A1:
				//player.Anim.SetBool("IsReloading", true);
				//player.Anim.SetInteger("GunType", ((int)GunType.M4A1));
				_ = core.ExecuteCommand(new List<ICommandTask> { singleCommand });
				player.WeaponManager.ammo.Reload();
				break;
			case GunType.PumpShotGun:
				//player.Anim.SetBool("IsReloading", true);
				//player.Anim.SetInteger("GunType", ((int)GunType.PumpShotGun));
				_ = core.ExecuteCommand(commands);
				break;
			default:
				break;
		}
	}

	public override void Enter()
	{
		base.Enter();
		canChangeState = false;
		GunTypeReloadAction();
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		// 3번째 레이어의 애니메이션 상태를 가져옵니다.
		AnimatorStateInfo animStateInfo = player.Anim.GetCurrentAnimatorStateInfo(2);

		// "Reloading" 애니메이션 상태의 진행 상황을 확인합니다.
		if (animStateInfo.normalizedTime > 0.9f && canChangeState)
		{
			//gunStateMachine.ChangeState(player.HipFireState);
			//aim.SwitchState(aim.Hip); // Reloading이 끝났으므로 상태 전환
		}
	}
}
