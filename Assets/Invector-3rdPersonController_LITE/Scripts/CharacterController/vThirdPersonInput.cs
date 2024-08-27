using Cinemachine;
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
        private Camera thirdPersonCamera;
		private CinemachineVirtualCamera adsVirtualCamera;
        //
		#endregion

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
			movementStateManager = GetComponent<MovementStateManager>();
			thirdPersonCamera = ThirdPersonCamera.Instance.GetComponent<Camera>();
			adsVirtualCamera = AdsCamera.Instance.GetComponent<CinemachineVirtualCamera>();
		}

		protected virtual void Start()
        {
			InitilizeController();
			InitializeTpCamera();
		}

        protected virtual void FixedUpdate()
        {
            cc.UpdateMotor();               // updates the ThirdPersonMotor methods
            cc.ControlLocomotionType();     // handle the controller locomotion type and movespeed
            cc.ControlRotationType();       // handle the controller rotation type
        }

        protected virtual void Update()
		{
			if (!IsOwner) return;
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
                //tpCamera = ThirdPersonCamera.Instance.GetComponent<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera && IsLocalPlayer)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }
        }

        protected virtual void InputHandle()
        {
            if(!Input.GetKey(KeyCode.Mouse1))
            {
				ChangeAnimationLayerWieght(1, 0);
                thirdPersonCamera.gameObject.SetActive(true);
                adsVirtualCamera.gameObject.SetActive(false);
            }
			else
            {
                thirdPersonCamera.gameObject.SetActive(false);
                adsVirtualCamera.gameObject.SetActive(true);
                ChangeAnimationLayerWieght(1, 1);
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

            if(!Input.GetButtonDown(horizontalInput) || !Input.GetButtonDown(verticallInput))
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

        /// <summary>
        /// Conditions to trigger the Jump animation & behavior
        /// </summary>
        /// <returns></returns>
        protected virtual bool JumpConditions()
        {
            return cc.isGrounded && cc.GroundAngle() < cc.slopeLimit && !cc.isJumping && !cc.stopMove;
        }

        /// <summary>
        /// Input to trigger the Jump 
        /// </summary>
        protected virtual void JumpInput()
        {
            if (Input.GetKeyDown(jumpInput) && JumpConditions())
                cc.Jump();
        }

        #endregion     
        
        //custom
        void ChangeAnimationLayerWieght(int  layer, float weight)
        {
            for(int i = 0; i < movementStateManager.anim.layerCount; i++)
            {
                if (i == 2) continue;
                movementStateManager.anim.SetLayerWeight(i, 0);
            }

            movementStateManager.anim.SetLayerWeight(layer, weight);
        }
    }
}