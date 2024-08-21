using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimStateManager : MonoBehaviour
{
	AimBaseState currentState;
	public HipFireState Hip = new HipFireState();
	public AimState Aim = new AimState();

	[SerializeField] private float mouseSense = 1;
	[SerializeField] private Transform camFollowPos;
	private float xAxis, yAxis;

	[HideInInspector] public Animator anim;
	[HideInInspector] public bool IsAiming;

	// Start is called before the first frame update
	void Start()
	{
		anim = GetComponent<Animator>();
		SwitchState(Hip);
	}

	// Update is called once per frame
	void Update()
	{
		if (IsAiming)
		{
			xAxis += Input.GetAxisRaw("Mouse X") * mouseSense;
			yAxis -= Input.GetAxisRaw("Mouse Y") * mouseSense;
			yAxis = Mathf.Clamp(yAxis, -80, 80);
		}

		currentState.UpdateSatate(this);
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
}
