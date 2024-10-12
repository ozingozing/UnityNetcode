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

		/*
		 * 1. In the current code, collision detection and damage
		 * are being processed on the client,
		 * which creates an integrity issue between the server and client.
		 * 2. It is recommended to handle collision detection 
		 * and damage on the server,
		 * and synchronize the results to all clients using ClientRpc.
		 */
		/*[ServerRpc]
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
			//barrelPos.localEulerAngles = weaponBloom.BloomAngle(barrelPos);
			//Debug.Log(barrelPos.localEulerAngles);

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

				if (hit.transform.TryGetComponent(out PlayerHealth health))
				{
					health.TakeDamage(10);
				}
			}
		}*/

		[ServerRpc]
		private void RequestFireServerRpc()
		{
			if (fireRateTimer >= FireRate)
			{
				// 서버에서 발사 처리
				FireServer();
				fireRateTimer = 0;
			}
		}

		private void FireServer()
		{
			barrelPos.LookAt(aim.aimPos);

			for (int i = 0; i < bulletPerShot; i++)
			{
				// 서버에서 Raycast 처리 후, 클라이언트에게 결과 전달
				Ray ray = new Ray(barrelPos.position, barrelPos.forward);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
				{
					// 피격 대상이 있을 경우, 데미지 처리
					if (hit.transform.TryGetComponent(out PlayerHealth health))
					{
						//health.TakeDamage(10); // 서버에서 데미지 처리
						health.TakeDamage(10, gameObject);
					}
					// 클라이언트에게 시각적 효과만 동기화
					FireEffectsClientRpc(hit.point, hit.normal);
				}
			}
		}

		[ClientRpc]
		private void FireEffectsClientRpc(Vector3 hitPoint, Vector3 hitNormal)
		{
			// 클라이언트에서 총알 효과 및 발사 사운드, 머즐 플래시 처리
			audioSource.PlayOneShot(gunShot);
			ammo.currentAmmo--;

			// 시각적 효과 (머즐 플래시, 총구 불빛)
			weaponRecoil.TriggerRecoil();
			TriggerMuzzleFlash();

			// 피격 지점에 파티클 생성
			Instantiate(hitParticle, hitPoint, Quaternion.LookRotation(hitNormal));
		}

		private void TriggerMuzzleFlash()
		{
			muzzleFlashParticle.Play();
			muzzleFlashLight.intensity = lightIntensity;
		}
	}
}