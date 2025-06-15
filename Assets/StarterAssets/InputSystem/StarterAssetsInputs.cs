using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using FishNet.Object;
using FishNet.Connection;

namespace StarterAssets
{

    /* ⚠️ What’s Less Optimal / Old-School according to chatGPT
It assumes “Invoke Unity Events” in PlayerInput settings, which wires input events to functions like OnMove, OnJump, etc.

This is simple, but limits flexibility (e.g., no clean way to switch action maps or use multiple control schemes dynamically).

It doesn’t use InputActionAsset references directly, so you can't easily query action state like .IsPressed() or .triggered.

Input values are stored manually in public fields (move, look, etc.). This works, but can get messy with complex input logic, and disconnects you from Unity's input rebind/UI systems. */
    public class StarterAssetsInputs : NetworkBehaviour
    {
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
