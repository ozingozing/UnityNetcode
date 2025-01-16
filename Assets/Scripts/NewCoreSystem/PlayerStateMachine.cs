using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
	public PlayerState CurrentState { get; private set; }

	public void Initialize(PlayerState startingState)
	{
		CurrentState = startingState;
		CurrentState.Enter();
	}

	public void ChangeState(PlayerState newState)
	{
		CurrentState.Exit();
		CurrentState = newState;
		CurrentState.Enter();
	}

	public void ChangeState(PlayerState newState, PlayerAnimationEvent @event)
	{
		CurrentState.Exit();
		CurrentState = newState;
		CurrentState.Enter(@event);
	}
}
