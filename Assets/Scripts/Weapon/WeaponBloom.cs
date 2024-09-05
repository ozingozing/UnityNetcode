using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBloom : MonoBehaviour
{
    [SerializeField] private float defaultBloomAngle = 3;
    [SerializeField] private float walkBloomMultiplier = 500f;
    [SerializeField] private float sprintBloomMultiplier = 1000f;
    [SerializeField] private float adsBloomMultiplier = 0.5f;

    MovementStateManager moveStateManager;
    AimStateManager aimStateManager;

    float currentBloom;

	private void Awake()
	{
        currentBloom = defaultBloomAngle;
		moveStateManager = GetComponentInParent<MovementStateManager>();
        aimStateManager = GetComponentInParent<AimStateManager>();
	}

    public Vector3 BloomAngle(Transform barrelPos)
    {
		if (moveStateManager.currentState == moveStateManager.Walk) currentBloom = defaultBloomAngle * walkBloomMultiplier;
        else if (moveStateManager.currentState == moveStateManager.Run) currentBloom = defaultBloomAngle * sprintBloomMultiplier;
        else if(moveStateManager.currentState == moveStateManager.Idle) currentBloom = defaultBloomAngle * adsBloomMultiplier;

		if (aimStateManager.currentState == aimStateManager.Aim) currentBloom  *= adsBloomMultiplier;

        float randX = Random.Range(-currentBloom, currentBloom);
        float randY = Random.Range(-currentBloom, currentBloom);
        float randZ = Random.Range(-currentBloom, currentBloom);

        Vector3 randomRotation = new Vector3(randX, randY, randZ);

        return barrelPos.localEulerAngles + randomRotation;
    }
}
