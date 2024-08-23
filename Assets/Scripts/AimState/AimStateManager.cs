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

	[SerializeField] Transform aimPos;
	[SerializeField] float aimSmoothSpeed = 20;
	[SerializeField] LayerMask aimMask;

	CheckLocalComponent checkLocalComponent;
	[SerializeField] public MultiAimConstraint bodyRig;
	public Vector3 targetOffset;
	public float rotationSpeed = 5f;  // 회전 속도

	private void Awake()
	{
		checkLocalComponent = GetComponent<CheckLocalComponent>();
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
	public void UpdateOffsetServerRpc(Vector3 newOffset)
	{
		bodyRig.data.offset = newOffset;
		UpdateOffsetClientRpc(newOffset);
	}

	[ClientRpc]
	public void UpdateOffsetClientRpc(Vector3 newOffset)
	{
		if (!IsLocalPlayer)
		{
			bodyRig.data.offset = newOffset;
		}
	}
}
