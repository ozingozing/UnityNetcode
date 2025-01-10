using ChocoOzing.CoreSystem;
using UnityEngine;

public class PlayerGunActionState : PlayerState
{
	//TODO: Check if the current weapon type requires
	//the HipFire state and set the starting state
	//to either HipFireState or DefaultState accordingly;
	[SerializeField] private float mouseSense = 1;
	public float xAxis, yAxis;

	[HideInInspector] public Vector3 actualAimPos;
	[SerializeField] public float aimSmoothSpeed = 20;
	[SerializeField] public LayerMask aimMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("Default");


	public Transform lastAimPos;

	public PlayerGunActionState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public override void DoChecks()
	{
		base.DoChecks();
	}

	public override void Enter()
	{
		base.Enter();

		xAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.y;
		yAxis = CamManager.Instance.ThirdPersonCam.transform.localEulerAngles.x;
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();

		Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
		Ray ray = Camera.main.ScreenPointToRay(screenCenter);
		xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
		yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
		//yAxis = Mathf.Clamp(yAxis, -80, 80);

		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
		{
			player.aimPos.position = Vector3.Lerp(player.aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
		}

		if (IsAiming)
		{
			player.camFollowPos.localEulerAngles = new Vector3(yAxis, player.camFollowPos.localEulerAngles.y, player.camFollowPos.localEulerAngles.z);
			player.transform.eulerAngles = new Vector3(player.transform.eulerAngles.x, xAxis, player.transform.eulerAngles.z);
		}
	}

	public override void PhysicsUpdate()
	{
		base.PhysicsUpdate();
	}
}
