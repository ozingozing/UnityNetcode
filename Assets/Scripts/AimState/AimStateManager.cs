using System.Collections;
using System.Collections.Generic;
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
	[SerializeField] float aimSmoothSpeed = 20;
	[SerializeField] LayerMask aimMask;

	CheckLocalComponent checkLocalComponent;
	public MultiAimConstraint bodyRig;
	public TwoBoneIKConstraint rHandAimTwoBone;
	public MultiAimConstraint rHandAim;

	private void Awake()
	{
		checkLocalComponent = GetComponent<CheckLocalComponent>();
		rHandAimTwoBone.weight = 0;
		rHandAim.weight = 0;
	}

	// Start is called before the first frame update
	void Start()
	{
		anim = GetComponent<Animator>();
		SwitchState(Hip);
	}

	// Update is called once per frame
	void Update()
	{
		if(checkLocalComponent.IsLocalPlayer)
		{
			if (IsAiming)
			{
				xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
				yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
				yAxis = Mathf.Clamp(yAxis, -80, 80);

				Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
				Ray ray = Camera.main.ScreenPointToRay(screenCenter);

				if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimMask))
				{
					aimPos.position = Vector3.Lerp(aimPos.position, hit.point, aimSmoothSpeed * Time.deltaTime);
				}
			}

			currentState.UpdateSatate(this);
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
		bodyRig.data.offset = newOffset;
		UpdateAdsOffsetClientRpc(newOffset);
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
	public void UpdateRightHandRigWeightServerRPC(float newWeight)
	{
		rHandAim.weight = newWeight;
		rHandAimTwoBone.weight = newWeight;
		UpdateRightHandRigWeightClientRPC(newWeight);
	}

	[ClientRpc]
	public void UpdateRightHandRigWeightClientRPC(float newWeight)
	{
		if(!IsLocalPlayer)
		{
			rHandAim.weight = newWeight;
			rHandAimTwoBone.weight = newWeight;
		}
	}

	public void ChangeRightHandRigWeight(float newWeight)
	{
		rHandAim.weight = newWeight;
		rHandAimTwoBone.weight = newWeight;
	}
}
