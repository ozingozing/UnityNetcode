using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

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
	AudioSource audioSource;

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

	[ServerRpc(RequireOwnership = false)]
	private void RequestFireServerRpc(ServerRpcParams rpcParams = default)
	{
		// �������� �߻� ��û�� ó���ϰ�, �߻� Ÿ�̹��� �����մϴ�.
		Debug.Log(fireRateTimer);
		if (fireRateTimer >= FireRate)
		{
			Fire();
			// �������� �߻� Ÿ�̹��� �����մϴ�.
			fireRateTimer = 0;
		}
	}

	private void Fire()
	{
		// �Ѿ� �߻� ó��
		barrelPos.LookAt(aim.aimPos);

		for (int i = 0; i < bulletPerShot; i++)
		{
			GameObject currentBullet = Instantiate(bullet, barrelPos.position, barrelPos.rotation);

			NetworkObject networkObject = currentBullet.GetComponent<NetworkObject>();
			if (networkObject != null)
			{
				networkObject.Spawn();
			}

			Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
			audioSource.PlayOneShot(gunShot);
			rb.AddForce(barrelPos.forward * bulletVelocity, ForceMode.Impulse);
		}
	}
}
