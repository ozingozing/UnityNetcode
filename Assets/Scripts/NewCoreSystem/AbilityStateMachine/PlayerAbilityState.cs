using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using Unity.Netcode;
using UnityEngine;

public class PlayerAbilityState : PlayerState
{
	AbilitySystem abilitySystem;
	public PlayerAbilityState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
		abilitySystem = core.GetCoreComponent<AbilitySystem>();
	}

	public override void Enter(PlayerAnimationEvent @evnet)
	{
		base.Enter();
		if (abilitySystem != null)
		{
			abilitySystem.SkillAction(@evnet);
		}
		else
			Debug.LogWarning("Ability Sysyem is Null!!!");
	}

	public override void Exit()
	{
		base.Exit();
		player.Movement.IsMoveLock.Set(false);
	}

	public override void LogicUpdate()
	{
		if (abilitySystem.controller.cooltimer.IsFinished && !abilitySystem.currentAction.isHoldAction)
		{
			SetAllStateDefault();
		}
	}

	void SetAllStateDefault()
	{
		player.StateMachine.ChangeState(player.IdleState);
		player.GunStateMachine.ChangeState(player.HipFireState);
		player.AnimationManager.HipFire(0.1f);
	}
}
