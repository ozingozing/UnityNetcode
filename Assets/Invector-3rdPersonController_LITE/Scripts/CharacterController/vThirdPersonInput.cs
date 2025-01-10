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
		MyPlayer myPlayer;
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

		//CountdownTimer extrapolationTimer;
		CountdownTimer reconciliationTimer;
		ChocoOzing.Network.Vector3Compressor vectorCompressor = new Vector3Compressor(1000f, -1000f);
		ChocoOzing.Network.QuaternionCompressor quaternionCompressor = new ChocoOzing.Network.QuaternionCompressor();

		[Header("Netcode Debug")]
		[SerializeField] TextMeshProUGUI networkText;
		[SerializeField] TextMeshProUGUI playerText;
		[SerializeField] TextMeshProUGUI serverRpcText;
		[SerializeField] TextMeshProUGUI clientRpcText;

		public ulong thisClientId;
		public bool isDebug = false;
		public bool ValueCorrection;
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
			myPlayer = GetComponent<MyPlayer>();

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
			while (neworkTimer.ShouldTick() && ValueCorrection)
			{
				if(IsLocalPlayer)
					HandleClientTick();
				if(IsServer)
					HandleServerTick();
			}
			if (IsLocalPlayer && !myPlayer.IsMoveLock.Value)
			{
				MOVE();
			}
		}


		protected virtual void Update()
		{
			neworkTimer.Update(Time.deltaTime);
			reconciliationTimer.Tick(Time.deltaTime);
			//extrapolationTimer.Tick(Time.deltaTime);
			if(isDebug)
				playerText.SetText($"Owner: {IsOwner} NetworkObjectId: {NetworkObjectId} Velocity: {cc._rigidbody.velocity.magnitude:F1}");

			if (IsLocalPlayer && !myPlayer.IsMoveLock.Value)
			{
				InputHandle();                  // update the input methods
				cc.UpdateAnimator();            // updates the Animator Parameters
			}
		}

		void SwitchAuthorityMode(AuthorityMode mode)
		{
			clientNetworkTransform.authorityMode = mode;
			/*bool shouldSync = mode == AuthorityMode.Client;
			clientNetworkTransform.SyncPositionX = shouldSync;
			clientNetworkTransform.SyncPositionY = shouldSync;
			clientNetworkTransform.SyncPositionZ = shouldSync;*/
		}

		void HandleServerTick()
		{
			if (!IsServer || IsOwner) return;
			var bufferIndex = -1;
			InputPayload inputPayload = default;
			SwitchAuthorityMode(AuthorityMode.Server);
			while (serverInputQueue.Count > 0)
			{
				inputPayload = serverInputQueue.Dequeue();
				bufferIndex = inputPayload.tick % bufferSize;
				StatePayload statePayload = ProcessMovement(inputPayload);
				statePayload = HandleExtrapolation(statePayload, this.transform, CalculateLatencyInMillis(inputPayload));
				serverStateBuffer.Add(statePayload, bufferIndex);
			}
			SwitchAuthorityMode(AuthorityMode.Client);
			if (bufferIndex == -1) return;
			StatePayload state = serverStateBuffer.Get(bufferIndex);
			SendToClientRpc(new PackedStatePayload()
			{
				tick = state.tick,
				position = vectorCompressor.PackVector3(state.position),
				rotation = quaternionCompressor.PackQuaternion(state.rotation),
				velocity = vectorCompressor.PackVector3(state.velocity),
			});
		}

		/*void Extrapolate()
		{
			if (!IsOwner && IsServer && extrapolationTimer.IsRunning)
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
		}*/

		StatePayload HandleExtrapolation(StatePayload latest, Transform current,float latency)
		{
			if (ShouldExtrapolate(latency))
			{
				float latencyWeight = latency;

				Vector3 positionDelta = current.position - latest.position;
				Quaternion rotationDelta = latest.rotation * Quaternion.Inverse(current.rotation);

				latest.position += positionDelta * latencyWeight;
				latest.rotation *= Quaternion.Slerp(Quaternion.identity, rotationDelta, latencyWeight);

				if(IsServer && !IsOwner)
				{
					transform.rotation *= Quaternion.Slerp(transform.rotation, latest.rotation, 360f * Time.deltaTime);
					transform.position = Vector3.Lerp(transform.position, latest.position, 10 * Time.deltaTime);
				}

				/*latest.velocity = latest.velocity * latencyWeight;
				latest.position += latest.velocity * latencyWeight;
				latest.rotation *= Quaternion.AngleAxis(
						latencyWeight * latest.velocity.magnitude * Mathf.Rad2Deg,
						Vector3.up);

				if( IsServer &&
					!IsOwner &&
					latest.networkObjectId == thisClientId)
				{
					transform.rotation *= Quaternion.Slerp(transform.rotation, latest.rotation, 360f * Time.deltaTime);
					transform.position = Vector3.Lerp(transform.position, latest.position, 10 * Time.deltaTime);
				}*/
			}
			return latest;
		}

		bool ShouldExtrapolate(float latency)
		{
			return (latency < 0.1f) ? false :  latency < extrapolationLimit && latency > Time.fixedDeltaTime;
		}

		[ClientRpc]
		void SendToClientRpc(PackedStatePayload statePayload)
		{
			if (isDebug)
			{
				clientRpcText.SetText($"Received state from server Tick {statePayload.tick} Server POS: {statePayload.position}");
				serverCube.transform.position = vectorCompressor.UnpackVector3(statePayload.position);
			}

			if(IsOwner)
			lastServerState = new StatePayload()
			{
				tick = statePayload.tick,
				position = vectorCompressor.UnpackVector3(statePayload.position),
				rotation = quaternionCompressor.UnpackQuaternion(statePayload.rotation),
				velocity = vectorCompressor.UnpackVector3(statePayload.velocity),
			};
		}

		void HandleClientTick()
		{
			var currentTick = neworkTimer.CurrentTick;
			var bufferIndex = currentTick % bufferSize;


			InputPayload inputPayload = new InputPayload()
			{
				tick = currentTick,
				timestamp = DateTime.Now,
				inputVector = vectorCompressor.PackVector3(cc.moveDirection),
				position = vectorCompressor.PackVector3(transform.position),
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
		void SendToServerRpc(InputPayload input, ServerRpcParams serverRpcParams = default)
		{
			if(isDebug)
			{
				serverRpcText.SetText($"Received input from client Tick: {input.tick} Client POS: {input.position}");
				clientCube.transform.position = vectorCompressor.UnpackVector3(input.position);
			}
			if(serverRpcParams.Receive.SenderClientId == thisClientId)
			{
				serverInputQueue.Enqueue(input);
			}
		}

		StatePayload ProcessMovement(InputPayload input)
		{
			if (IsServer &&
				!IsOwner)
			{
				Vector3 targetDirection = vectorCompressor.UnpackVector3(input.inputVector).With(y: 0);
				transform.forward = Vector3.Slerp(
					transform.forward,
					targetDirection,
					360f * Time.deltaTime
				);
				transform.position = Vector3.Lerp(transform.position, vectorCompressor.UnpackVector3(input.position), 10 * Time.deltaTime);
			}

			return new StatePayload()
			{
				tick = input.tick,
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
/*
		// Change float to short
		short EncodeCoordinate(float value, float min, float max, int maxBits = 1023)
		{
			// Normalize to [min, max]
			float normalized = Mathf.Clamp((value - min) / (max - min), 0f, 1f);
			return (short)(normalized * maxBits); // Scale to fit within 10 bits
		}

		// Change int to float
		float DecodeCoordinate(int value, float min, float max, int maxBits = 1023)
		{
			float normalized = value / (float)maxBits; // Restore from 10-bit scale
			return normalized * (max - min) + min; // Denormalize back to [min, max]
		}

		int PackVector3(Vector3 position)
		{
			int packed = 0;
			packed |= (EncodeCoordinate(position.x, MIN, MAX) & 0x3FF) << 20; // X 좌표 (10비트)
			packed |= (EncodeCoordinate(position.y, MIN, MAX) & 0x3FF) << 10; // Y 좌표 (10비트)
			packed |= (EncodeCoordinate(position.z, MIN, MAX) & 0x3FF);       // Z 좌표 (10비트)
			return packed;
		}

		int PackQuaternion(Quaternion quaternion)
		{
			float largest = Mathf.Abs(quaternion.x);
			int largestIndex = 0;

			if (Mathf.Abs(quaternion.y) > largest) { largest = Mathf.Abs(quaternion.y); largestIndex = 1; };
			if (Mathf.Abs(quaternion.z) > largest) { largest = Mathf.Abs(quaternion.z); largestIndex = 2; };
			if (Mathf.Abs(quaternion.w) > largest) { largest = Mathf.Abs(quaternion.w); largestIndex = 3; };

			int sign = (quaternion[largestIndex] < 0) ? 1 : 0;

			float a = quaternion[(largestIndex + 1) % 4];
			float b = quaternion[(largestIndex + 2) % 4];
			float c = quaternion[(largestIndex + 3) % 4];

			int packedA = Mathf.RoundToInt((a + 1f) * 1023f);
			int packedB = Mathf.RoundToInt((b + 1f) * 1023f);
			int packedC = Mathf.RoundToInt((c + 1f) * 1023f);

			return 
				(largestIndex << 30) |
				(sign << 29) |
				(packedA << 20) |
				(packedB << 10) |
				packedC;
		}

		Vector3 UnpackVector3(int packed)
		{
			// 10비트를 기준으로 각 축 값을 해석
			float x = DecodeCoordinate((packed >> 20) & 0x3FF, MIN, MAX); // X 좌표 (10비트)
			float y = DecodeCoordinate((packed >> 10) & 0x3FF, MIN, MAX); // Y 좌표 (10비트)
			float z = DecodeCoordinate(packed & 0x3FF, MIN, MAX);         // Z 좌표 (10비트)
			return new Vector3(x, y, z);
		}

		Quaternion UnpackQuaternion(int packed)
		{
			int largestIndex = (packed >> 30) & 0x3;
			int sign = (packed >> 29) & 0x1;
			int packedA = (packed >> 20) & 0x3FF;
			int packedB = (packed >> 10) & 0x3FF;
			int packedC = packed & 0x3FF;

			//Restorate Quaternion value around [-1, 1]
			float a = (packedA / 1023f) - 1f;
			float b = (packedB / 1023f) - 1f;
			float c = (packedC / 1023f) - 1f;
			//Restorate  Quaternion w value
			float w = Mathf.Sqrt(1f - a * a - b * b - c * c);

			Quaternion quaternion = new Quaternion();
			quaternion[(largestIndex + 1) % 4] = a;
			quaternion[(largestIndex + 2) % 4] = b;
			quaternion[(largestIndex + 3) % 4] = c;
			quaternion[largestIndex] = (sign == 1) ? -w : w;

			return quaternion;
		}
*/
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
				thirdPersonCamera.gameObject.SetActive(true);
				adsVirtualCamera.gameObject.SetActive(false);
			}
			else
			{
				thirdPersonCamera.gameObject.SetActive(false);
				adsVirtualCamera.gameObject.SetActive(true);
			}
			cc.isStrafing = myPlayer.StateMachine.CurrentState.IsAiming;
			
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
}

//Network variables should be value objects
[System.Serializable]
public struct InputPayload : INetworkSerializable
{
	public int tick;
	public DateTime timestamp;
	public int inputVector;
	public int position;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref tick);
		serializer.SerializeValue(ref timestamp);
		serializer.SerializeValue(ref inputVector);
		serializer.SerializeValue(ref position);
	}
}

[System.Serializable]
public struct StatePayload	
{
	public int tick;
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 velocity;
}

[System.Serializable]
public struct PackedStatePayload : INetworkSerializable
{
	public int tick;
	public int position;
	public int rotation;
	public int velocity;
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref tick);
		serializer.SerializeValue(ref position);
		serializer.SerializeValue(ref rotation);
		serializer.SerializeValue(ref velocity);
	}
}