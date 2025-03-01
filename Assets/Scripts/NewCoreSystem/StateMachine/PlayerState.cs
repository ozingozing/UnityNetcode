using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using System;
using UnityEngine;

public class PlayerState
{
    protected Core core;

    protected MyPlayer player;
    protected PlayerStateMachine playerStateMachine;
    //TODO: Should Make PlayerData
    //protected PlayerData playerData;

    protected bool isAnimationFinished;
    protected bool isExitingState;
	//public bool IsAiming;
    public Observer<bool> isAiming;

	public AbilityData abilityData;
	protected float startTime;
    private string animBoolName;
    /// <summary>
    /// Player BaseMovement
    /// </summary>
    /// <param name="_player"></param>
    /// <param name="_playerStateMachine"></param>
    /// <param name="_animBoolName"></param>
    public PlayerState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName)
    {
        this.player = _player;
        this.playerStateMachine = _playerStateMachine;
        this.animBoolName = _animBoolName;
        core = _player.Core;
		playerStateMachine.StateMachines.Add(this);
	}

	public virtual void Enter(PlayerAnimationEvent @event)
	{
		DoChecks();
		if (animBoolName != "_")
			player.Anim.SetBool(animBoolName, true);
		startTime = Time.time;
		//Debug
		Debug.Log($"PlayerState Enter: {animBoolName}");
		isAnimationFinished = false;
		isExitingState = false;
	}

	public virtual void Enter()
    {
        DoChecks();
        if(animBoolName != "_")
         player.Anim.SetBool(animBoolName, true);
        startTime = Time.time;
        //Debug
        Debug.Log($"PlayerState Enter: {animBoolName}");
        isAnimationFinished = false;
        isExitingState = false;
    }

    public virtual void Exit()
    {
		if (animBoolName != "_")
			player.Anim.SetBool(animBoolName, false);
		//Debug
		Debug.Log($"PlayerState Exit: {animBoolName}");
		isExitingState = true;
        isAnimationFinished = true;
	}

    public virtual void Clear() { }

    public virtual void LogicUpdate() { }

    public virtual void PhysicsUpdate()
    {
        DoChecks();
    }

    public virtual void AnimationTrigger() { }
    public virtual void DoChecks() { }
    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;
}
