using QFSW.QC.Actions;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace ChocoOzing
{
	public class NewGun : GunBase
	{
		public ImpactType ImpactType;
		[SerializeField] private float spreadAngle = 10; // 산탄 각도 조절 변수
		private void OnEnable()
		{
			Debug.Log("NewGun!");
			aim.WeaponManager = this;
			aim.GunType = GunType.PumpShotGun;
			StartCoroutine(GunAction());
		}

		private void OnDisable()
		{
			aim.WeaponManager = null;
		}

		public override void Start()
		{
			ReloadActionAnim = "ManyReload";
		}

		public override IEnumerator GunAction()
		{
			while (true)
			{
				fireRateTimer += Time.deltaTime;

				if (IsLocalPlayer)
				{
					if (ShouldFire() && fireRateTimer >= fireRate)
					{
						FireServerRpc();
						fireRateTimer = 0;
					}
				}
				yield return null;
			}
		}

		public override bool ShouldFire()
		{
			// 클라이언트에서 발사 조건을 체크하고, 타이밍에 맞는지 확인합니다.
			if (fireRateTimer < fireRate) return false;
			if (ammo.currentAmmo == 0) return false;
			if (aim.currentState == aim.Reload) return false;
			if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
			if (!semiAuto && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
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

		RaycastHit hit;
		[ServerRpc]
		public void FireServerRpc()
		{
			FireClientRpc();
		}

		[ClientRpc]
		public void FireClientRpc()
		{
			barrelPos.LookAt(aim.aimPos);
			barrelPos.localEulerAngles = weaponBloom.BloomAngle(barrelPos, moveStateManager, aim);

			for (int i = 0; i < bulletPerShot; i++)
			{
				// 원뿔 형태로 퍼지도록 방향 벡터를 무작위로 생성
				Vector3 spreadDirection = Quaternion.Euler(
					Random.Range(-spreadAngle, spreadAngle),  // Pitch (상하)
					Random.Range(-spreadAngle, spreadAngle),  // Yaw (좌우)
					0) * barrelPos.forward;

				// Ray를 spreadDirection 방향으로 생성
				Ray ray = new Ray(barrelPos.position, spreadDirection);

				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
				{
					// 피격 대상이 있을 경우, 데미지 처리
					if (hit.transform.TryGetComponent(out PlayerHealth health))
					{
						//health.TakeDamage(10); // 서버에서 데미지 처리
						health.TakeDamage(10, gameObject);
					}
					// 클라이언트에게 시각적 효과만 동기화
					FireEffects(hit.point, hit.normal);
				}
			}
		}

		int countPershot = 1;
		
		private void FireEffects(Vector3 hitPoint, Vector3 hitNormal)
		{
			// 클라이언트에서 총알 효과 및 발사 사운드, 머즐 플래시 처리
			audioSource.PlayOneShot(gunShot);

			if(countPershot++ % bulletPerShot == 0)
			{
				ammo.currentAmmo--;
				aim.anim.Play("AdsPump");
			}
			
			// 시각적 효과 (머즐 플래시, 총구 불빛)
			weaponRecoil.TriggerRecoil();
			TriggerMuzzleFlash();


			//TestSurfaceManager//
			if (hit.collider != null)
			{
				SurfaceManager.Instance.HandleImpact(
					hit.transform.gameObject,
					hit.point,
					hit.normal,
					ImpactType,
					0
				);
			}
			else Debug.Log("hit NULLLLLLL");
			//TestSurfaceManager//

			// 피격 지점에 파티클 생성
			Instantiate(hitParticle, hitPoint, Quaternion.LookRotation(hitNormal));
		}

		public override void TriggerMuzzleFlash()
		{
			muzzleFlashParticle.Play();
			muzzleFlashLight.intensity = lightIntensity;
		}
	}
}