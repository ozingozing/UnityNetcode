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

	private void OnEnable()
	{
		StartCoroutine(WeaponRecoilAction());
	}

	IEnumerator WeaponRecoilAction()
	{
		while (true)
		{
			yield return null;
			currentRecoilPosition = Mathf.SmoothDamp(currentRecoilPosition, 0, ref velocity, 1 / returnSpeed);
			finalRecoilPosition = Mathf.SmoothDamp(currentRecoilPosition, finalRecoilPosition, ref velocity, 1 / kickBackSpeed);
			recolFollowPos.localPosition = new Vector3(0, 0, finalRecoilPosition);
		}
	}

	public void TriggerRecoil() => currentRecoilPosition += kickBackAmount;
}
