using ChocoOzing;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class GunBase : NetworkBehaviour
{
	// 필요한 메서드 정의
	public abstract bool ShouldFire();
	public abstract void TriggerMuzzleFlash();
	public abstract IEnumerator GunAction();

	// Scripts
	public AimStateManager aim;
	public WeaponAmmo ammo;
	public WeaponRecoil weaponRecoil;
	public WeaponBloom weaponBloom;
	public MovementStateManager moveStateManager;

	// Fire Rate
	public float fireRate;
	public bool semiAuto;
	public float fireRateTimer;

	//Fire Muzzle Action
	public Light muzzleFlashLight;
	public ParticleSystem muzzleFlashParticle;
	public float lightIntensity;
	public float lightReturnSpeed = 20;


	// Bullet Properties
	public GameObject bullet;
	public Transform barrelPos;
	public float bulletVelocity;
	public int bulletPerShot;
	public AudioClip gunShot;
	public GameObject hitParticle;
	public AudioSource audioSource;
	public LayerMask layerMask;

	//SetAnimActionName;
	public string ReloadActionAnim;

	private void Awake()
	{
		weaponRecoil = GetComponent<WeaponRecoil>();
		weaponBloom = GetComponent<WeaponBloom>();
		ammo = GetComponent<WeaponAmmo>();
		aim = GetComponentInParent<AimStateManager>();
		moveStateManager = GetComponentInParent<MovementStateManager>();

		audioSource = GetComponentInParent<AudioSource>();

		muzzleFlashLight = GetComponentInChildren<Light>();
		muzzleFlashParticle = GetComponentInChildren<ParticleSystem>();
		lightIntensity = muzzleFlashLight.intensity;
		muzzleFlashLight.intensity = 0;

		fireRateTimer = fireRate;
	}

	public virtual void Start()
	{

	}

}
