using ChocoOzing;
using ChocoOzing.Network;
using ChocoOzing.Utilities;
using Cinemachine;
using System;
using System.Collections.Generic;
using TMPro;
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
		private GameObject thirdPersonCamera;
		private GameObject adsVirtualCamera;

		//Netcode general
		NetworkTimer timer;
		const float serverTick = 60f; //60FPS
		const int bufferSize = 1024;

		//Netcode client specific
		CircularBuffer<StatePayload> clientStateBuffer;
		CircularBuffer<InputPayload> clientInputBuffer;
		StatePayload lastServerState;
		StatePayload lastProcessedState;

		//Netcode server specific
		CircularBuffer<StatePayload> serverStateBuffer;
		Queue<InputPayload> serverInputQueue;

		[Header("Netcode")]
		[SerializeField] private float reconciliationCooldownTime = 1f;
		[SerializeField] private float reconciliationThreshold = 10f;
		[SerializeField] private GameObject serverCube;
		[SerializeField] private GameObject clientCube;

		CountdownTimer reconciliationCooldown;

		[Header("Netcode Debug")]
		[SerializeField] TextMeshProUGUI networkText;
		[SerializeField] TextMeshProUGUI playerText;
		[SerializeField] TextMeshProUGUI serverRpcText;
		[SerializeField] TextMeshProUGUI clientRpcText;
		//
		#endregion

		public override void OnNetworkSpawn()
		{
			if(IsOwner)
			{
				networkText.SetText($"Player {NetworkManager.LocalClientId} Host: {NetworkManager.IsHost} Server: {IsServer} Client: {IsClient}");
				if (!IsServer) serverRpcText.SetText("Not Server");
				if (!IsClient) clientRpcText.SetText("Not Client");
			}
			base.OnNetworkSpawn();
		}


		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
			thirdPersonCamera = CamManager.Instance.ThirdPersonCam.gameObject;
			adsVirtualCamera = CamManager.Instance.AdsCam.gameObject;

			timer = new NetworkTimer(serverTick);
			
			clientStateBuffer = new CircularBuffer<StatePayload>(bufferSize);
			clientInputBuffer = new CircularBuffer<InputPayload>(bufferSize);
			
			serverStateBuffer = new CircularBuffer<StatePayload>(bufferSize);
			serverInputQueue = new Queue<InputPayload>();

			reconciliationCooldown = new CountdownTimer(reconciliationCooldownTime);
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
			while(timer.ShouldTick())
			{
				HandleClientTick();
				HandleServerTick();
			}
		}


		protected virtual void Update()
		{
			timer.Update(Time.deltaTime);
			reconciliationCooldown.Tick(Time.deltaTime);
			
			playerText.SetText($"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {rb.velocity.magnitude:F1}");

			if (IsOwner)
			{
				InputHandle();                  // update the input methods
				cc.UpdateAnimator();            // updates the Animator Parameters
			}
		}

		void HandleServerTick()
		{
			if (!IsServer) return;

			var bufferIndex = -1;
			while(serverInputQueue.Count > 0)
			{
				InputPayload inputPayload = serverInputQueue.Dequeue();
				bufferIndex = inputPayload.tick % bufferSize;

				StatePayload statePayload = ProcessMovement(inputPayload);
				serverStateBuffer.Add(statePayload, bufferIndex);
			}

			if (bufferIndex == -1) return;
			SendToClientRpc(serverStateBuffer.Get(bufferIndex));
		}


		[ClientRpc]
		void SendToClientRpc(StatePayload statePayload)
		{
			clientRpcText.SetText($"Received state from server Tick {statePayload.tick} Server POS: {statePayload.position}");
			serverCube.transform.position = statePayload.position.With();
			if (!IsOwner) return;
			lastServerState = statePayload;
		}

		void HandleClientTick()
		{
			if (!IsClient || !IsOwner) return;

			var currentTick = timer.CurrentTick;
			var bufferIndex = currentTick % bufferSize;

			InputPayload inputPayload = new InputPayload()
			{
				tick = currentTick,
				timestamp = DateTime.Now,
				networkObjectId = NetworkObjectId,
				inputVector = cc.moveDirection,
				position = transform.position,
			};

			clientInputBuffer.Add(inputPayload, bufferIndex);
			SendToServerRpc(inputPayload);

			StatePayload statePayload = ProcessMovement(inputPayload);
			clientStateBuffer.Add(statePayload, bufferIndex);

			HandleServerReconciliation();
		}

		bool ShouldReconcile()
		{
			bool isNewServerState = !lastServerState.Equals(default);
			bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
													|| !lastProcessedState.Equals(lastServerState);
			return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationCooldown.IsRunning;
		}
		void HandleServerReconciliation()
		{
			if (!ShouldReconcile()) return;

			float positionError;
			int bufferIndex = lastServerState.tick % bufferSize;

			if (bufferIndex - 1 < 0) return;
			//Host RPCs excute immediately, so we can use the last server state
			StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;
			StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
			positionError = Vector3.Distance(rewindState.position, clientState.position);

			if(positionError > reconciliationThreshold)
			{
				ReconcileState(rewindState);
				reconciliationCooldown.Start();
			}

			lastProcessedState = lastServerState;
		}

		void ReconcileState(StatePayload rewindState)
		{
			transform.position = rewindState.position;
			transform.rotation = rewindState.rotation;
			rb.velocity = rewindState.velocity;
			rb.angularVelocity = rewindState.angularVelocity;

			if (!rewindState.Equals(lastServerState)) return;

			clientStateBuffer.Add(rewindState, rewindState.tick);

			//Replay all inputs fromt the rewind state to the current state
			int tickToReplay = lastServerState.tick;

			while(tickToReplay < timer.CurrentTick)
			{
				int bufferIndex = tickToReplay % bufferSize;
				StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
				clientStateBuffer.Add(statePayload, bufferIndex);
				tickToReplay++;
			}
		}

		[ServerRpc]
		void SendToServerRpc(InputPayload input)
		{
			serverRpcText.SetText($"Received input from client Tick: {input.tick} Client POS: {input.position}");
			clientCube.transform.position = input.position.With();
			serverInputQueue.Enqueue(input);
		}

		StatePayload ProcessMovement(InputPayload input)
		{
			MOVE();

			return new StatePayload()
			{
				tick = input.tick,
				networkObjectId = NetworkObjectId,
				position = transform.position,
				rotation = transform.rotation,
				velocity = rb.velocity,
				angularVelocity = rb.velocity
			};
		}

		public void MOVE()
		{
			cc.UpdateMotor();               // updates the ThirdPersonMotor methods
			cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
			cc.ControlRotationType();       // handle the controller rotation type

			Vector3 forwardWithoutY = transform.forward.With(y: 0).normalized;
			rb.velocity = Vector3.Lerp(rb.velocity, forwardWithoutY * cc.input.z, timer.MinTimeBetweenTicks);
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
				if (tpCamera && IsLocalPlayer)
				{
					tpCamera.SetMainTarget(transform);
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
				//movementStateManager.PlayerAdsMove();
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

	}

	//Network variables should be value objects
	public struct InputPayload : INetworkSerializable
	{
		public int tick;
		public DateTime timestamp;
		public ulong networkObjectId;
		public Vector3 inputVector;
		public Vector3 position;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref tick);
			serializer.SerializeValue(ref timestamp);
			serializer.SerializeValue(ref networkObjectId);
			serializer.SerializeValue(ref inputVector);
			serializer.SerializeValue(ref position);
		}
	}

	public struct StatePayload : INetworkSerializable
	{
		public int tick;
		public ulong networkObjectId;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 velocity;
		public Vector3 angularVelocity;
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref tick);
			serializer.SerializeValue(ref networkObjectId);
			serializer.SerializeValue(ref position);
			serializer.SerializeValue(ref rotation);
			serializer.SerializeValue(ref velocity);
			serializer.SerializeValue(ref angularVelocity);
		}
	}
}