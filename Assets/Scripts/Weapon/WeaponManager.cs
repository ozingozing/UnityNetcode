using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChocoOzing
{
	public class WeaponManager : NetworkBehaviour
	{
		[Header("Fire Rate")]
		[SerializeField] private float FireRate;
		[SerializeField] private bool semiAuto;
		private float fireRateTimer;

		[Header("Bullet Properties")]
		[SerializeField] private GameObject bullet;
		[SerializeField] private Transform barrelPos;
		[SerializeField] private float bulletVelocity;
		[SerializeField] private int bulletPerShot;
		private AimStateManager aim;

		[SerializeField] private AudioClip gunShot;
		[SerializeField] private GameObject hitParticle;
		public AudioSource audioSource;
		[SerializeField] private LayerMask layerMask;
		public WeaponAmmo ammo;

		private void Awake()
		{
			aim = GetComponentInParent<AimStateManager>();
			audioSource = GetComponentInParent<AudioSource>();
			ammo = GetComponent<WeaponAmmo>();
		}

		void Start()
		{
			fireRateTimer = FireRate;
			StartCoroutine(UpdateCoroutine());
		}

		public IEnumerator UpdateCoroutine()
		{
			while (true)
			{
				fireRateTimer += Time.deltaTime;
				if (IsOwner)
				{
					if (IsLocalPlayer && ShouldFire())
					{
						RequestFireServerRpc();
					}
				}
				yield return null;
			}
		}

		private bool ShouldFire()
		{
			// Ŭ���̾�Ʈ���� �߻� ������ üũ�ϰ�, Ÿ�ֿ̹� �´��� Ȯ���մϴ�.
			if (fireRateTimer < FireRate) return false;
			if (ammo.currentAmmo == 0) return false;
			if (aim.currentState == aim.Reload) return false;
			if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
			if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
			return false;
		}

		[ServerRpc]
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
			audioSource.PlayOneShot(gunShot);

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

			if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
			{
				ammo.currentAmmo--;
				Instantiate(hitParticle, hit.point, Quaternion.LookRotation(hit.normal));
			}
		}
	}
}