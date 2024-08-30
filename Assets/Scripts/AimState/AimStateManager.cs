using Cinemachine;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AimStateManager : NetworkBehaviour
{
	AimBaseState currentState;
	public HipFireState Hip = new HipFireState();
	public AimState Aim = new AimState();

	[SerializeField] private float mouseSense = 1;
	[SerializeField] private Transform camFollowPos;
	private float xAxis, yAxis;

	[HideInInspector] public Animator anim;
	[HideInInspector] public bool IsAiming;

	public Transform aimPos;
	[HideInInspector] public Vector3 actualAimPos;
	[SerializeField] public float aimSmoothSpeed = 20;
	[SerializeField] public LayerMask aimMask;

	public CheckLocalComponent checkLocalComponent;
	public MultiAimConstraint bodyRig;
	public TwoBoneIKConstraint rHandAimTwoBone;
	public MultiAimConstraint rHandAim;
	public MultiAimConstraint headRig;

	private const float WEIGHT_UPDATE_THRESHOLD = 0.1f;

	private void Awake()
	{	
		checkLocalComponent = GetComponent<CheckLocalComponent>();
		rHandAimTwoBone.weight = 0;
		rHandAim.weight = 0;
	}

	// Start is called before the first frame update
	void Start()
	{
		if (IsLocalPlayer)
		{
			AdsCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = camFollowPos;
			AdsCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = camFollowPos;
			ThirdPersonCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().Follow = camFollowPos;
			ThirdPersonCamera.Instance.gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = camFollowPos;
		}
		anim = GetComponent<Animator>();
		SwitchState(Hip);
		StartCoroutine(PlayerActionUpdate());
	}

	IEnumerator PlayerActionUpdate()
	{
		while (true)
		{
			if (checkLocalComponent.IsLocalPlayer)
			{
				Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
				Ray ray = Camera.main.ScreenPointToRay(screenCenter);
				xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
				yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
				yAxis = Mathf.Clamp(yAxis, -80, 80);

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

	public void SwitchState(AimBaseState state)
	{
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
		if (!checkLocalComponent.IsLocalPlayer)
		{
			bodyRig.data.offset = newOffset;
		}
	}

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
