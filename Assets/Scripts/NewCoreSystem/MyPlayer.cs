using ChocoOzing.CoreSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public enum GunType
{
	M4A1,
	PumpShotGun,
}

public class MyPlayer : MonoBehaviour
{
	/// <summary>
	/// if you want use Rigidbody? use this
	/// UNT0008 Null propagation on Unity objects
	/// Don't use [?.]
	/// </summary>
	protected Movement Movement
	{ get => movement != null ? movement : Core.GetCoreComponent(ref movement); }
	private Movement movement;

	#region Component
	public Core Core { get; private set; }
	public Animator Anim {  get; private set; }
	public CapsuleCollider MovementCollider { get; private set; }
	#endregion

	#region Weapon
	public GunBase WeaponManager;
	public GunType GunType;
	public Transform aimPos;
	public Transform camFollowPos;
	#endregion

	#region State Variables
	public PlayerStateMachine StateMachine { get; private set; }
	public PlayerGunStateMachine GunStateMachine { get; private set; }
	public IdleState IdleState { get; private set; }
	public WalkState WalkState { get; private set; }
	public RunState RunState { get; private set; }
	public HipFireState HipFireState { get; private set; }
	public AimState AimState { get; private set; }
	public ReloadState ReloadState { get; private set; }
	public DefaultState DefaultState { get; private set; }
	#endregion

	private void OnEnable()
	{
		CamManager.Instance.MapViewCam.Priority = 0;
	}

	private void Awake()
	{
		Core = GetComponentInChildren<Core>();

		StateMachine = new PlayerStateMachine();
		GunStateMachine = new PlayerGunStateMachine();
		
		IdleState = new IdleState(this, StateMachine, "Idle");
		WalkState = new WalkState(this, StateMachine, "IsWalking");
		RunState = new RunState(this, StateMachine, "IsRunning");
		HipFireState = new HipFireState(this, GunStateMachine, "IsHipfiring");
		AimState = new AimState(this, GunStateMachine, "IsAiming");
		ReloadState = new ReloadState(this, GunStateMachine, "IsReloading");
	}
	private void Start()
	{
		Anim = GetComponent<Animator>();
		MovementCollider = GetComponent<CapsuleCollider>();

		StateMachine.Initialize(IdleState);
		GunStateMachine.Initialize(HipFireState);
	}

	public void PlayerActionStart()
	{
		StartCoroutine(UpdateCoroutine());
		StartCoroutine(FixedUpdateCoroutine());
	}

	IEnumerator UpdateCoroutine()
	{
		while (true)
		{
			yield return null;
			Core.LogicUpdate();
			StateMachine.CurrentState.LogicUpdate();
			GunStateMachine.CurrentState.LogicUpdate();
		}
	}

	IEnumerator FixedUpdateCoroutine()
	{
		while (true)
		{
			yield return new WaitForFixedUpdate();
			StateMachine.CurrentState.PhysicsUpdate();
			GunStateMachine.CurrentState.PhysicsUpdate();
		}
	}

	public void ShotGunReloadAction()
	{
		if (WeaponManager.ammo.currentAmmo < WeaponManager.ammo.clipSize)
				WeaponManager.ammo.ShotGunReload();

		if (WeaponManager.ammo.currentAmmo < WeaponManager.ammo.clipSize)
		{
			Anim.Play("ShotgunReloadAction", -1, 0f);
		}
		else
		{
			Anim.Play("ShotgunSetPos");
		}
		/*GunStateMachine.CurrentState.ShotGunReloadAction();*/
	}
	public void MagIn()
	{
		WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.magInSound);
		/* GunStateMachine.CurrentState.MagIn()*/
	}
	public void MagOut()
	{
		WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.magOutSound);

		/*GunStateMachine.CurrentState.MagOut();*/
	}
	public virtual void ReleaseSlide()
	{
		WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.releaseSlideSound);

		/*GunStateMachine.CurrentState.ReleaseSlide();*/
	}
	public void ReloadFinish()
	{
		Anim.SetBool("IsReloading", false);
		GunStateMachine.CurrentState.ReloadFinish();
	}
}
