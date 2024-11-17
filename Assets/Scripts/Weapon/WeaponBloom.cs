using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBloom : MonoBehaviour
{
    [SerializeField] private float defaultBloomAngle = 0.1f;
    [SerializeField] private float walkBloomMultiplier = 0.5f;
    [SerializeField] private float sprintBloomMultiplier = 1f;
    [SerializeField] private float adsBloomMultiplier = 0.5f;
    float currentBloom;

	private void Awake()
	{
        currentBloom = defaultBloomAngle;
	}

    public Vector3 BloomAngle(Transform barrelPos, MovementStateManager currentState, AimStateManager aimState)
    {
		if (currentState.currentState == currentState.Walk)
        {
            Debug.Log("Now Walk!!");
			currentBloom = defaultBloomAngle * walkBloomMultiplier;
		}
        else if (currentState.currentState == currentState.Run)
        {
            Debug.Log("Now RUN!!!!");
			currentBloom = defaultBloomAngle * sprintBloomMultiplier;
		}
        else if(currentState.currentState == currentState.Idle)
        {
            Debug.Log("Now Idle");
			currentBloom = defaultBloomAngle * adsBloomMultiplier;
		}

        if (aimState.currentState == aimState.Aim)
        {
            Debug.Log("Now ADS");
            currentBloom *= adsBloomMultiplier;
        }
        else Debug.Log("else");
		float randX = Random.Range(-currentBloom, currentBloom);
        float randY = Random.Range(-currentBloom, currentBloom);
        float randZ = Random.Range(-currentBloom, currentBloom);

        Vector3 randomRotation = new Vector3(randX, randY, randZ);

        return barrelPos.localEulerAngles + randomRotation;
    }
}
