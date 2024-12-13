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
		//private Rigidbody rb;
		private GameObject thirdPersonCamera;
		private GameObject adsVirtualCamera;
		ClientNetworkTransform clientNetworkTransform;

		//Netcode general
		NetworkTimer neworkTimer;
		const float serverTick = 30f; //30FPS
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
		[SerializeField] private float extrapolationLimit = 0.5f; //500ms
		[SerializeField] private float extrapolationMultiplier = 1.25f;
		[SerializeField] private GameObject serverCube;
		[SerializeField] private GameObject clientCube;

		StatePayload extrapolationState;
		CountdownTimer extrapolationTimer;

		CountdownTimer reconciliationTimer;

		[Header("Netcode Debug")]
		[SerializeField] TextMeshProUGUI networkText;
		[SerializeField] TextMeshProUGUI playerText;
		[SerializeField] TextMeshProUGUI serverRpcText;
		[SerializeField] TextMeshProUGUI clientRpcText;
		//
		#endregion

		public override void OnNetworkSpawn()
		{
			if (IsOwner)
			{
				networkText.SetText($"Player {NetworkManager.LocalClientId} Host: {NetworkManager.IsHost} Server: {IsServer} Client: {IsClient}");
				if (!IsServer) serverRpcText.SetText("Not Server");
				if (!IsClient) clientRpcText.SetText("Not Client");
			}
			base.OnNetworkSpawn();
		}


		private void Awake()
		{
			//rb = GetComponent<Rigidbody>();
			thirdPersonCamera = CamManager.Instance.ThirdPersonCam.gameObject;
			adsVirtualCamera = CamManager.Instance.AdsCam.gameObject;
			clientNetworkTransform = GetComponent<ClientNetworkTransform>();

			neworkTimer = new NetworkTimer(serverTick);

			clientStateBuffer = new CircularBuffer<StatePayload>(bufferSize);
			clientInputBuffer = new CircularBuffer<InputPayload>(bufferSize);

			serverStateBuffer = new CircularBuffer<StatePayload>(bufferSize);
			serverInputQueue = new Queue<InputPayload>();

			reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
			extrapolationTimer = new CountdownTimer(extrapolationLimit);

			reconciliationTimer.OnTimerStart += () => {
				extrapolationTimer.Stop();
			};

			extrapolationTimer.OnTimerStart += () => {
				reconciliationTimer.Stop();
				SwitchAuthorityMode(AuthorityMode.Server);
			};
			extrapolationTimer.OnTimerStop += () => {
				extrapolationState = default;
				SwitchAuthorityMode(AuthorityMode.Client);
			};
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
			while (neworkTimer.ShouldTick())
			{
				HandleClientTick();
				HandleServerTick();
			}
			//Run on Update or FixedUpdate, or both - depends on the game, consider exposing on option to the editor
			Extrapolate();
		}


		protected virtual void Update()
		{
			neworkTimer.Update(Time.deltaTime);
			reconciliationTimer.Tick(Time.deltaTime);
			extrapolationTimer.Tick(Time.deltaTime);
			//Run on Update or FixedUpdate, or both - depends on the game
			Extrapolate();

			playerText.SetText($"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {cc._rigidbody.velocity.magnitude:F1}");

			if (IsOwner && IsLocalPlayer)
			{
				InputHandle();                  // update the input methods
				cc.UpdateAnimator();            // updates the Animator Parameters
			}
		}

		void SwitchAuthorityMode(AuthorityMode mode)
		{
			clientNetworkTransform.authorityMode = mode;
			bool shouldSync = mode == AuthorityMode.Client;
			/*clientNetworkTransform.SyncPositionX = shouldSync;
			clientNetworkTransform.SyncPositionY = shouldSync;
			clientNetworkTransform.SyncPositionZ = shouldSync;*/
		}

		void HandleServerTick()
		{
			if (!IsServer) return;

			var bufferIndex = -1;
			InputPayload inputPayload = default;
			while (serverInputQueue.Count > 0)
			{
				inputPayload = serverInputQueue.Dequeue();
				bufferIndex = inputPayload.tick % bufferSize;

				StatePayload statePayload = ProcessMovement(inputPayload);
				serverStateBuffer.Add(statePayload, bufferIndex);
			}

			if (bufferIndex == -1) return;
			SendToClientRpc(serverStateBuffer.Get(bufferIndex));
			HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
		}

		void Extrapolate()
		{
			if (IsServer && extrapolationTimer.IsRunning)
			{
				transform.position += extrapolationState.position.With(y: 0);
				transform.rotation = Quaternion.Slerp(transform.rotation, extrapolationState.rotation, cc.moveSpeed * Time.deltaTime); // 회전 외삽 추가
			}
		}

		void HandleExtrapolation(StatePayload latest, float latency)
		{
			if(ShouldExtrapolate(latency))
			{
				float latencyWeight = (1 + latency * extrapolationMultiplier);
				float axisLength = latencyWeight * latest.angularVelocity.magnitude * Mathf.Rad2Deg;
				Quaternion angularRotation = Quaternion.AngleAxis(axisLength, latest.angularVelocity);

				// Calculate the arc the object would traverse in degrees
				extrapolationState.position = latest.velocity * latencyWeight;
				extrapolationState.rotation = angularRotation * transform.rotation;

				extrapolationTimer.Start();
			} 
			else
			{
				extrapolationTimer.Stop();
			}
		}

		bool ShouldExtrapolate(float latency)
		{
			return latency < extrapolationLimit && latency > Time.fixedDeltaTime;
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

			var currentTick = neworkTimer.CurrentTick;
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

		static float CalculateLatencyInMillis(InputPayload inputPayload)
		{
			return (DateTime.Now - inputPayload.timestamp).Milliseconds / 1000f;
		}

		bool ShouldReconcile()
		{
			bool isNewServerState = !lastServerState.Equals(default);
			bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default)
													|| !lastProcessedState.Equals(lastServerState);
			return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning && !extrapolationTimer.IsRunning;
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

			if (positionError > reconciliationThreshold)
			{
				ReconcileState(rewindState);
				reconciliationTimer.Start();
			}

			lastProcessedState = lastServerState;
		}

		void ReconcileState(StatePayload rewindState)
		{
			transform.position = rewindState.position;
			transform.rotation = rewindState.rotation;
			cc._rigidbody.velocity = rewindState.velocity;
			cc._rigidbody.angularVelocity = rewindState.angularVelocity;

			if (!rewindState.Equals(lastServerState)) return;

			clientStateBuffer.Add(rewindState, rewindState.tick);

			//Replay all inputs fromt the rewind state to the current state
			int tickToReplay = lastServerState.tick;

			while (tickToReplay < neworkTimer.CurrentTick)
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
			if(IsOwner && IsLocalPlayer)
			{
				MOVE();
			}

			return new StatePayload()
			{
				tick = input.tick,
				networkObjectId = NetworkObjectId,
				position = transform.position,
				rotation = transform.rotation,
				velocity = cc._rigidbody.velocity,
				angularVelocity = cc._rigidbody.angularVelocity,
			};
		}

		public void MOVE()
		{
			InputHandle();                  // update the input methods
			
			cc.UpdateMotor();               // updates the ThirdPersonMotor methods
			cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
			cc.ControlRotationType();       // handle the controller rotation type

			//Vector3 forwardWithoutY = transform.forward.With(y: 0).normalized;
			//rb.velocity = Vector3.Lerp(rb.velocity, forwardWithoutY * cc.moveSpeed, neworkTimer.MinTimeBetweenTicks);
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
				cc._rigidbody.angularVelocity = Vector3.zero;
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