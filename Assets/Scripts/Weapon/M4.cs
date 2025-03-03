using ChocoOzing.Utilities;
using QFSW.QC.Actions;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChocoOzing
{
	public class M4 : GunBase
	{
		public ImpactType ImpactType;
		private void OnEnable()
		{
			myPlayer.WeaponManager = this;
			myPlayer.GunType = GunType.M4A1;
			if(IsOwner && IsLocalPlayer)
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

		/*private void FixedUpdate()
		{
			if(canShoot)
			{
				FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // 서버에 발사 요청
				canShoot = false;
			}
		}*/

		[ServerRpc]
		public void FireServerRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			/*RaycastBatchProcessor.Instance.PerformRaycasts(
				barrelPosition,
				spreadDirections,
				layerMask,
				false,
				false,
				false,
				(RaycastHit[] hits) =>
				{
					foreach (var item in hits)
					{
						// 피격 처리
						if (item.transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
						{
							damageReceiver.TakeDamage(10, gameObject); // 데미지 처리
						}
					}
				}
			);*/
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
						if (hits[i].transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
						{
							if(!IsHost)
								damageReceiver.TakeDamageServerRpc(10, OwnerClientId); // 데미지 처리
						}
						//TestSurfaceManager//
						else if (hits[i].collider != null)
						{
							SurfaceManager.Instance.HandleImpact(
								hits[i].transform.gameObject,
								hits[i].point,
								hits[i].normal,
								ImpactType,
								0
							);
						}
						//TestSurfaceManager//

						// 피격 지점에 파티클 생성
						SafeGetPoolObj(hitParticlePool, hits[i].point + hits[i].normal * 0.1f, Quaternion.identity);
						// 클라이언트에서 총알 효과 및 발사 사운드, 머즐 플래시 처리
						audioSource.PlayOneShot(gunShot, gunShootVolum);
						ammo.currentAmmo--;
						// 시각적 효과 (머즐 플래시, 총구 불빛)
						SafeGetPoolObj(muzzlePool, barrelPos.position, Quaternion.LookRotation(barrelPos.forward));
					}
				}
			);
			/*if(Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
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

			// 피격 지점에 파티클 생성
			SafeGetPoolObj(hitParticlePool, hitPoint + hitNormal * 0.1f, Quaternion.identity);
			// 클라이언트에서 총알 효과 및 발사 사운드, 머즐 플래시 처리
			audioSource.PlayOneShot(gunShot, gunShootVolum);
			ammo.currentAmmo--;
			// 시각적 효과 (머즐 플래시, 총구 불빛)
			SafeGetPoolObj(muzzlePool, barrelPos.position, Quaternion.LookRotation(barrelPos.forward));*/
		}
	}
}