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
					FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // ������ �߻� ��û
					weaponRecoil.TriggerRecoil();
					fireRateTimer = 0;
				}
				yield return null;
			}
		}

		/// <summary>
		/// Ŭ���̾�Ʈ���� ������ �߻� ��û�� ����
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
						// �ǰ� ó��
						if (hits[i].transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
						{
							damageReceiver.TakeDamage(10, gameObject); // ������ ó��
						}
					}
				}
			);
			// ��� Ŭ���̾�Ʈ�� �ð� ȿ���� ����ȭ
			FireEffectsClientRpc(barrelPosition, spreadDirections);

			/*foreach (var direction in spreadDirections)
			{
				if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
				{
					// �ǰ� ó��
					if (hit.transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
					{
						damageReceiver.TakeDamage(10, gameObject); // ������ ó��
					}

					// ��� Ŭ���̾�Ʈ�� �ð� ȿ���� ����ȭ
					FireEffectsClientRpc(hit.point, hit.normal, barrelPosition, direction);
				}
			}*/
		}

		/// <summary>
		/// ��� Ŭ���̾�Ʈ�� �ð� ȿ�� ����ȭ
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

							// ��ƼŬ �� ���� ȿ�� ó��
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

			// ��ƼŬ �� ���� ȿ�� ó��
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
