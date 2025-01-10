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

	public override void Enter(AbilityData abilityData)
	{
		base.Enter();
		if (abilitySystem != null)
		{
			switch (abilityData.abilityType)
			{
				case AbilityType.Melee:
					break;
				case AbilityType.Ranged:
					break;
				case AbilityType.Projectile:
					break;
				case AbilityType.AreaOfEffect:
					abilitySystem.AreaOfEffectActionServerRpc(abilitySystem.OwnerClientId, (int)abilityData.abilityType);
					break;
				case AbilityType.Buff:
					break;
				case AbilityType.Debuff:
					break;
				default:
					break;
			}
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
