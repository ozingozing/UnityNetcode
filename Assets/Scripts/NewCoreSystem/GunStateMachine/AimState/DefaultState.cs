using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultState : PlayerGunActionState
{
	public DefaultState(MyPlayer _player, PlayerStateMachine _playerStateMachine, string _animBoolName) : base(_player, _playerStateMachine, _animBoolName)
	{
	}
}
