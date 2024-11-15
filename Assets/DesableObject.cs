using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesableObject : MonoBehaviour
{
	[SerializeField] private float WaitTime;
	private void OnEnable()
	{
		StartCoroutine(DesableWaitForSeconds(WaitTime));
	}

	IEnumerator DesableWaitForSeconds(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		GetComponent<MeshRenderer>().enabled = false;
	}
}
