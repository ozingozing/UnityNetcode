using QFSW.QC.Actions;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChocoOzing
{
	public class NewGun : GunBase
	{
		public ImpactType ImpactType;
		[SerializeField] private float spreadAngle = 10; // 산탄 각도 조절 변수

		private void OnEnable()
		{
			aim.WeaponManager = this;
			aim.GunType = GunType.PumpShotGun;
			if (IsOwner)
				StartCoroutine(GunAction());
		}

		private void OnDisable()
		{
			aim.WeaponManager = null;
		}

		public override IEnumerator GunAction()
		{
			while (true)
			{
				fireRateTimer += Time.deltaTime;

				if (ShouldFire() && fireRateTimer >= fireRate)
				{
					can = true;
					weaponRecoil.TriggerRecoil();
					fireRateTimer = 0;
				}
				yield return null;
			}
		}
		bool can = false;
		private void FixedUpdate()
		{
			if(can)
			{
				FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // 서버에 발사 요청
				/*if(IsServer)
				{
					Vector3[] spreadDirections = GenerateSpreadDirections();
					foreach (var direction in spreadDirections)
					{
						if (Physics.Raycast(barrelPos.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
						{
							// 피격 처리
							if (hit.transform.TryGetComponent(out PlayerHealth health))
							{
								health.TakeDamage(10, gameObject); // 데미지 처리
							}

							// 모든 클라이언트에 시각 효과를 동기화
							FireEffectsClientRpc(hit.point, hit.normal, barrelPos.position, direction);
						}
					}
				}
				else FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // 서버에 발사 요청*/
				can = false;
			}
		}

		public override bool ShouldFire()
		{
			if (fireRateTimer < fireRate) return false;
			if (ammo.currentAmmo == 0) return false;
			if (aim.currentState == aim.Reload) return false;
			if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
			if (!semiAuto && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
			return false;
		}

		/// <summary>
		/// 각도를 기반으로 산탄 방향 벡터를 생성합니다.
		/// </summary>
		private Vector3[] GenerateSpreadDirections()
		{
			Vector3[] spreadDirections = new Vector3[bulletPerShot];
			for (int i = 0; i < bulletPerShot; i++)
			{
				spreadDirections[i] = Quaternion.Euler(
					Random.Range(-spreadAngle, spreadAngle), // Pitch (상하)
					Random.Range(-spreadAngle, spreadAngle), // Yaw (좌우)
					0) * barrelPos.forward;
			}
			return spreadDirections;
		}

		/// <summary>
		/// 클라이언트에서 서버로 발사 요청을 전달
		/// </summary>
		[ServerRpc]
		private void FireServerRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			foreach (var direction in spreadDirections)
			{
				if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
				{
					// 피격 처리
					if (hit.transform.TryGetComponent(out PlayerHealth health))
					{
						health.TakeDamage(10, gameObject); // 데미지 처리
					}

					// 모든 클라이언트에 시각 효과를 동기화
					FireEffectsClientRpc(hit.point, hit.normal, barrelPosition, direction);
				}
			}
		}

		/// <summary>
		/// 모든 클라이언트에 시각 효과 동기화
		/// </summary>
		[ClientRpc]
		private void FireEffectsClientRpc(Vector3 hitPoint, Vector3 hitNormal, Vector3 barrelPosition, Vector3 direction)
		{
			if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
			{
				if(hit.collider)
				{
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
					//TestSurfaceManager//
				}
			}

			// 파티클 및 사운드 효과 처리
			SafeGetPoolObj(hitParticlePool, hitPoint + hitNormal * 0.1f, Quaternion.identity);

			if (countPershot++ % bulletPerShot == 0)
			{
				SafeGetPoolObj(muzzlePool, barrelPos.position, Quaternion.identity);
				ammo.currentAmmo--;
				aim.anim.Play("AdsPump");
			}
			else if (countPershot % 2 != 0)
			{
				audioSource.PlayOneShot(gunShot, gunShootVolum);
			}
		}

		private int countPershot = 1;
	}
}
