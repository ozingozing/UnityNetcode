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
			rHandAimTwoBone.weight = 0;
			rHandAim.weight = 0;
		}

		// Start is called before the first frame update
		void Start()
		{
			if (IsLocalPlayer)
			{
				AdsCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = camFollowPos;
				AdsCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = aimPos;
				ThirdPersonCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = camFollowPos.parent;
				ThirdPersonCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = aimPos;
			}
			anim = GetComponent<Animator>();
			currentState = Hip;
			SwitchState(currentState);
		}

		private void OnEnable()
		{
			StartCoroutine(PlayerActionUpdate());
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

				if(IsServer)
				{
					if(Input.GetKeyDown(KeyCode.M))
					{
						GameObject.Find("MapManager").GetComponent<WalkerGenerator>().InitializeGrid();
					}
				}	

				yield return null;
			}
		}

		private void LateUpdate()
		{
			if (IsAiming)
			{
				camFollowPos.localEulerAngles = new Vector3(yAxis, camFollowPos.localEulerAngles.y, camFollowPos.localEulerAngles.z);
				transform.eulerAngles = new Vector3(transform.eulerAngles.x, xAxis, transform.eulerAngles.z);
			}
		}

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

		[ServerRpc]
		public void UpdateAdsOffsetServerRpc(float newOffset)
		{
			if(Mathf.Abs(bodyRig.data.offset.y - newOffset) > 0.1f)
			{
				bodyRig.data.offset = new Vector3(0, newOffset, 0);
				UpdateAdsOffsetClientRpc(newOffset);
			}
		}
		
		[ClientRpc]
		public void UpdateAdsOffsetClientRpc(float newOffset)
		{
			bodyRig.data.offset = new Vector3(0, newOffset, 0);
		}

		[ServerRpc]
		public void UpdateRigWeightServerRPC(float newWeight)
		{
			if(Mathf.Abs(rig.weight - newWeight) > 0.5f)
			{
				rig.weight = newWeight;
				UpdateRigWeightClientRPC(newWeight);
			}
		}
		[ClientRpc]
		public void UpdateRigWeightClientRPC(float newWeight)
		{
			rig.weight = newWeight;
		}

		/*
		 * This is an issue with this Func
		 * It calls ServerRpc, followed by CLientRpc.
		 * When I Set the value to cahane in ClientRpc when
		 * !IsOwner, the changed values is not apllied to other clients;
		 */
		/*[ServerRpc]
		public void UpdateRigWeightServerRPC(float newWeight)
		{
			if (rig.weight != newWeight) // 값이 달라졌을 때만 처리
			{
				rig.weight = newWeight;
				UpdateRigWeightClientRPC(newWeight);
			}
		}

		[ClientRpc]
		public void UpdateRigWeightClientRPC(float newWeight)
		{
			if(!IsOwner)
				rig.weight = newWeight;
		}*/

		[ServerRpc]
		public void UpdateRightHandRigWeightServerRPC(float newWeight)
		{
			if (Mathf.Abs(rHandAim.weight - newWeight) > WEIGHT_UPDATE_THRESHOLD ||
			Mathf.Abs(rHandAimTwoBone.weight - newWeight) > WEIGHT_UPDATE_THRESHOLD ||
			Mathf.Abs(bodyRig.weight - newWeight) > WEIGHT_UPDATE_THRESHOLD)
			{
				rHandAim.weight = newWeight;
				rHandAimTwoBone.weight = newWeight;
				bodyRig.weight = newWeight;
				UpdateRightHandRigWeightClientRPC(newWeight);
			}
			
		}

		[ClientRpc]
		public void UpdateRightHandRigWeightClientRPC(float newWeight)
		{
			rHandAim.weight = newWeight;
			rHandAimTwoBone.weight = newWeight;
			bodyRig.weight = newWeight;
		}
	}
}
