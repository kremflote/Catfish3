using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using FishNet.Object;
using FishNet.Connection;

namespace StarterAssets
{   
    public class StarterAssetsInputs : NetworkBehaviour
    {
        // Dette scriptet leser input fra unity sitt input system
        // Deretter setter den current input som egne instance variabler, effectively exposing them
        // Deretter kan andre scripts accessere user input ved å ha en instance av dette scriptet

        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        public SelectionManager selectionManager;

        public void OnInteract(InputValue value)
        {
            if (!IsOwner) return;
            if (selectionManager != null)
                selectionManager.OnInteract(value);
        }


#if ENABLE_INPUT_SYSTEM
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                enabled = false;
        }
        public void OnMove(InputValue value)
        {
            if (!IsOwner) return;
            MoveInput(value.Get<Vector2>());
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
