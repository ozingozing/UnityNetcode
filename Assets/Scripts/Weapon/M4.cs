using QFSW.QC.Actions;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace ChocoOzing
{
	public class M4 : GunBase
	{
		private void OnEnable()
		{
			aim.WeaponManager = this;
			aim.GunType = GunType.M4A1;
			StartCoroutine(GunAction());
		}

		private void OnDisable()
		{
			aim.WeaponManager = null;
		}

		public override void Start()
		{
			ReloadActionAnim = "Reloading 0";
		}

		public override IEnumerator GunAction()
		{
			while (true)
			{
				fireRateTimer += Time.deltaTime;

				if(IsLocalPlayer)
				{
					if (ShouldFire())
					{
						RequestFireServerRpc();
					}
				}
				yield return null;
			}
		}

		public override bool ShouldFire()
		{
			// Ŭ���̾�Ʈ���� �߻� ������ üũ�ϰ�, Ÿ�ֿ̹� �´��� Ȯ���մϴ�.
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
			// �Ѿ� �߻� ó��
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

		[ServerRpc]
		private void RequestFireServerRpc()
		{
			if (fireRateTimer >= fireRate)
			{
				// �������� �߻� ó��
				Fire();
				fireRateTimer = 0;
			}
		}

		public override void Fire()
		{
			barrelPos.LookAt(aim.aimPos);
			barrelPos.localEulerAngles = weaponBloom.BloomAngle(barrelPos, moveStateManager, aim);
			for (int i = 0; i < bulletPerShot; i++)
			{
				// �������� Raycast ó�� ��, Ŭ���̾�Ʈ���� ��� ����
				Ray ray = new Ray(barrelPos.position, barrelPos.forward);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
				{
					// �ǰ� ����� ���� ���, ������ ó��
					if (hit.transform.TryGetComponent(out PlayerHealth health))
					{
						//health.TakeDamage(10); // �������� ������ ó��
						health.TakeDamage(10, gameObject);
					}
					// Ŭ���̾�Ʈ���� �ð��� ȿ���� ����ȭ
					FireEffectsClientRpc(hit.point, hit.normal);
				}
			}
		}

		[ClientRpc]
		private void FireEffectsClientRpc(Vector3 hitPoint, Vector3 hitNormal)
		{
			// Ŭ���̾�Ʈ���� �Ѿ� ȿ�� �� �߻� ����, ���� �÷��� ó��
			audioSource.PlayOneShot(gunShot);
			ammo.currentAmmo--;

			// �ð��� ȿ�� (���� �÷���, �ѱ� �Һ�)
			weaponRecoil.TriggerRecoil();
			TriggerMuzzleFlash();

			// �ǰ� ������ ��ƼŬ ����
			Instantiate(hitParticle, hitPoint, Quaternion.LookRotation(hitNormal));
		}

		public override void TriggerMuzzleFlash()
		{
			muzzleFlashParticle.Play();
			muzzleFlashLight.intensity = lightIntensity;
		}
	}
}