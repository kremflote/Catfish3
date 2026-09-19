using FishNet.Object;
using UnityEngine;



#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : NetworkBehaviour
	{
		// Owner-side controller for body movement, camera rotation, and player references.

		
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		private bool mouseEnabled;
		public bool canRotate = true;
        private float _cinemachineTargetPitch;

        [Header("InventoryToggleManager")]
        [Tooltip("Changes movement if inventory is open")]
        public InventoryToggleManager InventoryToggleManager;

        [Header("Player")]
        private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


#if ENABLE_INPUT_SYSTEM
        public PlayerInput _playerInput;
#endif
        public CharacterController _controller;
		public PlayerStateMachine StateMachine;
        public PlayerInputState _input {get; set; }
		public SelectionManager selectionManager; 
		private GameObject _mainCamera;
        private Camera playerCamera;
        private const float _threshold = 0.01f;

		public override void OnStartClient()
		{
            base.OnStartClient();

            _controller = GetComponent<CharacterController>();

            if (base.IsOwner)
            {
                InitializeComponents();
                StateMachine.Initialize(new MovementState(this));
                _mainCamera.SetActive(true);
				playerCamera = _mainCamera.GetComponent<Camera>();
                mouseEnabled = false;
                
#if ENABLE_INPUT_SYSTEM
                _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
                _jumpTimeoutDelta = JumpTimeout;
                _fallTimeoutDelta = FallTimeout;
            }
        }
        private void InitializeComponents()
        {
            Transform parent = transform.parent;
            Transform mainCamera = parent.Find("MainCamera");
            Transform managers = parent.Find("Managers");
            Transform inventoryToggleManagerGO = managers.Find("InventoryToggleManager");
            InventoryToggleManager = inventoryToggleManagerGO.GetComponent<InventoryToggleManager>();
            _mainCamera = mainCamera.gameObject;
            _input = GetComponent<PlayerInputState>();
        }
        public void UpdateCursorLock()
        {
            // Inventory owns the cursor while open; gameplay owns it otherwise.
            if (InventoryToggleManager.GetIsOpen() == true)
            {
                mouseEnabled = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                mouseEnabled = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        public void UpdatePlayerParent()
        {
            // Keep the player root attached to a boat while standing on it.
            Transform playerRootTransform = transform.parent;

            float rayDistance = 5f;
            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
            {
                Transform current = hit.collider.transform;
				Debug.Log("Raycast hit: " + current.name);

                while (current != null)
                {
                    Transform modifiedBoat = current.Find("modifiedboat");
                    if (modifiedBoat != null)
                    {
						Debug.Log("Found modifiedboat in: " + current.name);
                        playerRootTransform.SetParent(current);
                        return;
                    }

                    current = current.parent;
                }
            }
            playerRootTransform.SetParent(null);
        }
        public void GroundedCheck()
		{
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}
        public void CameraRotation()
		{
			if (!canRotate)
            { return; }
			if (_input.look.sqrMagnitude >= _threshold)
			{
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}
        public void Move()
		{
			// CharacterController movement kept from Starter Assets, used by MovementState.
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
			if (_input.move != Vector2.zero)
			{
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}
        public void JumpAndGravity()
		{
			if (Grounded)
			{
				_fallTimeoutDelta = FallTimeout;
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				_jumpTimeoutDelta = JumpTimeout;
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
				_input.jump = false;
			}
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}
		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}
        private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }
    }
}
