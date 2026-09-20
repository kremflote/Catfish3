using FishNet.Object;
using UnityEngine;



#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public enum PlayerInputMode
    {
        Gameplay,
        Inventory,
        WorldInteraction
    }

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

        [Header("Crouch")]
        [Tooltip("CharacterController height while crouching.")]
        [SerializeField] private float crouchHeight = 1.2f;
        [Tooltip("Movement speed multiplier while crouching.")]
        [SerializeField] private float crouchSpeedMultiplier = 0.65f;
        [Tooltip("How long the collider takes to move between standing and crouching.")]
        [SerializeField] private float crouchTransitionDuration = 0.18f;

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
		[Tooltip("How far below the CharacterController bottom to check for ground.")]
		public float GroundedOffset = 0.05f;
		[Tooltip("The grounded check radius as a multiplier of the CharacterController radius.")]
		public float GroundedRadius = 0.35f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		private bool canRotate = true;
        private float _cinemachineTargetPitch;

        [Header("References")]
        [SerializeField] private PlayerContext playerContext;
        [SerializeField] private GameObject mainCameraObject;
        [SerializeField] private InventoryVisibilityController inventoryVisibilityController;
        [SerializeField] private PlayerStateMachine stateMachine;
        [SerializeField] private SelectionManager selectionManager;

        private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;


#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private PlayerInputState _input;
        private GameObject _mainCamera;
        private Camera playerCamera;
        private PlayerInputMode currentInputMode = PlayerInputMode.Gameplay;
        private bool wantsCursorLocked = true;
        private bool isCrouched;
        private float standingHeight;
        private Vector3 standingCenter;
        private float targetColliderHeight;
        private Vector3 targetColliderCenter;
        private const float _threshold = 0.01f;

        public InventoryVisibilityController InventoryVisibilityController => inventoryVisibilityController;
        public PlayerStateMachine StateMachine => stateMachine;
        public PlayerInputState Input => _input;
        public SelectionManager SelectionManager => selectionManager;
        public bool IsCrouched => isCrouched;

        // FishNet calls this when the player exists on this client; only the owner initializes local control.
		public override void OnStartClient()
		{
            base.OnStartClient();

            _controller = GetComponent<CharacterController>();
            CacheStandingCollider();

            if (base.IsOwner)
            {
                InitializeComponents();
                if (stateMachine != null)
                    stateMachine.Initialize(new MovementState(this));
                if (_mainCamera != null)
                {
                    _mainCamera.SetActive(true);
                    playerCamera = _mainCamera.GetComponent<Camera>();
                }
                
#if ENABLE_INPUT_SYSTEM
                _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
                _jumpTimeoutDelta = JumpTimeout;
                _fallTimeoutDelta = FallTimeout;
                ApplyInputMode(PlayerInputMode.Gameplay);
            }
        }

        // Stores the original capsule dimensions so crouch can return to the correct standing shape.
        private void CacheStandingCollider()
        {
            if (_controller == null)
                return;

            standingHeight = _controller.height;
            standingCenter = _controller.center;
            targetColliderHeight = standingHeight;
            targetColliderCenter = standingCenter;
        }

        // Resolves references from PlayerContext instead of relying entirely on Inspector wiring.
        private void InitializeComponents()
        {
            Transform playerRoot = transform.root;

            if (playerContext == null)
                playerContext = playerRoot.GetComponent<PlayerContext>();

            if (playerContext == null)
                playerContext = playerRoot.gameObject.AddComponent<PlayerContext>();

            playerContext.ResolveReferences();

            if (mainCameraObject == null)
                mainCameraObject = playerContext.MainCamera != null ? playerContext.MainCamera.gameObject : null;

            if (inventoryVisibilityController == null)
                inventoryVisibilityController = playerContext.InventoryVisibilityController;

            if (_input == null)
                _input = playerContext.Input != null ? playerContext.Input : GetComponent<PlayerInputState>();

            if (stateMachine == null)
                stateMachine = playerContext.StateMachine;

            if (selectionManager == null)
                selectionManager = playerRoot.GetComponent<SelectionManager>();

            _mainCamera = mainCameraObject;

            if (_mainCamera == null)
                Debug.LogError("Main camera reference is missing.", this);

            if (inventoryVisibilityController == null)
                Debug.LogError("InventoryVisibilityController reference is missing.", this);

            if (_input == null)
                Debug.LogError("PlayerInputState reference is missing.", this);

            if (stateMachine == null)
                Debug.LogError("PlayerStateMachine reference is missing.", this);
        }

        // Starts or stops crouch by choosing the target collider height/center for the smooth transition.
        public void SetCrouched(bool shouldCrouch)
        {
            if (_controller == null || isCrouched == shouldCrouch)
                return;

            if (!shouldCrouch && !CanStand())
                return;

            isCrouched = shouldCrouch;

            if (isCrouched)
            {
                float targetHeight = Mathf.Clamp(crouchHeight, _controller.radius * 2f, standingHeight);
                targetColliderHeight = targetHeight;
                targetColliderCenter = standingCenter + Vector3.up * ((standingHeight - targetHeight) / 2f);
                return;
            }

            targetColliderHeight = standingHeight;
            targetColliderCenter = standingCenter;
        }

        // Placeholder for future ceiling checks; currently standing is always allowed.
        public bool CanStand()
        {
            // Crouch keeps the capsule top stable and raises the bottom, so standing only extends downward.
            return true;
        }

        // Applies a named input mode so player states own cursor lock, camera look, and interaction access.
        public void ApplyInputMode(PlayerInputMode mode)
        {
            currentInputMode = mode;

            switch (mode)
            {
                case PlayerInputMode.Inventory:
                    if (_input != null)
                        _input.CrouchInput(false);

                    SetCrouched(false);
                    ApplyInputSettings(cursorLocked: false, lookEnabled: false, interactEnabled: false, rotationEnabled: false);
                    break;
                case PlayerInputMode.WorldInteraction:
                    ApplyInputSettings(cursorLocked: true, lookEnabled: false, interactEnabled: true, rotationEnabled: false);
                    break;
                default:
                    ApplyInputSettings(cursorLocked: true, lookEnabled: true, interactEnabled: true, rotationEnabled: true);
                    break;
            }
        }

        // Applies the actual input/cursor flags behind a higher-level PlayerInputMode.
        private void ApplyInputSettings(bool cursorLocked, bool lookEnabled, bool interactEnabled, bool rotationEnabled)
        {
            wantsCursorLocked = cursorLocked;
            ApplyCursorState();
            canRotate = rotationEnabled;
            if (_input == null)
                return;

            _input.SetLookInputEnabled(lookEnabled);
            _input.SetInteractInputEnabled(interactEnabled);
        }

        // Sets Unity's global cursor state to match the active player mode.
        private void ApplyCursorState()
        {
            Cursor.lockState = wantsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !wantsCursorLocked;
        }

        // Owner-only per-frame maintenance for crouch animation and cursor lock recovery.
        private void Update()
        {
            if (!IsOwner)
                return;

            UpdateCrouchTransition();
            EnsureCursorState();
        }

        // Smoothly animates CharacterController height/center toward standing or crouched values.
        private void UpdateCrouchTransition()
        {
            if (_controller == null)
                return;

            float duration = Mathf.Max(0.01f, crouchTransitionDuration);
            float heightStep = Mathf.Abs(standingHeight - crouchHeight) / duration * Time.deltaTime;
            _controller.height = Mathf.MoveTowards(_controller.height, targetColliderHeight, heightStep);
            _controller.center = Vector3.MoveTowards(_controller.center, targetColliderCenter, heightStep);
        }

        // Reapplies cursor lock after alt-tab or focus changes.
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && IsOwner)
                EnsureCursorState();
        }

        // Repairs cursor state if Unity or the OS changes it outside our input-mode flow.
        private void EnsureCursorState()
        {
            CursorLockMode expectedLockState = wantsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
            bool expectedVisible = !wantsCursorLocked;

            if (Cursor.lockState != expectedLockState || Cursor.visible != expectedVisible)
                ApplyInputMode(currentInputMode);
        }

        // Parents the player to a boat while standing on it so the player moves with the boat.
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

        // Checks whether the character is grounded using the capsule bottom instead of only Unity's built-in flag.
        public void GroundedCheck()
		{
            if (_controller == null)
            {
                Grounded = false;
                return;
            }

            GetCapsulePoints(_controller.height, _controller.center, out Vector3 bottom, out _, out float radius);
            float checkRadius = Mathf.Max(0.01f, radius * GroundedRadius);
            Vector3 footPosition = bottom - transform.up * radius;
            Vector3 spherePosition = footPosition + transform.up * checkRadius + Vector3.down * GroundedOffset;
			Grounded = _controller.isGrounded || Physics.CheckSphere(spherePosition, checkRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

        // Rotates the body horizontally and the Cinemachine target vertically from look input.
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

        // Moves the CharacterController using cached input, sprint/crouch speed, and current vertical velocity.
        public void Move()
		{
			// CharacterController movement kept from Starter Assets, used by MovementState.
			float targetSpeed = isCrouched ? MoveSpeed * crouchSpeedMultiplier : (_input.sprint ? SprintSpeed : MoveSpeed);

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

        // Applies jump impulse and gravity, including small timers that make jumps and step-offs feel smoother.
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
			if (_verticalVelocity > -_terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

        // Calculates useful world-space points for the CharacterController capsule.
        private void GetCapsulePoints(float height, Vector3 center, out Vector3 bottom, out Vector3 top, out float radius)
        {
            radius = Mathf.Max(0.01f, _controller.radius * 0.95f);
            float halfHeight = Mathf.Max(height / 2f, radius);
            Vector3 worldCenter = transform.TransformPoint(center);
            Vector3 capsuleOffset = transform.up * Mathf.Max(0f, halfHeight - radius);

            bottom = worldCenter - capsuleOffset;
            top = worldCenter + capsuleOffset;
        }

        // Keeps camera pitch inside a readable range and handles angle wrapping.
		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

        // Draws the grounded probe in the Scene view to make collider tuning easier.
        private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

            CharacterController controller = _controller != null ? _controller : GetComponent<CharacterController>();
            if (controller == null)
                return;

            float radius = Mathf.Max(0.01f, controller.radius * 0.95f);
            float halfHeight = Mathf.Max(controller.height / 2f, radius);
            Vector3 worldCenter = transform.TransformPoint(controller.center);
            Vector3 bottom = worldCenter - transform.up * Mathf.Max(0f, halfHeight - radius);
            Vector3 footPosition = bottom - transform.up * radius;
			Gizmos.DrawSphere(footPosition + transform.up * (radius * GroundedRadius) + Vector3.down * GroundedOffset, radius * GroundedRadius);
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
