using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    WeaponManager weaponManager;

    [SerializeField] private Transform recolFollowPos;
    [SerializeField] private float kickBackAmount = -1;
    [SerializeField] private float kickBackSpeed = 10, returnSpeed = 20;
    private float currentRecoilPosition, finalRecoilPosition;

	private void Awake()
	{
		weaponManager = GetComponent<WeaponManager>();
	}

	// Update is called once per frame
	void Update()
    {
        currentRecoilPosition = Mathf.Lerp(currentRecoilPosition, 0, returnSpeed * Time.deltaTime); 
        finalRecoilPosition = Mathf.Lerp(finalRecoilPosition, currentRecoilPosition, kickBackSpeed * Time.deltaTime);
        recolFollowPos.localPosition = new Vector3(0, 0, finalRecoilPosition);

		weaponManager.muzzleFlashLight.intensity = Mathf.Lerp(weaponManager.muzzleFlashLight.intensity, 0, weaponManager.lightReturnSpeed * Time.deltaTime);
	}

    public void TriggerRecoil() => currentRecoilPosition += kickBackAmount;
}
