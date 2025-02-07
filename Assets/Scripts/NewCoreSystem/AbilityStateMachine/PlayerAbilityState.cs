using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerAbilityState : PlayerState
{
	AbilitySystem abilitySystem;
	PlayerInit playerInit;
	public float setDefualtDuration = 0.1f;

	public PlayerAbilityState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
		abilitySystem = core.GetCoreComponent<AbilitySystem>();
		playerInit = player.GetComponent<PlayerInit>();
	}

	public override void Enter(PlayerAnimationEvent @evnet)
	{
		base.Enter();
		playerInit.TurnOffCurrentWeaponServerRpc();
		if (abilitySystem != null)
		{
			setDefualtDuration = @evnet.abilityData.exitDuration;
			abilitySystem.SkillAction(@evnet);
		}
		else
			Debug.LogWarning("Ability Sysyem is Null!!!");
	}

	public override void Exit()
	{
		base.Exit();
		player.Movement.IsMoveLock.Set(false);
		playerInit.TurnOnCurrentWeaponServerRpc();
	}

	public override void LogicUpdate()
	{
		if (abilitySystem.controller.cooltimer.IsFinished && !abilitySystem.currentAction.isHoldAction)
		{
			SetAllStateDefault(setDefualtDuration);
		}
	}

	void SetAllStateDefault(float duration = 0.25f)
	{
		player.StateMachine.ChangeState(player.IdleState);
		player.AnimationManager.HipFire(duration);
	}
}
