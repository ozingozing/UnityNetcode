using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
		{
			if(other.TryGetComponent<ThrowObject>(out ThrowObject item))
				item.RequestDelete();
		}
	}
}
