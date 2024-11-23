using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
	private void Start()
	{
		if(NetworkManager.Singleton.IsServer)
		{
			GameObject.Find("Quantum Console (SRP)").SetActive(false);
		}
	}
}
