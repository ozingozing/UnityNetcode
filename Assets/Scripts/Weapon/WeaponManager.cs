using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponManager : NetworkBehaviour
{
	CheckLocalComponent checkLocalComponent;

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
	AudioSource audioSource;
	[SerializeField] private LayerMask layerMask;

	private void Awake()
	{
		aim = GetComponentInParent<AimStateManager>();
		audioSource = GetComponentInParent<AudioSource>();
		checkLocalComponent = GetComponentInParent<CheckLocalComponent>();
	}

	void Start()
	{
		fireRateTimer = FireRate;
	}

	void Update()
	{
		// Ŭ���̾�Ʈ�� �Է��� Ȯ���ϰ� ������ �߻� ��û�� �����ϴ�.
		fireRateTimer += Time.deltaTime;
		if (IsLocalPlayer && ShouldFire())
		{
			RequestFireServerRpc();
		}
	}

	private bool ShouldFire()
	{
		// Ŭ���̾�Ʈ���� �߻� ������ üũ�ϰ�, Ÿ�ֿ̹� �´��� Ȯ���մϴ�.
		if (fireRateTimer < FireRate) return false;
		if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
		if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
		return false;
	}

	[ServerRpc]
	private void RequestFireServerRpc(ServerRpcParams rpcParams = default)
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

		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
		{
			Instantiate(hitParticle, hit.point, Quaternion.LookRotation(hit.normal));
		}
	}
}
