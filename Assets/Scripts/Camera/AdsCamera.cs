using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsCamera : MonoBehaviour
{
	public static AdsCamera Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
