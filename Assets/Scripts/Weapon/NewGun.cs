using ChocoOzing.Utilities;
using QFSW.QC.Actions;
using System.Collections;
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
						FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // ������ �߻� ��û
					weaponRecoil.TriggerRecoil();
					fireRateTimer = 0;
				}
				yield return null;
			}
		}

		private void FixedUpdate()
		{
			if(IsServer && can)
			{
				can =false;
				RaycastBatchProcessor.Instance.PerformRaycasts(
				pos,
				spreads,
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
							//TestSurfaceManager//
						}
					});
			}
		}

		bool can = false;
		Vector3 pos;
		Vector3[] spreads;
		/// <summary>
		/// Ŭ���̾�Ʈ���� ������ �߻� ��û�� ����
		/// </summary>
		[ServerRpc]
		private void FireServerRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			FireEffectsClientRpc(barrelPosition, spreadDirections);
			can = true;
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
					foreach (var item in hits)
					{
						// �ǰ� ó��
						if (item.transform.TryGetComponentInChildren(out DamageReceiver damageReceiver))
						{
							damageReceiver.TakeDamage(10, gameObject); // ������ ó��
						}
					}
				}
			);
			// ��� Ŭ���̾�Ʈ�� �ð� ȿ���� ����ȭ
			FireEffectsClientRpc(barrelPosition, spreadDirections);*/
		}

		bool clientShot = false;
		/// <summary>
		/// ��� Ŭ���̾�Ʈ�� �ð� ȿ�� ����ȭ
		/// </summary>
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
						//TestSurfaceManager//
					}
				}
			);*/
		}

		private int countPershot = 1;
	}
}
