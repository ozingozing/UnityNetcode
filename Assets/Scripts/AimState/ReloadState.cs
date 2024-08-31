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
		// 3��° ���̾��� �ִϸ��̼� ���¸� �����ɴϴ�.
		AnimatorStateInfo animStateInfo = aim.anim.GetCurrentAnimatorStateInfo(2);

		// "Reloading" �ִϸ��̼� ������ ���� ��Ȳ�� Ȯ���մϴ�.
		if (animStateInfo.IsName("Reloading") && animStateInfo.normalizedTime > 0.9f && !hasSwitchedState)
		{
			aim.SwitchState(aim.Hip); // Reloading�� �������Ƿ� ���� ��ȯ
			hasSwitchedState = true; // ���� ��ȯ�� �� ���� �̷�������� ����
		}
	}
}
