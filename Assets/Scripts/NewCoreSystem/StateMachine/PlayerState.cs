using ChocoOzing.CoreSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected Core core;

    protected MyPlayer player;
    protected PlayerStateMachine playerStateMachine;
    protected PlayerGunStateMachine gunStateMachine;
    //TODO: Should Make PlayerData
    //protected PlayerData playerData;

    protected bool isAnimationFinished;
    protected bool isExitingState;

    protected float startTime;
    private string animBoolName;

    public PlayerState(MyPlayer _player, PlayerStateMachine _playerStateMachine, /*AddPlayerData*/ string _animBoolName)
    {
        this.player = _player;
        this.playerStateMachine = _playerStateMachine;
        this.animBoolName = _animBoolName;
        core = _player.Core;
    }

	public PlayerState(MyPlayer _player, PlayerGunStateMachine _gunStateMachine, /*AddPlayerData*/ string _animBoolName)
	{
		this.player = _player;
		this.gunStateMachine = _gunStateMachine;
		this.animBoolName = _animBoolName;
		core = _player.Core;
	}

	public virtual void Enter()
    {
        DoChecks();
        player.Anim.SetBool(animBoolName, true);
        startTime = Time.time;
        //Debug
        Debug.Log($"PlayerState Enter: {animBoolName}");
        isAnimationFinished = false;
        isExitingState = false;
    }

    public virtual void Exit()
    {
        player.Anim.SetBool(animBoolName, false);
		//Debug
		Debug.Log($"PlayerState Exit: {animBoolName}");
		isExitingState = true;
        isAnimationFinished = true;
	}

    public virtual void LogicUpdate() { }

    public virtual void PhysicsUpdate()
    {
        DoChecks();
    }

    public virtual void AnimationTrigger() { }
    public virtual void DoChecks() { }
    public virtual void AnimationFinishTrigger() => isAnimationFinished = true;

    public virtual void ShotGunReloadAction() { }
	public virtual void MagIn() { }
    public virtual void MagOut() { }
	public virtual void ReleaseSlide() { }
	public virtual void ReloadFinish() { }
}
