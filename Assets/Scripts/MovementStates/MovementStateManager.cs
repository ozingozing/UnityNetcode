using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementStateManager : MonoBehaviour
{
    public float currentMoveSpeed = 3;
    public float walkSpeed = 3, walkBackSpeed = 2;
    public float runSpeed = 7, runBackSpeed = 5;
	
    [HideInInspector] public Animator anim;
    [HideInInspector] public Vector3 dir;
    [HideInInspector] public float inputHorizontal, inputVertical;
    private Rigidbody rb;


	[HideInInspector] public MovementBaseState currentState;
	public IdleState Idle = new IdleState();
	public WalkState Walk = new WalkState();
	public RunState Run = new RunState();


	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
		
		SwitchState(Idle);
	}

	public void PlayerAdsMove()
	{
		anim.SetFloat("InputHorizontal", Input.GetAxis("Horizontal"));
		anim.SetFloat("InputVertical", Input.GetAxis("Vertical"));
		currentState.UpdateState(this);
	}

	public void SwitchState(MovementBaseState state)
	{
		currentState = state;
		currentState.EnterState(this);
	}

	void Move()
	{
		Vector3 moveVelocity = dir.normalized * currentMoveSpeed;
		rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
	}
}
