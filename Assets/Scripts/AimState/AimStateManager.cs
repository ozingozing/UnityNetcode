using Cinemachine;
using Invector.vCharacterController;
using QFSW.QC.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace ChocoOzing
{
	public enum GunType
	{
		M4A1,
		PumpShotGun,
	}

	public class AimStateManager : NetworkBehaviour
	{
		private const float WEIGHT_UPDATE_THRESHOLD = 0.1f;
		//TODO: Check if the current weapon type requires
		//the HipFire state and set the starting state
		//to either HipFireState or DefaultState accordingly;
		public AimBaseState currentState;
		public HipFireState Hip = new HipFireState();
		public AimState Aim = new AimState();
		public ReloadState Reload = new ReloadState();
		public DefaultState Default = new DefaultState();

		[HideInInspector] public GunBase WeaponManager;
		[HideInInspector] public GunType GunType;

		[SerializeField] private float mouseSense = 1;
		[SerializeField] public Transform camFollowPos;
		public float xAxis, yAxis;

		[HideInInspector] public Animator anim;
		 public bool IsAiming;

		public Transform aimPos;
		[HideInInspector] public Vector3 actualAimPos;
		[SerializeField] public float aimSmoothSpeed = 20;
		[SerializeField] public LayerMask aimMask;

		public Rig rig;
		public MultiAimConstraint bodyRig;
		public TwoBoneIKConstraint rHandAimTwoBone;
		public MultiAimConstraint rHandAim;
		public MultiAimConstraint headRig;

		public Transform lastAimPos;

		private void Awake()
		{
			rig = GetComponentInChildren<Rig>();
			WeaponManager = GetComponentInChildren<GunBase>();
			anim = GetComponent<Animator>();
			rHandAimTwoBone.weight = 0;
			rHandAim.weight = 0;
		}

		// Start is called before the first frame update
		void Start()
		{
			if (IsLocalPlayer)
			{
				CamManager.Instance.AdsCam.Follow = camFollowPos;
				CamManager.Instance.AdsCam.LookAt = aimPos;
				CamManager.Instance.ThirdPersonCam.Follow = camFollowPos.parent;
				CamManager.Instance.ThirdPersonCam.LookAt = aimPos;
			}
			currentState = Hip;
			SwitchState(currentState);
		}

		private void OnEnable()
		{
			CamManager.Instance.MapViewCam.Priority = 0;
			StartCoroutine(PlayerActionUpdate());
			StartCoroutine(AimLateUpdate());
		}


		IEnumerator PlayerActionUpdate()
		{
			while (true)
			{
				if (IsLocalPlayer)
				{
					Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
					Ray ray = Camera.main.ScreenPointToRay(screenCenter);
					xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
					yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
					//yAxis = Mathf.Clamp(yAxis, -80, 80);

					if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
					{
						aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
					}

					currentState.UpdateSatate(this);
				}	

				yield return null;
			}
		}
		
		IEnumerator AimLateUpdate()
		{
			while(true)
			{
				if (IsLocalPlayer)
				{
					if (IsAiming)
					{
						camFollowPos.localEulerAngles = new Vector3(yAxis, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
						transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
					}
				}
				yield return null;
			}
		}

		/*private void LateUpdate()
		{
			if (IsAiming)
			{
				camFollowPos.localEulerAngles = new Vector3(yAxis, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
			}
		}*/

		public void ShotGunReloadAction()
		{
			if (WeaponManager.ammo.currentAmmo < WeaponManager.ammo.clipSize)
				WeaponManager.ammo.ShotGunReload();
			
			if (WeaponManager.ammo.currentAmmo < WeaponManager.ammo.clipSize)
			{
				anim.Play("ShotgunReloadAction", -1, 0f);
			}
			else
			{
				anim.Play("ShotgunSetPos");
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

		public void ReleaseSlide()
		{
			WeaponManager.audioSource.PlayOneShot(WeaponManager.ammo.releaseSlideSound);
		}

		public void ReloadFinish()
		{
			anim.SetBool("GunReload", false);
		}

		public void SwitchState(AimBaseState state)
		{
			currentState.ExitState(this);
			currentState = state;
			currentState.EnterState(this);
		}
	}
}
