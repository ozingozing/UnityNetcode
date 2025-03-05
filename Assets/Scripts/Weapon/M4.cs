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
			if (IsOwner && IsLocalPlayer)
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
					if (IsClient)
						FireServerRpc(barrelPos.position, GenerateSpreadDirections());
					weaponRecoil.TriggerRecoil();
					fireRateTimer = 0;
				}
				yield return null;
			}
		}

		private void FixedUpdate()
		{
			if(IsServer && canShot)
			{
				canShot = false;
				RaycastBatchProcessor.Instance.PerformRaycasts(
				pos,
				spreads,
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
							damageReceiver.TakeDamage(10, gameObject); // 데미지 처리
						}
					}
				});
			}
			else if(IsClient && clientShot)
			{
				clientShot = false;
				RaycastBatchProcessor.Instance.PerformRaycasts(
					pos,
					spreads,
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
					});
			}
		}

		bool canShot = false;
		Vector3 pos;
		Vector3[] spreads;
		[ServerRpc]
		public void FireServerRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			FireEffectsClientRpc(barrelPosition, spreadDirections);
			canShot = true;
			pos = barrelPosition;
			spreads = spreadDirections;
			/*RaycastBatchProcessor.Instance.PerformRaycasts(
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
							damageReceiver.TakeDamage(10, gameObject); // 데미지 처리
						}
					}
				});

			FireEffectsClientRpc(barrelPosition, spreadDirections);*/
		}

		bool clientShot = false;
		[ClientRpc]
		private void FireEffectsClientRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			clientShot = true;
			pos = barrelPosition;
			spreads = spreadDirections;
			/*RaycastBatchProcessor.Instance.PerformRaycasts(
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
			});*/
		}
	}
}