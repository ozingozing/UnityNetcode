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
		player.IsMoveLock.Set(false);
	}

	public override void LogicUpdate()
	{
		base.LogicUpdate();
		if(abilitySystem.controller.cooltimer.IsFinished)
		{
			SetAllStateDefault();
		}
	}

	void SetAllStateDefault()
	{
		player.StateMachine.ChangeState(player.IdleState);
		player.GunStateMachine.ChangeState(player.HipFireState);
	}
}
