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
					FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // ������ �߻� ��û
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
				FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // ������ �߻� ��û
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
						// �ǰ� ó��
						if (item.transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
						{
							damageReceiver.TakeDamage(10, gameObject); // ������ ó��
						}
					}
				}
			);*/
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
								damageReceiver.TakeDamageServerRpc(10, OwnerClientId); // ������ ó��
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

						// �ǰ� ������ ��ƼŬ ����
						SafeGetPoolObj(hitParticlePool, hits[i].point + hits[i].normal * 0.1f, Quaternion.identity);
						// Ŭ���̾�Ʈ���� �Ѿ� ȿ�� �� �߻� ����, ���� �÷��� ó��
						audioSource.PlayOneShot(gunShot, gunShootVolum);
						ammo.currentAmmo--;
						// �ð��� ȿ�� (���� �÷���, �ѱ� �Һ�)
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

			// �ǰ� ������ ��ƼŬ ����
			SafeGetPoolObj(hitParticlePool, hitPoint + hitNormal * 0.1f, Quaternion.identity);
			// Ŭ���̾�Ʈ���� �Ѿ� ȿ�� �� �߻� ����, ���� �÷��� ó��
			audioSource.PlayOneShot(gunShot, gunShootVolum);
			ammo.currentAmmo--;
			// �ð��� ȿ�� (���� �÷���, �ѱ� �Һ�)
			SafeGetPoolObj(muzzlePool, barrelPos.position, Quaternion.LookRotation(barrelPos.forward));*/
		}
	}
}