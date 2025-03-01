using ChocoOzing.Utilities;
using QFSW.QC.Actions;
using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace ChocoOzing
{
	public class NewGun : GunBase
	{
		public ImpactType ImpactType;
		
		private void OnEnable()
		{
			myPlayer.WeaponManager = this;
			myPlayer.GunType = GunType.PumpShotGun;
			if (IsOwner)
				StartCoroutine(GunAction());
		}

		private void OnDisable()
		{
			myPlayer.WeaponManager = null;
		}
		public override IEnumerator GunAction()
		{
			while (true)
			{
				fireRateTimer += Time.deltaTime;

				if (ShouldFire() && fireRateTimer >= fireRate)
				{
					BarrelPositionReadyAction();
					FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // 서버에 발사 요청
					weaponRecoil.TriggerRecoil();
					fireRateTimer = 0;
				}
				yield return null;
			}
		}

		/// <summary>
		/// 클라이언트에서 서버로 발사 요청을 전달
		/// </summary>
		[ServerRpc]
		private void FireServerRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			RaycastBatchProcessor.Instance.PerformRaycasts(
				barrelPosition,
				spreadDirections,
				layerMask,
				false,
				false,
				false,
				(RaycastHit[] hits) =>
				{
					for(int i = 0; i < hits.Length; i++)
					{
						// 피격 처리
						if (hits[i].transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
						{
							damageReceiver.TakeDamage(10, gameObject); // 데미지 처리
						}
					}
				}
			);
			// 모든 클라이언트에 시각 효과를 동기화
			FireEffectsClientRpc(barrelPosition, spreadDirections);

			/*foreach (var direction in spreadDirections)
			{
				if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
				{
					// 피격 처리
					if (hit.transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
					{
						damageReceiver.TakeDamage(10, gameObject); // 데미지 처리
					}

					// 모든 클라이언트에 시각 효과를 동기화
					FireEffectsClientRpc(hit.point, hit.normal, barrelPosition, direction);
				}
			}*/
		}

		/// <summary>
		/// 모든 클라이언트에 시각 효과 동기화
		/// </summary>
		[ClientRpc]
		private void FireEffectsClientRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			RaycastBatchProcessor.Instance.PerformRaycasts(
				barrelPosition,
				spreadDirections,
				layerMask,
				false,
				false,
				false,
				(RaycastHit[] hits) =>
				{
					for (int i = 0; i < hits.Length; i++)
					{
						//TestSurfaceManager//
						if (hits[i].collider != null)
						{
							SurfaceManager.Instance.HandleImpact(
								hits[i].transform.gameObject,
								hits[i].point,
								hits[i].normal,
								ImpactType,
								0
							);

							// 파티클 및 사운드 효과 처리
							SafeGetPoolObj(hitParticlePool, hits[i].point + hits[i].normal * 0.1f, Quaternion.identity);

							if (countPershot++ % bulletPerShot == 0)
							{
								SafeGetPoolObj(muzzlePool, barrelPos.position, Quaternion.LookRotation(barrelPos.forward));
								ammo.currentAmmo--;
								myPlayer.Anim.Play("AdsPump");
							}
							else if (countPershot % 2 != 0)
							{
								audioSource.PlayOneShot(gunShot, gunShootVolum);
							}
						}
						//TestSurfaceManager//
					}
				}
			);

			/*if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
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

			// 파티클 및 사운드 효과 처리
			SafeGetPoolObj(hitParticlePool, hitPoint + hitNormal * 0.1f, Quaternion.identity);

			if (countPershot++ % bulletPerShot == 0)
			{
				SafeGetPoolObj(muzzlePool, barrelPos.position, Quaternion.LookRotation(barrelPos.forward));
				ammo.currentAmmo--;
				myPlayer.Anim.Play("AdsPump");
			}
			else if (countPershot % 2 != 0)
			{
				audioSource.PlayOneShot(gunShot, gunShootVolum);
			}*/
		}

		private int countPershot = 1;
	}
}
