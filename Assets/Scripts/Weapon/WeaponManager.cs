using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChocoOzing
{
	public class WeaponManager : NetworkBehaviour
	{
		[Header("Fire Rate")]
		[SerializeField] private float FireRate;
		[SerializeField] private bool semiAuto;
		private float fireRateTimer;

		[Header("Bullet Properties")]
		[SerializeField] private GameObject bullet;
		[SerializeField] private Transform barrelPos;
		[SerializeField] private float bulletVelocity;
		[SerializeField] private int bulletPerShot;
		private AimStateManager aim;

		[SerializeField] private AudioClip gunShot;
		[SerializeField] private GameObject hitParticle;
		public AudioSource audioSource;
		[SerializeField] private LayerMask layerMask;
		public WeaponAmmo ammo;
		WeaponRecoil weaponRecoil;
		WeaponBloom weaponBloom;
		
		public Light muzzleFlashLight;
		ParticleSystem muzzleFlashParticle;
		float lightIntensity;
		[SerializeField] public float lightReturnSpeed = 20;


		private void Awake()
		{
			weaponRecoil = GetComponent<WeaponRecoil>();
			weaponBloom = GetComponent<WeaponBloom>();
			aim = GetComponentInParent<AimStateManager>();
			audioSource = GetComponentInParent<AudioSource>();
			ammo = GetComponent<WeaponAmmo>();

			muzzleFlashLight = GetComponentInChildren<Light>();
			lightIntensity = muzzleFlashLight.intensity;
			muzzleFlashLight.intensity = 0;
			muzzleFlashParticle = GetComponentInChildren<ParticleSystem>();
		}

		void Start()
		{
			fireRateTimer = FireRate;
			StartCoroutine(UpdateCoroutine());
		}

		public IEnumerator UpdateCoroutine()
		{
			while (true)
			{
				fireRateTimer += Time.deltaTime;
				if (IsLocalPlayer)
				{
					if(ShouldFire())
						RequestFireServerRpc();
				}
				yield return null;
			}
		}

		public bool ShouldFire()
		{
			// 클라이언트에서 발사 조건을 체크하고, 타이밍에 맞는지 확인합니다.
			if (fireRateTimer < FireRate) return false;
			if (ammo.currentAmmo == 0) return false;
			if (aim.currentState == aim.Reload) return false;
			if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
			if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
			return false;
		}

		[ServerRpc]
		private void RequestFireServerRpc()
		{
			if (fireRateTimer >= FireRate)
			{
				Fire();
				fireRateTimer = 0;
			}
		}

		private void Fire()
		{
			// 총알 발사 처리
			barrelPos.LookAt(aim.aimPos);
			/*barrelPos.localEulerAngles = weaponBloom.BloomAngle(barrelPos);
			Debug.Log(barrelPos.localEulerAngles);*/

			for (int i = 0; i < bulletPerShot; i++)
			{
				ShootRayClientRpc();
			}
		}

		[ClientRpc]
		private void ShootRayClientRpc()
		{
			Ray ray = new Ray(barrelPos.position, barrelPos.forward);
			RaycastHit hit;

			weaponRecoil.TriggerRecoil();
			TriggerMuzzleFlash();

			if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
			{
				audioSource.PlayOneShot(gunShot);
				ammo.currentAmmo--;
				Instantiate(hitParticle, hit.point, Quaternion.LookRotation(hit.normal));
			}
		}

		private void TriggerMuzzleFlash()
		{
			muzzleFlashParticle.Play();
			muzzleFlashLight.intensity = lightIntensity;
		}
	}
}