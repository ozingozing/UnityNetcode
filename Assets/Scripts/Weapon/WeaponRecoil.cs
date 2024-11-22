using ChocoOzing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
	GunBase weaponManager;

    [SerializeField] private Transform recolFollowPos;
    [SerializeField] private float kickBackAmount = -1;
    [SerializeField] private float kickBackSpeed = 10, returnSpeed = 20;
    private float currentRecoilPosition, finalRecoilPosition;
	float velocity = 0.0f;
	private void Awake()
	{
		weaponManager = GetComponent<GunBase>();
	}

	// Update is called once per frame
	void Update()
    {
		//currentRecoilPosition = Mathf.Lerp(currentRecoilPosition, 0, returnSpeed * Time.deltaTime); 
		//finalRecoilPosition = Mathf.Lerp(finalRecoilPosition, currentRecoilPosition, kickBackSpeed * Time.deltaTime);
		/*currentRecoilPosition = Mathf.SmoothDamp(currentRecoilPosition, 0, ref velocity, 1 / returnSpeed);
		finalRecoilPosition = Mathf.SmoothDamp(currentRecoilPosition, finalRecoilPosition, ref velocity, 1 / kickBackSpeed);
		recolFollowPos.localPosition = new Vector3(0, 0, finalRecoilPosition);
		*/
		weaponManager.muzzleFlashLight.intensity = Mathf.Lerp(weaponManager.muzzleFlashLight.intensity, 0, weaponManager.lightReturnSpeed * Time.deltaTime);
	}

	private void FixedUpdate()
	{
		currentRecoilPosition = Mathf.SmoothDamp(currentRecoilPosition, 0, ref velocity, 1 / returnSpeed);
		finalRecoilPosition = Mathf.SmoothDamp(currentRecoilPosition, finalRecoilPosition, ref velocity, 1 / kickBackSpeed);
		recolFollowPos.localPosition = new Vector3(0, 0, finalRecoilPosition);
	}

	public void TriggerRecoil() => currentRecoilPosition += kickBackAmount;
}
