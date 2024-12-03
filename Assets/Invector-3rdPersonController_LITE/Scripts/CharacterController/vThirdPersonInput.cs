using ChocoOzing;
using Cinemachine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Invector.vCharacterController
{
	public class vThirdPersonInput : NetworkBehaviour
	{
		#region Variables       

		[Header("Controller Input")]
		public string horizontalInput = "Horizontal";
		public string verticallInput = "Vertical";
		public KeyCode jumpInput = KeyCode.Space;
		public KeyCode strafeInput = KeyCode.Tab;
		public KeyCode sprintInput = KeyCode.LeftShift;

		[Header("Camera Input")]
		public string rotateCameraXInput = "Mouse X";
		public string rotateCameraYInput = "Mouse Y";

		[HideInInspector] public vThirdPersonController cc;
		[HideInInspector] public vThirdPersonCamera tpCamera;
		[HideInInspector] public Camera cameraMain;

		//추가
		private Rigidbody rb;
		private MovementStateManager movementStateManager;
		private AimStateManager aimStateManager;
		private GameObject thirdPersonCamera;
		private GameObject adsVirtualCamera;
		//
		#endregion

		//Both Client And Server Specific
		[SerializeField] private int tickRate = 60;
		[SerializeField] int currentTick;
		private float time;
		private float tickTime;

		//Client Specific
		private const int BUFFERSIZE = 1024;
		[SerializeField] MovementData[] clientMovementDatas = new MovementData[BUFFERSIZE];

		//Server Specific
		[SerializeField] private float maxPositionError = 0.5f;
		private float lastReceivedServerTime = 0; // 서버로부터 받은 마지막 데이터의 시간
		private const float extrapolationThreshold = 0.05f; // 외삽을 적용할 최소 시간 (100ms 이상 지연되었을 때)

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
			movementStateManager = GetComponent<MovementStateManager>();
			aimStateManager = GetComponent<AimStateManager>();
			thirdPersonCamera = CamManager.Instance.ThirdPersonCam.gameObject;
			adsVirtualCamera = CamManager.Instance.AdsCam.gameObject;

			time = 1f / tickRate;
		}

		protected virtual void Start()
		{
			InitializeTpCamera();
		}

		protected virtual void OnEnable()
		{
			InitilizeController();
		}

		protected virtual void FixedUpdate()
		{
			if (!IsClient || !IsOwner) return;

			if (time > tickTime)
			{
				currentTick++;
				time -= tickTime;

				if(currentTick <= 2)
				{
					cc.UpdateMotor();               // updates the ThirdPersonMotor methods
					cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
					cc.ControlRotationType();       // handle the controller rotation type
				}
				MOVE();
			} 
		}

		public void MOVE()
		{
			float deltaTime = Time.time - lastReceivedServerTime; // 현재 시간에서 마지막으로 받은 서버 데이터 시간 차이
			// 서버로부터 받은 위치와 회전 보간
			clientMovementDatas[currentTick % BUFFERSIZE] = new MovementData
			{
				tick = currentTick,
				position = transform.position,
				rotation = transform.rotation,
				rbVelocity = cc._rigidbody.velocity,
				angularVelocity = cc._rigidbody.angularVelocity,
			};

			// 현재 서버에서 받은 데이터를 기준으로 외삽
			if (currentTick > 2)
			{
				// 보간처리
				MoveServerRPC(clientMovementDatas[currentTick % BUFFERSIZE], clientMovementDatas[(currentTick - 1) % BUFFERSIZE]);

				// 서버로부터 Rpc받은 시간이
				// extrapolationThreshold보다 경과가 지났을 경우 외삽처리
				if (deltaTime > extrapolationThreshold)
				{
					cc.UpdateMotor();               // updates the ThirdPersonMotor methods
					cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
					cc.ControlRotationType();       // handle the controller rotation type
				}

				currentTick = 0;
			}
		}

		// 보간처리
		/*public void MOVE()
		{
			clientMovementDatas[currentTick % BUFFERSIZE] = new MovementData
			{
				tick = currentTick,
				position = transform.position,
				rotation = transform.rotation,
				rbVelocity = cc._rigidbody.velocity,
				angularVelocity = cc._rigidbody.angularVelocity,
			};

			if (currentTick < 2) return;
			else
			{
				MoveServerRPC(clientMovementDatas[currentTick % BUFFERSIZE], clientMovementDatas[(currentTick - 1) % BUFFERSIZE]);
				currentTick = 0;
			}
		}*/

		protected virtual void Update()
		{
			time += Time.deltaTime;
			InputHandle();                  // update the input methods
			cc.UpdateAnimator();            // updates the Animator Parameters
		}

		public virtual void OnAnimatorMove()
		{
			cc.ControlAnimatorRootMotion(); // handle root motion animations 
		}

		#region Basic Locomotion Inputs

		protected virtual void InitilizeController()
		{
			cc = GetComponent<vThirdPersonController>();

			if (cc != null)
				cc.Init();
		}

		protected virtual void InitializeTpCamera()
		{
			if (tpCamera == null)
			{
				tpCamera = FindObjectOfType<vThirdPersonCamera>();
				if (tpCamera == null)
					return;
				if (tpCamera)
				{
					tpCamera.SetMainTarget(aimStateManager.transform);
					tpCamera.Init();
				}
			}
		}

		protected virtual void InputHandle()
		{
			if (!Input.GetKey(KeyCode.Mouse1))
			{
				cc.isStrafing = false;
				thirdPersonCamera.gameObject.SetActive(true);
				adsVirtualCamera.gameObject.SetActive(false);
			}
			else
			{
				cc.isStrafing = true;
				thirdPersonCamera.gameObject.SetActive(false);
				adsVirtualCamera.gameObject.SetActive(true);
				movementStateManager.PlayerAdsMove();
			}
			MoveInput();
			CameraInput();
			SprintInput();
			StrafeInput();
			JumpInput();
		}

		public virtual void MoveInput()
		{
			cc.input.x = Input.GetAxis(horizontalInput);
			cc.input.z = Input.GetAxis(verticallInput);

			if (!Input.GetButtonDown(horizontalInput) || !Input.GetButtonDown(verticallInput))
			{
				rb.angularVelocity = Vector3.zero;
			}
		}

		protected virtual void CameraInput()
		{
			if (!cameraMain)
			{
				if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
				else
				{
					cameraMain = Camera.main;
					cc.rotateTarget = cameraMain.transform;
				}
			}

			if (cameraMain)
			{
				cc.UpdateMoveDirection(cameraMain.transform);
			}

			if (tpCamera == null)
				return;

			var Y = Input.GetAxis(rotateCameraYInput);
			var X = Input.GetAxis(rotateCameraXInput);

			tpCamera.RotateCamera(X, Y);
		}

		protected virtual void StrafeInput()
		{
			if (Input.GetKeyDown(strafeInput))
				cc.Strafe();
		}

		protected virtual void SprintInput()
		{
			if (Input.GetKeyDown(sprintInput))
				cc.Sprint(true);
			else if (Input.GetKeyUp(sprintInput))
				cc.Sprint(false);
		}

		protected virtual bool JumpConditions()
		{
			return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
		}

		protected virtual void JumpInput()
		{
			if (Input.GetKeyDown(jumpInput) && JumpConditions())
				cc.Jump();
		}

		#endregion

		[ServerRpc]
		private void MoveServerRPC(MovementData currentMovementData, MovementData lastMovementData)
		{
			if (Vector3.Distance(lastMovementData.position, currentMovementData.position) > maxPositionError)
			{
				//Debug.Log("Position Error!!!");
				//Debug.Log("Start!!! ClientSidePrediction / ServerReconciliation");
				ReconciliateClientRPC(currentMovementData.tick);
			}
		}

		[ClientRpc]
		private void ReconciliateClientRPC(int activationTick)
		{
			// 서버가 클라이언트에게 언제 데이터를 보냈는지 기록 (외삽을 위한 기준)
			lastReceivedServerTime = Time.time;

			// 올바른 위치, 속도, 회전 데이터를 사용하여 보정
			Vector3 correctPosition = clientMovementDatas[(activationTick - 1) % BUFFERSIZE].position;
			Vector3 correctRbVelocity = clientMovementDatas[(activationTick - 1) % BUFFERSIZE].rbVelocity;
			Vector3 correctAngularVelocity = clientMovementDatas[(activationTick - 1) % BUFFERSIZE].angularVelocity;
			Quaternion correctRotation = clientMovementDatas[(activationTick - 1) % BUFFERSIZE].rotation;

			// 보간 속도
			float positionLerpSpeed = 10f;
			float rotationLerpSpeed = 180f;

			// 위치 보간: SmoothDamp
			float smoothDampSpeed = 3f;  // SmoothDamp에 적용할 속도
			Vector3 velocity = Vector3.zero;  // SmoothDamp의 속도를 제어할 변수
			Physics.simulationMode = SimulationMode.Script;
			transform.position = Vector3.SmoothDamp(transform.position, correctPosition, ref velocity, smoothDampSpeed * Time.fixedDeltaTime);

			// 속도 보간
			rb.velocity = Vector3.Lerp(rb.velocity, correctRbVelocity, positionLerpSpeed * Time.fixedDeltaTime);

			// 각속도 보간
			rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, correctAngularVelocity, positionLerpSpeed * Time.fixedDeltaTime);

			// 회전 보간: Slerp
			transform.rotation = Quaternion.Slerp(transform.rotation, correctRotation, rotationLerpSpeed * Time.fixedDeltaTime);

			// 고정된 업데이트로 돌아감
			Physics.simulationMode = SimulationMode.FixedUpdate;

			// 보정 후 새로운 이동 데이터를 업데이트
			clientMovementDatas[activationTick % BUFFERSIZE].position = transform.position;
			clientMovementDatas[activationTick % BUFFERSIZE].rotation = transform.rotation;
			clientMovementDatas[activationTick % BUFFERSIZE].angularVelocity = rb.angularVelocity; // 각속도 보정
			clientMovementDatas[activationTick % BUFFERSIZE].rbVelocity = rb.velocity; // 물리 속도 보정
		}
	}
}

[System.Serializable]
public class MovementData : INetworkSerializable
{
	public int tick;
	public Vector3 position;
	public Vector3 rbVelocity;
	public Vector3 angularVelocity;
	public Quaternion rotation;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref tick);
		serializer.SerializeValue(ref position);
		serializer.SerializeValue(ref rbVelocity);
		serializer.SerializeValue(ref angularVelocity);
		serializer.SerializeValue(ref rotation);
	}
}
