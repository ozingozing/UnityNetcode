using Cinemachine;
using Invector.vCharacterController;
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
	public class AimStateManager : NetworkBehaviour
	{
		private const float WEIGHT_UPDATE_THRESHOLD = 0.1f;

		public AimBaseState currentState;
		public HipFireState Hip = new HipFireState();
		public AimState Aim = new AimState();
		public ReloadState Reload = new ReloadState();
		public DefaultState Default = new DefaultState();

		public WeaponManager WeaponManager;

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
			WeaponManager = GetComponentInChildren<WeaponManager>();
			rHandAimTwoBone.weight = 0;
			rHandAim.weight = 0;
		}

		// Start is called before the first frame update
		void Start()
		{
			if (IsLocalPlayer)
			{
				//TODO: Cam Target Fix!!!!WTF
				AdsCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = camFollowPos;
				AdsCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = aimPos;
				ThirdPersonCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = camFollowPos.parent;
				ThirdPersonCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = aimPos;
			}
			anim = GetComponent<Animator>();
			currentState = Hip;
			SwitchState(currentState);
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

		public void SwitchState(AimBaseState state)
		{
			currentState.ExitState(this);
			currentState = state;
			currentState.EnterState(this);
		}

		[ServerRpc]
		public void UpdateAdsOffsetServerRpc(Vector3 newOffset)
		{
			if (bodyRig.data.offset != newOffset)
			{
				bodyRig.data.offset = newOffset;
				UpdateAdsOffsetClientRpc(newOffset);
			}
		}
		
		[ClientRpc]
		public void UpdateAdsOffsetClientRpc(Vector3 newOffset)
		{
			if (!IsLocalPlayer)
			{
				bodyRig.data.offset = newOffset;
			}
		}

		[ServerRpc]
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
