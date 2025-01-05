using UnityEngine;

public class AimState : PlayerGunActionState
{
	public AimState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public override void Enter()
	{
		base.Enter();
		xAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.y;
		yAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.x;

		IsAiming = true;
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
			playerStateMachine.ChangeState(player.HipFireState);
		}
	}
}
