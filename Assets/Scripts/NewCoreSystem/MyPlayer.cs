using Architecture.AbilitySystem.Model;
using ChocoOzing.CoreSystem;
using ChocoOzing.EventBusSystem;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public enum GunType
{
	M4A1,
	PumpShotGun,
}

public class MyPlayer : MonoBehaviour, IEntity
{ 
	// 지연 초기화
	/// <summary>
	/// if you want use Rigidbody? use this
	/// UNT0008 Null propagation on Unity objects
	/// Don't use [?.]
	/// </summary>
	protected Movement Movement
	{ get => movement = (movement != null) ? movement : Core.GetCoreComponent(ref movement); }
	private Movement movement;

	public AnimationManager AnimationManager
	{ get => animationManager ??= new AnimationManager(Anim); }
	private AnimationManager animationManager;

	public MyPlayer Player
	{  get => player = (player != null) ? player : this; }
	private MyPlayer player;

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
	public PlayerStateMachine GunStateMachine { get; private set; }
	
	public IdleState IdleState { get; private set; }
	public WalkState WalkState { get; private set; }
	public RunState RunState { get; private set; }

	public PlayerAbilityState PlayerAbilityState { get; private set; }
	
	public HipFireState HipFireState { get; private set; }
	public AimState AimState { get; private set; }
	public ReloadState ReloadState { get; private set; }
	public DefaultState DefaultState { get; private set; }
	#endregion

	#region Values
	public Observer<bool> IsMoveLock = new Observer<bool>(false);
	#endregion

	private void OnEnable()
	{
		CamManager.Instance.MapViewCam.Priority = 0;
	}

	private void Awake()
	{
		Anim = GetComponent<Animator>();
		MovementCollider = GetComponent<CapsuleCollider>();

		Core = GetComponentInChildren<Core>();

		StateMachine = new PlayerStateMachine();
		GunStateMachine = new PlayerStateMachine();
		
		IdleState = new IdleState(this, StateMachine, "Idle");
		WalkState = new WalkState(this, StateMachine, "IsWalking");
		RunState = new RunState(this, StateMachine, "IsRunning");

		PlayerAbilityState = new PlayerAbilityState(this, StateMachine, "IsAbility");

		HipFireState = new HipFireState(this, GunStateMachine, "IsHipfiring");
		AimState = new AimState(this, GunStateMachine, "IsAiming");
		ReloadState = new ReloadState(this, GunStateMachine, "_");
	}

	private void Start()
	{
		StateMachine.Initialize(IdleState);
		GunStateMachine.Initialize(HipFireState);
	}

	public void PlayerActionStart()
	{
		LocalPlayerInit();

		StartCoroutine(UpdateCoroutine());
		StartCoroutine(FixedUpdateCoroutine());
	}

	EventBinding<PlayerAnimationEvent> eventBinding;
	private void LocalPlayerInit()
	{
		eventBinding = new EventBinding<PlayerAnimationEvent>(SkillAction);
		EventBus<PlayerAnimationEvent>.Register(eventBinding);
		IsMoveLock.AddListener(Movement.StopMove);
	}

	private void OnDestroy()
	{
		if (eventBinding != null)
		{
			eventBinding.Remove(SkillAction);
			EventBus<PlayerAnimationEvent>.Deregister(eventBinding);
			eventBinding = null;
		}
		IsMoveLock.Dispose();
	}

	float crossFadeValue = 0.1f;
	public void SkillAction(PlayerAnimationEvent @event)
	{
		IsMoveLock.Set(@event.abilityData.moveLock);
		Anim.CrossFade(@event.abilityData.animationHash, crossFadeValue);
		StateMachine.ChangeState(PlayerAbilityState, @event);
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

	public void MagIn()
	{
		WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.magInSound);
	}

	public void MagOut()
	{
		WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.magOutSound);
	}

	public virtual void ReleaseSlide()
	{
		WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.releaseSlideSound);
	}

	public void AnimationFinishTrigger()
	{
		GunStateMachine.CurrentState.AnimationFinishTrigger();
	}
}