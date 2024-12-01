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

		/// <summary>
		/// Ŭ���̾�Ʈ���� ������ �߻� ��û�� ����
		/// </summary>
		[ServerRpc]
		private void FireServerRpc(Vector3 barrelPosition, Vector3[] spreadDirections)
		{
			foreach (var direction in spreadDirections)
			{
				if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
				{
					// �ǰ� ó��
					if (hit.transform.TryGetComponent(out PlayerHealth health))
					{
						health.TakeDamage(10, gameObject); // ������ ó��
					}

					// ��� Ŭ���̾�Ʈ�� �ð� ȿ���� ����ȭ
					FireEffectsClientRpc(hit.point, hit.normal, barrelPosition, direction);
				}
			}
		}

		/// <summary>
		/// ��� Ŭ���̾�Ʈ�� �ð� ȿ�� ����ȭ
		/// </summary>
		[ClientRpc]
		private void FireEffectsClientRpc(Vector3 hitPoint, Vector3 hitNormal, Vector3 barrelPosition, Vector3 direction)
		{
			if (Physics.Raycast(barrelPosition, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
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
