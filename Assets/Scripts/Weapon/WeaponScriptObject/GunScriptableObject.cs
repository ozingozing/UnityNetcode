using ChocoOzing;
using System.Collections;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Pool;

public enum WeaponType
{
	Rifle,
	Shotgun,
	Sniper,
	// 필요한 무기 종류를 추가
}

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
	[Header("WeaponType")]
	public WeaponType WeaponType;

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
	WeaponRecoil weaponRecoil;

	public Light muzzleFlashLight;
	ParticleSystem muzzleFlashParticle;
	float lightIntensity;
	[SerializeField] public float lightReturnSpeed = 20;
	// 기타 무기 속성 추가
}
