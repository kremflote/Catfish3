using StarterAssets;

// Shared base for states where the player is still controlling their body or vehicle.
public abstract class LocomotionState : PlayerState
{
    protected readonly FirstPersonController pController;
    protected readonly SelectionManager selectionManager;

    protected LocomotionState(FirstPersonController controller, SelectionManager selectionManager = null)
    {
        pController = controller;
        this.selectionManager = selectionManager ?? controller.SelectionManager;
    }

    // Ensures the body controller is active when entering any locomotion state.
    public override void Enter()
    {
        if (!pController.enabled)
            pController.enabled = true;
    }

    // Shared locomotion loop: check overlays, update movement, then choose input/cursor behavior.
    public override void Update()
    {
        // Common player loop; concrete states only supply their movement mode.
        UpdateInventoryOverlay();
        pController.UpdatePlayerParent();
        UpdateLocomotion();

        if (pController.StateMachine.CurrentOverlayState is InventoryState)
        {
            pController.ApplyInputMode(PlayerInputMode.Inventory);
            return;
        }

        UpdateSelection();
        ApplyLocomotionInputMode();
    }

    // Runs camera look after movement, unless inventory overlay has camera control locked.
    public override void LateUpdate()
    {
        if (pController.StateMachine.CurrentOverlayState is InventoryState)
            return;

        pController.CameraRotation();
    }

    // Lets each locomotion state define whether movement means walking, crouching, or piloting.
    protected abstract void UpdateLocomotion();

    // Updates the object under the crosshair; piloting states can extend this for cockpit controls.
    protected virtual void UpdateSelection()
    {
        selectionManager?.HandleSelection();
    }

    // Chooses gameplay or world-interaction mode based on whether a draggable control is active.
    protected virtual void ApplyLocomotionInputMode()
    {
        PlayerInputMode mode = selectionManager != null && selectionManager.IsUsingWorldInteraction
            ? PlayerInputMode.WorldInteraction
            : PlayerInputMode.Gameplay;

        pController.ApplyInputMode(mode);
    }

    // Opens the inventory overlay without replacing the locomotion state underneath it.
    private void UpdateInventoryOverlay()
    {
        // Inventory is an overlay, so it should not replace walking or piloting.
        if (pController.InventoryVisibilityController.IsOpen)
            pController.StateMachine.SetOverlayState(new InventoryState(pController));
    }
}
