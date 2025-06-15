using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

namespace StarterAssets
{
    [RequireComponent(typeof(PlayerInput))]
    public class StarterAssetsInputsModern : NetworkBehaviour
    {
        [Header("Input States")]
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpInput { get; private set; }
        public bool SprintInput { get; private set; }

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;

        private PlayerInput playerInput;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }



        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
            {
                enabled = false;
                return;
            }
            InitializeInput();
        }

        private void InitializeInput()
        {
            var actionMap = playerInput.actions.FindActionMap("Player", true);

            moveAction = actionMap.FindAction("Move", true);
            lookAction = actionMap.FindAction("Look", true);
            jumpAction = actionMap.FindAction("Jump", true);
            sprintAction = actionMap.FindAction("Sprint", true);

            // Optional: register callbacks (alternative to polling)
            jumpAction.performed += ctx => JumpInput = true;
            jumpAction.canceled += ctx => JumpInput = false;

            sprintAction.performed += ctx => SprintInput = true;
            sprintAction.canceled += ctx => SprintInput = false;

            // Enable manually if not auto-enabled
            actionMap.Enable();
        }

        private void Update()
        {
            if (!IsOwner) return;

            MoveInput = moveAction.ReadValue<Vector2>();
            LookInput = lookAction.ReadValue<Vector2>();

            // Jump and sprint are updated via callback
        }

        public void SwitchToActionMap(string mapName)
        {
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap(mapName);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!IsOwner) return;

            Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !hasFocus;
        }
    }
}
