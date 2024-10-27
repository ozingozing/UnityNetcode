using ChocoOzing;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot Config", menuName = "Guns/Shoot Configuration", order =2)]
public class ShootConfigurationScriptableObject : ScriptableObject
{
	[Header("Fire Rate")]
	public float FireRate;
	public bool semiAuto;
	public float fireRateTimer;

	[Header("Bullet Properties")]
	public GameObject bullet;
	public float bulletVelocity;
	public int bulletPerShot;


	[Header("Shoot Effect")]
	public float lightIntensity;
	public float lightReturnSpeed = 20;


	[Header("Recoil")]
	public float kickBackAmount = -1;
	public float kickBackSpeed = 10, returnSpeed = 20;
	public float currentRecoilPosition, finalRecoilPosition;
	[Header("Bloom")]
	public float defaultBloomAngle = 3;
	public float walkBloomMultiplier = 500f;
	public float sprintBloomMultiplier = 1000f;
	public float adsBloomMultiplier = 0.5f;
	public float currentBloom;


	[Header("Ammo")]
	public int clipSize;
	public int extraAmmo;
	public int currentAmmo;
	public AudioClip magInSound;
	public AudioClip magOutSound;
	public AudioClip releaseSlideSound;
}
