using ChocoOzing.CoreSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayer : MonoBehaviour
{
	/// <summary>
	/// if you want use Rigidbody? use this
	/// UNT0008 Null propagation on Unity objects
	/// Don't use [?.]
	/// </summary>
	protected Movement Movement
	{ get => movement != null ? movement : Core.GetCoreComponent(ref movement); }
	private Movement movement;

	#region Component
	public Core Core { get; private set; }
	public Animator Anim {  get; private set; }
	public CapsuleCollider MovementCollider { get; private set; }
	#endregion

	#region State Variables
	public PlayerStateMachine StateMachine { get; private set; }
	public IdleState IdleState { get; private set; }
	public WalkState WalkState { get; private set; }
	public RunState RunState { get; private set; }
	#endregion

	private void Awake()
	{
		Core = GetComponentInChildren<Core>();

		StateMachine = new PlayerStateMachine();
		
		IdleState = new IdleState(this, StateMachine, "Idle");
		WalkState = new WalkState(this, StateMachine, "IsWalking");
		RunState = new RunState(this, StateMachine, "IsRunning");
	}
	private void Start()
	{
		Anim = GetComponent<Animator>();
		MovementCollider = GetComponent<CapsuleCollider>();

		StateMachine.Initialize(IdleState);
	}

	private void Update()
	{
		Core.LogicUpdate();
		StateMachine.CurrentState.LogicUpdate();
	}

	private void FixedUpdate()
	{
		StateMachine.CurrentState.PhysicsUpdate();
	}
}
