using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using FishNet.Object;

namespace StarterAssets
{   
    // Owner-side input cache used by player, selection, and boat control code.
    public class PlayerInputState : NetworkBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Click Input")]
        public bool clickHeld;
        public bool click;

        private InputAction clickAction;


        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public SelectionManager selectionManager;




#if ENABLE_INPUT_SYSTEM
        public override void OnStartClient()
        {
            base.OnStartClient();

            clickHeld = false;

            clickAction = InputSystem.actions.FindAction("Click");

            if (!IsOwner)
                enabled = false;
        }

        private void Update()
        {
            UpdateClickState();
        }

        private void UpdateClickState()
        {
            UpdateClick();
            UpdateClickHeld();
        }

        private void UpdateClickHeld()
        {
            if (clickAction.WasPressedThisFrame())
            {
                clickHeld = true;
            }

            if (clickAction.WasReleasedThisFrame())
            {
                clickHeld = false;
            }
        }

        private void UpdateClick()
        {
            if (clickAction.IsPressed())
            {
                click = true;
            }

            else
            {
                click = false;
            }
        }

        public void OnMove(InputValue value)
        {
            if (!IsOwner) return;
            MoveInput(value.Get<Vector2>());
        }

        public void OnInteract(InputValue value)
        {
            if (!IsOwner) return;
            if (selectionManager != null)
                selectionManager.OnInteract(value);
        }

        public void OnClick(InputValue value)
        {
            if (!IsOwner) return;
            
        }

        public void OnLook(InputValue value)
        {
            if (!IsOwner || !cursorInputForLook) return;
            LookInput(value.Get<Vector2>());
        }

        public void OnJump(InputValue value)
        {
            if (!IsOwner) return;
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            if (!IsOwner) return;
            SprintInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (IsOwner)
                SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }

        
    }
}
