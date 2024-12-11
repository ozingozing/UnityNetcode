using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultState : PlayerGunActionState
{
	public DefaultState(MyPlayer _player, PlayerGunStateMachine _gunStateMachine, string _animBoolName) : base(_player, _gunStateMachine, _animBoolName)
	{
	}
}
