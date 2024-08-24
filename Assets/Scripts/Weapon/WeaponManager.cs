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
		// 클라이언트의 입력을 확인하고 서버에 발사 요청을 보냅니다.
		fireRateTimer += Time.deltaTime;
		if (IsLocalPlayer && ShouldFire())
		{
			RequestFireServerRpc();
		}
	}

	private bool ShouldFire()
	{
		// 클라이언트에서 발사 조건을 체크하고, 타이밍에 맞는지 확인합니다.
		if (fireRateTimer < FireRate) return false;
		if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
		if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
		return false;
	}

	[ServerRpc(RequireOwnership = false)]
	private void RequestFireServerRpc(ServerRpcParams rpcParams = default)
	{
		// 서버에서 발사 요청을 처리하고, 발사 타이밍을 관리합니다.
		Debug.Log(fireRateTimer);
		if (fireRateTimer >= FireRate)
		{
			Fire();
			// 서버에서 발사 타이밍을 관리합니다.
			fireRateTimer = 0;
		}
	}

	private void Fire()
	{
		// 총알 발사 처리
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
