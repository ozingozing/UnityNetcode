using ChocoOzing;
using ChocoOzing.CoreSystem;
using System;
using System.Collections;
using System.Drawing;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public abstract class GunBase : NetworkBehaviour
{
	// �ʿ��� �޼��� ����
	//public abstract void TriggerMuzzleFlash();
	public abstract IEnumerator GunAction();

	//NetworkParameter

	// Scripts
	public WeaponAmmo ammo;
	public WeaponRecoil weaponRecoil;
	public MyPlayer myPlayer;

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
	public float spreadAngle = 10; // ��ź ���� ���� ����
	public float bulletVelocity;
	public int bulletPerShot;
	[Range(0f, 1f)] // 0���� 1������ ������ ���� �����̴�
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
		myPlayer = GetComponentInParent<MyPlayer>();
		weaponRecoil = GetComponent<WeaponRecoil>();
		ammo = GetComponent<WeaponAmmo>();

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
		barrelPos.LookAt(myPlayer.aimPos);
		barrelPos.localEulerAngles = BloomAngle(barrelPos, myPlayer);
		canShoot = true;
	}

	public virtual bool ShouldFire()
	{
		// Ŭ���̾�Ʈ���� �߻� ������ üũ�ϰ�, Ÿ�ֿ̹� �´��� Ȯ���մϴ�.
		if (fireRateTimer < fireRate) return false;
		if (ammo.currentAmmo == 0) return false;
		if (myPlayer.StateMachine.CurrentState == myPlayer.ReloadState) return false;
		if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
		if (!semiAuto && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.Mouse1)) return true;
		return false;
	}

	/// <summary>
	/// ������ ������� ��ź ���� ���͸� �����մϴ�.
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
					UnityEngine.Random.Range(-spreadAngle, spreadAngle), // Pitch (����)
					UnityEngine.Random.Range(-spreadAngle, spreadAngle), // Yaw (�¿�)
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

	public virtual Vector3 BloomAngle(Transform barrelPos, MyPlayer currentState)
	{
		if (currentState.StateMachine.CurrentState == currentState.WalkState)
		{
			currentBloom = defaultBloomAngle * walkBloomMultiplier;
		}
		else if (currentState.StateMachine.CurrentState == currentState.RunState)
		{
			currentBloom = defaultBloomAngle * sprintBloomMultiplier;
		}
		else if (currentState.StateMachine.CurrentState == currentState.IdleState)
		{
			currentBloom = defaultBloomAngle * adsBloomMultiplier;
		}

		if (myPlayer.StateMachine.CurrentState == currentState.AimState)
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
