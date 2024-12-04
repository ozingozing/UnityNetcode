using ChocoOzing.CoreSystem;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
	/*public abstract void EnterState(MovementStateManager movement);

    public abstract void UpdateState(MovementStateManager movement);
    
    public abstract void ExitState(MovementStateManager movement);*/
	/// <summary>
	/// if you want use Rigidbody? use this
	/// UNT0008 Null propagation on Unity objects
	/// Don't use [?.]
	/// </summary>
	protected Movement Movement
	{ get => movement != null ? movement : core.GetCoreComponent(ref movement); }
	private Movement movement;

	public float currentMoveSpeed = 3;
	public float walkSpeed = 3, walkBackSpeed = 2;
	public float runSpeed = 7, runBackSpeed = 5;

	[HideInInspector] public Vector3 dir;
	[HideInInspector] public float inputHorizontal, inputVertical;

	public PlayerGroundedState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}

	public override void DoChecks()
	{
		base.DoChecks();
	}

	public override void Enter()
	{
		base.Enter();
	}

	public override void Exit()
	{ 
		base.Exit();
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
	}

	public override void PhysicsUpdate()
	{
		base.PhysicsUpdate();
	}
}
