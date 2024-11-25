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
	public abstract bool ShouldFire();
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

	//Fire Muzzle Action
	public GameObject muzzleFlashParticle;
	public float lightIntensity;
	public float lightReturnSpeed = 20;


	// Bullet Properties
	//public GameObject bullet;
	public Transform barrelPos;
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

		fireRateTimer = fireRate;
		currentBloom = defaultBloomAngle;
	}

	public virtual void Start()
	{
		hitParticlePool = ObjectPool.CreateInstance(hitParticle.GetComponent<PoolableObject>(), defaultPoolSize);
		muzzlePool = ObjectPool.CreateInstance(muzzleFlashParticle.gameObject.GetComponent<PoolableObject>(), defaultPoolSize);
	}

	public void SafeGetPoolObj(ObjectPool pool, Vector3 point, Quaternion rotation)
	{
		StartCoroutine(GetPoolObject(pool, point, rotation));
	}

	IEnumerator GetPoolObject(ObjectPool pool, Vector3 point, Quaternion rotation)
	{
		if (!pool.GetObject(point, rotation))
		{
			yield return null;
			SafeGetPoolObj(pool, point, rotation);
		}
		yield return null;
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
