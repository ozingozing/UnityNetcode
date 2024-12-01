using QFSW.QC.Actions;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace ChocoOzing
{
	public class NewGun : GunBase
	{
		public ImpactType ImpactType;
		[SerializeField] private float spreadAngle = 10; // ��ź ���� ���� ����

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
				FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // ������ �߻� ��û
				/*if(IsServer)
				{
					Vector3[] spreadDirections = GenerateSpreadDirections();
					foreach (var direction in spreadDirections)
					{
						if (Physics.Raycast(barrelPos.position, direction, out RaycastHit hit, Mathf.Infinity, layerMask))
						{
							// �ǰ� ó��
							if (hit.transform.TryGetComponent(out PlayerHealth health))
							{
								health.TakeDamage(10, gameObject); // ������ ó��
							}

							// ��� Ŭ���̾�Ʈ�� �ð� ȿ���� ����ȭ
							FireEffectsClientRpc(hit.point, hit.normal, barrelPos.position, direction);
						}
					}
				}
				else FireServerRpc(barrelPos.position, GenerateSpreadDirections()); // ������ �߻� ��û*/
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
		/// ������ ������� ��ź ���� ���͸� �����մϴ�.
		/// </summary>
		private Vector3[] GenerateSpreadDirections()
		{
			Vector3[] spreadDirections = new Vector3[bulletPerShot];
			for (int i = 0; i < bulletPerShot; i++)
			{
				spreadDirections[i] = Quaternion.Euler(
					Random.Range(-spreadAngle, spreadAngle), // Pitch (����)
					Random.Range(-spreadAngle, spreadAngle), // Yaw (�¿�)
					0) * barrelPos.forward;
			}
			return spreadDirections;
		}

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
