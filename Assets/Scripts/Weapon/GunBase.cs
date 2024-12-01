using ChocoOzing;
using System;
using System.Collections;
using System.Drawing;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class GunBase : NetworkBehaviour
{
	// 필요한 메서드 정의
	//public abstract void TriggerMuzzleFlash();
	public abstract IEnumerator GunAction();

	//NetworkParameter

	// Scripts
	public AimStateManager aim;
	public WeaponAmmo ammo;
	public WeaponRecoil weaponRecoil;
	public MovementStateManager moveStateManager;

	// Fire Rate
	public float fireRate;
	public bool semiAuto;
	public float fireRateTimer;
	public bool canShoot = false;

	//Fire Muzzle Action
	public GameObject muzzleFlashParticle;
	public float lightIntensity;
	public float lightReturnSpeed = 20;


	// Bullet Properties
	//public GameObject bullet;
	public Transform barrelPos;
	public float spreadAngle = 10; // 산탄 각도 조절 변수
	public float bulletVelocity;
	public int bulletPerShot;
	[Range(0f, 1f)] // 0부터 1까지의 범위를 가진 슬라이더
	public float gunShootVolum;
	public AudioClip gunShot;
	public GameObject hitParticle;
	public AudioSource audioSource;
	public LayerMask layerMask;

	//Bloom
	[SerializeField] private float defaultBloomAngle = 1f;
	[SerializeField] private float walkBloomMultiplier = 2f;
	[SerializeField] private float sprintBloomMultiplier = 3f;
	[SerializeField] private float adsBloomMultiplier = 0.5f;
	float currentBloom;

	Vector3[] spreadDirections;
	public ObjectPool hitParticlePool;
	public ObjectPool muzzlePool;
	public int defaultPoolSize = 15;

	private void Awake()
	{
		weaponRecoil = GetComponent<WeaponRecoil>();
		ammo = GetComponent<WeaponAmmo>();
		aim = GetComponentInParent<AimStateManager>();
		moveStateManager = GetComponentInParent<MovementStateManager>();

		audioSource = GetComponentInParent<AudioSource>();
	}

	public virtual void Start()
	{
		hitParticlePool = ObjectPool.CreateInstance(hitParticle.GetComponent<PoolableObject>(), defaultPoolSize);
		muzzlePool = ObjectPool.CreateInstance(muzzleFlashParticle.gameObject.GetComponent<PoolableObject>(), defaultPoolSize);

		spreadDirections = new Vector3[bulletPerShot];
		fireRateTimer = fireRate;
		currentBloom = defaultBloomAngle;
	}

	public void BarrelPositionReadyAction()
	{
		barrelPos.LookAt(aim.aimPos);
		barrelPos.localEulerAngles = BloomAngle(barrelPos, moveStateManager, aim);
		canShoot = true;
	}

	public virtual bool ShouldFire()
	{
		// 클라이언트에서 발사 조건을 체크하고, 타이밍에 맞는지 확인합니다.
		if (fireRateTimer < fireRate) return false;
		if (ammo.currentAmmo == 0) return false;
		if (aim.currentState == aim.Reload) return false;
		if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
		if (!semiAuto && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
		return false;
	}

	/// <summary>
	/// 각도를 기반으로 산탄 방향 벡터를 생성합니다.
	/// </summary>
	public Vector3[] GenerateSpreadDirections()
	{
		if (bulletPerShot == 1)
		{
			spreadDirections[0] = barrelPos.forward;
		}
		else
		{
			for (int i = 0; i < bulletPerShot; i++)
			{
				spreadDirections[i] = Quaternion.Euler(
					UnityEngine.Random.Range(-spreadAngle, spreadAngle), // Pitch (상하)
					UnityEngine.Random.Range(-spreadAngle, spreadAngle), // Yaw (좌우)
					0) * barrelPos.forward;
			}
		}
		return spreadDirections;
	}

	public void SafeGetPoolObj(ObjectPool pool, Vector3 point, Quaternion rotation)
	{
		if (!pool.GetObject(point, rotation))
		{
			SafeGetPoolObj(pool, point, rotation);
		}
	}

	public virtual Vector3 BloomAngle(Transform barrelPos, MovementStateManager currentState, AimStateManager aimState)
	{
		if (currentState.currentState == currentState.Walk)
		{
			currentBloom = defaultBloomAngle * walkBloomMultiplier;
		}
		else if (currentState.currentState == currentState.Run)
		{
			currentBloom = defaultBloomAngle * sprintBloomMultiplier;
		}
		else if (currentState.currentState == currentState.Idle)
		{
			currentBloom = defaultBloomAngle * adsBloomMultiplier;
		}

		if (aimState.currentState == aimState.Aim)
		{
			currentBloom *= adsBloomMultiplier;
		}

		float randX = UnityEngine.Random.Range(-currentBloom, currentBloom);
		float randY = UnityEngine.Random.Range(-currentBloom, currentBloom);
		float randZ = UnityEngine.Random.Range(-currentBloom, currentBloom);

		Vector3 randomRotation = new Vector3(randX, randY, randZ);

		return barrelPos.localEulerAngles + randomRotation;
	}
}
