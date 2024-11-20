using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerObject
{
    public Vector3 Position;

    public Vector3 Direction;

    public float ChanceToChange;

    public WalkerObject(Vector3 position, Vector3 direction, float chanceToChange)
	{
		Position = position;
		Direction = direction;
		ChanceToChange = chanceToChange;
	}
}
