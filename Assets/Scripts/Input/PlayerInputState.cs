using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using FishNet.Object;

namespace StarterAssets
{
    public enum InventoryMouseButton
    {
        Left,
        Right
    }

    // Owner-side input cache used by player, selection, and boat control code.
    public class PlayerInputState : NetworkBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool crouch;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Click Input")]
        public bool clickHeld;
        public bool clickPressedThisFrame;
        public bool clickReleasedThisFrame;
        public Vector2 pointerPosition;
        public Vector2 pointerDelta;

        [Header("Inventory Input")]
        [SerializeField] private InventoryVisibilityController inventoryVisibilityController;
        [SerializeField] private InventoryController inventoryController;

        public event Action OnTabPressed;
        public event Action OnQPressed;
        public event Action OnTPressed;
        public event Action OnUPressed;
        public event Action<int> OnHotbarKeyPressed;
        public event Action<InventoryMouseButton> OnMouseClick;

        public SelectionManager selectionManager;

        private bool lookInputEnabled = true;
        private bool interactInputEnabled = true;
        private bool CanAcceptInput => IsOwner && isActiveAndEnabled;

#if ENABLE_INPUT_SYSTEM
        // Runs when FishNet spawns this player on a client; only the owning client should collect input.
        public override void OnStartClient()
        {
            base.OnStartClient();

            clickHeld = false;
            clickPressedThisFrame = false;
            clickReleasedThisFrame = false;
            pointerPosition = Vector2.zero;
            pointerDelta = Vector2.zero;

            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            InitializeInventoryComponents();
        }

        // Clears one-frame click flags after other scripts have had a frame to read them.
        private void LateUpdate()
        {
            clickPressedThisFrame = false;
            clickReleasedThisFrame = false;
            pointerDelta = Vector2.zero;
        }

        // Receives movement input from Unity's PlayerInput component and caches it for the controller.
        public void OnMove(InputValue value)
        {
            if (!CanAcceptInput) return;
            MoveInput(value.Get<Vector2>());
        }

        // Passes interact input to SelectionManager, unless the current player state has disabled interaction.
        public void OnInteract(InputValue value)
        {
            if (!CanAcceptInput || !interactInputEnabled) return;
            if (selectionManager != null)
                selectionManager.OnInteract(value);
        }

        // Receives click input from Unity's PlayerInput component and caches held/pressed/released state.
        public void OnClick(InputValue value)
        {
            if (!CanAcceptInput) return;

            bool wasHeld = clickHeld;
            clickHeld = value.isPressed;
            clickPressedThisFrame = !wasHeld && clickHeld;
            clickReleasedThisFrame = wasHeld && !clickHeld;

            if (value.isPressed)
                OnMouseClick?.Invoke(InventoryMouseButton.Left);
        }

        // Receives mouse/look input and caches it, unless UI or another state has locked camera look.
        public void OnLook(InputValue value)
        {
            if (!CanAcceptInput || !lookInputEnabled) return;
            LookInput(value.Get<Vector2>());
        }

        // Receives the pointer screen position so other player scripts do not poll Mouse.current directly.
        public void OnPointerPosition(InputValue value)
        {
            if (!CanAcceptInput) return;
            pointerPosition = value.Get<Vector2>();
        }

        // Receives raw pointer movement for drag-style interactions such as cockpit levers.
        public void OnPointerDelta(InputValue value)
        {
            if (!CanAcceptInput) return;
            pointerDelta = value.Get<Vector2>();
        }

        // Receives jump input and stores it for movement code to consume.
        public void OnJump(InputValue value)
        {
            if (!CanAcceptInput) return;
            JumpInput(value.isPressed);
        }

        // Receives sprint input and stores it for movement code to consume.
        public void OnSprint(InputValue value)
        {
            if (!CanAcceptInput) return;
            SprintInput(value.isPressed);
        }

        // Receives crouch input as a hold, so release stands back up.
        public void OnCrouch(InputValue value)
        {
            if (!CanAcceptInput) return;
            CrouchInput(value.isPressed);
        }

        // Receives Tab from Unity input and runs the inventory open/close flow.
        public void OnInventoryToggle(InputValue value)
        {
            if (!CanAcceptInput || !value.isPressed) return;
            HandleInventoryToggleInput();
        }

        // Debug input for spawning a random held item while prototyping inventory behavior.
        public void OnDebugCreateRandomItem(InputValue value)
        {
            if (!CanAcceptInput || !value.isPressed) return;
            OnQPressed?.Invoke();
        }

        // Debug input for registering all inventory UI elements during prototyping.
        public void OnDebugInsertAllItems(InputValue value)
        {
            if (!CanAcceptInput || !value.isPressed) return;
            OnTPressed?.Invoke();
        }

        // Debug input for creating and inserting one random inventory item.
        public void OnDebugInsertRandomItem(InputValue value)
        {
            if (!CanAcceptInput || !value.isPressed) return;
            OnUPressed?.Invoke();
        }

        // Receives right-click from Unity input and forwards it to inventory mouse handling.
        public void OnRightClick(InputValue value)
        {
            if (!CanAcceptInput || !value.isPressed) return;
            OnMouseClick?.Invoke(InventoryMouseButton.Right);
        }

        // Selects hotbar slot 1; Unity calls this when the Hotbar1 action fires.
        public void OnHotbar1(InputValue value) => HandleHotbarInput(value, 0);

        // Selects hotbar slot 2; each hotbar action maps to one visible number key.
        public void OnHotbar2(InputValue value) => HandleHotbarInput(value, 1);

        // Selects hotbar slot 3; code uses zero-based indexes internally.
        public void OnHotbar3(InputValue value) => HandleHotbarInput(value, 2);

        // Selects hotbar slot 4 through the shared hotbar helper.
        public void OnHotbar4(InputValue value) => HandleHotbarInput(value, 3);

        // Selects hotbar slot 5 through the shared hotbar helper.
        public void OnHotbar5(InputValue value) => HandleHotbarInput(value, 4);

        // Selects hotbar slot 6 through the shared hotbar helper.
        public void OnHotbar6(InputValue value) => HandleHotbarInput(value, 5);

        // Selects hotbar slot 7 through the shared hotbar helper.
        public void OnHotbar7(InputValue value) => HandleHotbarInput(value, 6);

        // Selects hotbar slot 8 through the shared hotbar helper.
        public void OnHotbar8(InputValue value) => HandleHotbarInput(value, 7);

        // Selects hotbar slot 9 through the shared hotbar helper.
        public void OnHotbar9(InputValue value) => HandleHotbarInput(value, 8);

        // Shared hotbar helper keeps each number-key callback tiny and consistent.
        private void HandleHotbarInput(InputValue value, int hotbarIndex)
        {
            if (!CanAcceptInput || !value.isPressed) return;
            OnHotbarKeyPressed?.Invoke(hotbarIndex);
        }
#endif

        // Handles the extra bookkeeping needed when opening or closing inventory with a held item.
        private void HandleInventoryToggleInput()
        {
            if (inventoryVisibilityController == null || inventoryController == null)
                return;

            bool isOpen = inventoryVisibilityController.IsOpen;

            if (isOpen && !inventoryController.SelectedItemIsNull())
            {
                bool success = inventoryController.CancelPickupItem();
                if (!success)
                    return;

                Debug.Log("Player dropped item successfully");
                inventoryController.UpdateHotbar();
                OnTabPressed?.Invoke();
                return;
            }

            if (isOpen && inventoryController.SelectedItemIsNull())
            {
                inventoryController.UpdateHotbar();
                OnTabPressed?.Invoke();
                return;
            }

            if (!isOpen)
            {
                inventoryController.UpdateInventoryFromHotbar();
                OnTabPressed?.Invoke();
            }
        }

        // Finds the local inventory pieces on the player prefab so this script can route input to them.
        private void InitializeInventoryComponents()
        {
            if (inventoryController == null)
                inventoryController = transform.root.GetComponentInChildren<InventoryController>(true);

            if (inventoryVisibilityController == null)
                inventoryVisibilityController = transform.root.GetComponentInChildren<InventoryVisibilityController>(true);

            if (inventoryController == null)
                Debug.LogWarning("InventoryController reference is missing.", this);

            if (inventoryVisibilityController == null)
                Debug.LogWarning("InventoryVisibilityController reference is missing.", this);
        }

        // Stores movement direction as plain data so movement code does not need to know about Unity input events.
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        // Stores look direction as plain data so camera code can read the latest value each frame.
        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        // Stores jump state so movement code can decide when and how to apply the jump.
        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        // Stores sprint state so movement code can choose normal speed or sprint speed.
        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        // Stores crouch state so locomotion code can decide when to shrink or restore the capsule.
        public void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

        // Lets player states disable camera look, for example while inventory is open.
        public void SetLookInputEnabled(bool enabled)
        {
            lookInputEnabled = enabled;

            if (!lookInputEnabled)
                look = Vector2.zero;
        }

        // Lets player states disable world interaction without disabling all input.
        public void SetInteractInputEnabled(bool enabled)
        {
            interactInputEnabled = enabled;
        }
    }
}
