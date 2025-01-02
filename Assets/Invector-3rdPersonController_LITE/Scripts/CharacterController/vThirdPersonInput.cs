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
		//[SerializeField] private float extrapolationMultiplier = 1.25f;
		[SerializeField] private GameObject serverCube;
		[SerializeField] private GameObject clientCube;

		StatePayload extrapolationState;
		//CountdownTimer extrapolationTimer;
		CountdownTimer reconciliationTimer;

		[Header("Netcode Debug")]
		[SerializeField] TextMeshProUGUI networkText;
		[SerializeField] TextMeshProUGUI playerText;
		[SerializeField] TextMeshProUGUI serverRpcText;
		[SerializeField] TextMeshProUGUI clientRpcText;

		public ulong thisClientId;
		public bool isDebug = false;
		public bool ValueCorrection;
		const float MAX = 1000f;
		const float MIN = -1000f;
		const float PRESICION = 0.01f;
		//
		#endregion

		public override void OnNetworkSpawn()
		{
			if(!isDebug)
			{
				clientCube.SetActive(false);
				serverCube.SetActive(false);
				networkText.gameObject.SetActive(false);
				playerText.gameObject.SetActive(false);
				serverRpcText.gameObject.SetActive(false);
				clientRpcText.gameObject.SetActive(false);
			}
			if (IsOwner && isDebug)
			{
				networkText.SetText($"Player {NetworkManager.LocalClientId} Host: {NetworkManager.IsHost} Server: {IsServer} Client: {IsClient}");
				if (!IsServer) serverRpcText.SetText("Not Server");
				if (!IsClient) clientRpcText.SetText("Not Client");
			}
			thisClientId = GetComponent<NetworkObject>().OwnerClientId;
			base.OnNetworkSpawn();
		}


		private void Awake()
		{
			thirdPersonCamera = CamManager.Instance.ThirdPersonCam.gameObject;
			adsVirtualCamera = CamManager.Instance.AdsCam.gameObject;
			clientNetworkTransform = GetComponent<ClientNetworkTransform>();

			neworkTimer = new NetworkTimer(serverTick);

			clientStateBuffer = new CircularBuffer<StatePayload>(bufferSize);
			clientInputBuffer = new CircularBuffer<InputPayload>(bufferSize);

			serverStateBuffer = new CircularBuffer<StatePayload>(bufferSize);
			serverInputQueue = new Queue<InputPayload>();

			reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);
			//extrapolationTimer = new CountdownTimer(extrapolationLimit);

			reconciliationTimer.OnTimerStart += () => {
				//extrapolationTimer.Stop();
			};

			/*extrapolationTimer.OnTimerStart += () => {
				reconciliationTimer.Stop();
				//SwitchAuthorityMode(AuthorityMode.Server);
			};
			extrapolationTimer.OnTimerStop += () => {
				extrapolationState = default;
				//SwitchAuthorityMode(AuthorityMode.Client);
			};*/
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
				if(IsLocalPlayer)
					HandleClientTick();
				if(IsServer)
					HandleServerTick();
			}
			if (IsLocalPlayer)
			{
				MOVE();
			}
			//Run on Update or FixedUpdate, or both - depends on the game, consider exposing on option to the editor
			//Extrapolate();
		}


		protected virtual void Update()
		{
			neworkTimer.Update(Time.deltaTime);
			reconciliationTimer.Tick(Time.deltaTime);
			//extrapolationTimer.Tick(Time.deltaTime);
			//Run on Update or FixedUpdate, or both - depends on the game
			//Extrapolate();
			if(isDebug)
				playerText.SetText($"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {cc._rigidbody.velocity.magnitude:F1}");

			if (IsLocalPlayer)
			{
				InputHandle();                  // update the input methods
				cc.UpdateAnimator();            // updates the Animator Parameters
			}
		}

		void SwitchAuthorityMode(AuthorityMode mode)
		{
			clientNetworkTransform.authorityMode = mode;
			bool shouldSync = mode == AuthorityMode.Client;
			clientNetworkTransform.SyncPositionX = shouldSync;
			clientNetworkTransform.SyncPositionY = shouldSync;
			clientNetworkTransform.SyncPositionZ = shouldSync;
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
				statePayload = HandleExtrapolation(statePayload, bufferIndex);
				serverStateBuffer.Add(statePayload, bufferIndex);
			}

			if (bufferIndex == -1) return;
			SendToClientRpc(serverStateBuffer.Get(bufferIndex));
			//HandleExtrapolation(serverStateBuffer.Get(bufferIndex), CalculateLatencyInMillis(inputPayload));
		}

		void Extrapolate()
		{
			if (!IsOwner && IsServer /*&& extrapolationTimer.IsRunning*/)
			{
				var playerObject = GetComponent<NetworkObject>();
				if (extrapolationState.networkObjectId == playerObject.OwnerClientId)
				{
					ulong originClientId = playerObject.OwnerClientId;
					playerObject.ChangeOwnership(NetworkManager.ServerClientId);
					transform.position += extrapolationState.position.With(y: 0);
					transform.rotation *= Quaternion.Slerp(transform.rotation, extrapolationState.rotation, 360f * Time.deltaTime); // 회전 외삽 추가
					playerObject.ChangeOwnership(originClientId);
				}
			}
		}

		StatePayload HandleExtrapolation(StatePayload latest, float latency)
		{
			if (!ValueCorrection)
				return latest;

			if (ShouldExtrapolate(latency))
			{
				//float latencyWeight = 1 + latency * 1.2f;
				float latencyWeight = latency;
				latest.velocity = latest.velocity * latencyWeight;
				latest.position += latest.velocity * latencyWeight;
				latest.rotation *= Quaternion.AngleAxis(
						latencyWeight * latest.velocity.magnitude * Mathf.Rad2Deg,
						Vector3.up);

				if( IsServer &&
					!IsOwner &&
					latest.networkObjectId == thisClientId)
				{
					transform.rotation *= Quaternion.Slerp(transform.rotation, latest.rotation, 360f * Time.deltaTime);
					transform.position += latest.velocity;
				}
			}
			return latest;
		}

		bool ShouldExtrapolate(float latency)
		{
			return (latency < 0.1f) ? false :  latency < extrapolationLimit && latency > Time.fixedDeltaTime;
		}

		[ClientRpc]
		void SendToClientRpc(StatePayload statePayload)
		{
			if (isDebug)
			{
				clientRpcText.SetText($"Received state from server Tick {statePayload.tick} Server POS: {statePayload.position}");
				serverCube.transform.position = statePayload.position;
			}
			lastServerState = statePayload;
		}

		void HandleClientTick()
		{
			var currentTick = neworkTimer.CurrentTick;
			var bufferIndex = currentTick % bufferSize;


			InputPayload inputPayload = new InputPayload()
			{
				tick = currentTick,
				timestamp = DateTime.Now,
				networkObjectId = NetworkManager.Singleton.LocalClientId,
				inputVector = PackVector3(cc.moveDirection),
				position = PackVector3(transform.position),
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
			return isNewServerState && isLastStateUndefinedOrDifferent && !reconciliationTimer.IsRunning /*&& !extrapolationTimer.IsRunning*/;
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
			if(isDebug)
			{
				serverRpcText.SetText($"Received input from client Tick: {input.tick} Client POS: {input.position}");
				clientCube.transform.position = UnpackVector3(input.position);
			}
			serverInputQueue.Enqueue(input);
		}

		StatePayload ProcessMovement(InputPayload input)
		{
			if (IsServer &&
				!IsOwner &&
				input.networkObjectId == thisClientId &&
				ValueCorrection)
			{
				cc.input = UnpackVector3(input.inputVector).With(y: 0);
				if (cc.input.sqrMagnitude > 0.001f)
				{
					Vector3 targetDirection = cc.input.normalized;

					transform.forward = Vector3.Slerp(
						transform.forward,
						targetDirection,
						360f * Time.deltaTime
					);
				}
				transform.position = Vector3.Lerp(transform.position, UnpackVector3(input.position), 10 * Time.deltaTime);
			}

			return new StatePayload()
			{
				tick = input.tick,
				networkObjectId = input.networkObjectId,
				position = transform.position,
				rotation = transform.rotation,
				velocity = cc._rigidbody.velocity,
			};
		}

		public void MOVE()
		{
			cc.UpdateMotor();               // updates the ThirdPersonMotor methods
			cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
			cc.ControlRotationType();       // handle the controller rotation type
		}


		public virtual void OnAnimatorMove()
		{
			cc.ControlAnimatorRootMotion(); // handle root motion animations 
		}

		short EncodeCoordinate(float value, float min, float max)
		{
			float normalized = Mathf.Clamp((value - min) / (max - min), 0f, 1f);
			return (short)(normalized * short.MaxValue);
		}

		float DecodeCoordinate(short value, float min, float max)
		{
			float normalized = value / (float)short.MaxValue;
			return normalized * (max - min) + min;
		}

		long PackVector3(Vector3 position)
		{
			position = Quantize(position, PRESICION);
			long packed = 0;
			packed |= ((long)EncodeCoordinate(position.x, MIN, MAX) & 0xFFFF) << 32; // X 좌표
			packed |= ((long)EncodeCoordinate(position.y, MIN, MAX) & 0xFFFF) << 16;  // Y 좌표
			packed |= ((long)EncodeCoordinate(position.z, MIN, MAX) & 0xFFFF);       // Z 좌표
			return packed;
		}

		Vector3 UnpackVector3(long packed)
		{
			return new Vector3(
				DecodeCoordinate((short)((packed >> 32) & 0xFFFF), MIN, MAX),
				DecodeCoordinate((short)((packed >> 16) & 0xFFFF), MIN, MAX),
				DecodeCoordinate((short)(packed & 0xFFFF), MIN, MAX));
		}

		Vector3 Quantize(Vector3 position, float precision)
		{
			return new Vector3(
				Mathf.Round(position.x / precision) * precision,
				Mathf.Round(position.y / precision) * precision,
				Mathf.Round(position.z / precision) * precision
			);
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
		public long inputVector;
		public long position;

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
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref tick);
			serializer.SerializeValue(ref networkObjectId);
			serializer.SerializeValue(ref position);
			serializer.SerializeValue(ref rotation);
			serializer.SerializeValue(ref velocity);
		}
	}
}