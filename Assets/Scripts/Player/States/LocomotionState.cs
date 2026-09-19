using StarterAssets;

// Shared base for states where the player is still controlling their body or vehicle.
public abstract class LocomotionState : PlayerState
{
    protected readonly FirstPersonController pController;
    protected readonly SelectionManager selectionManager;

    protected LocomotionState(FirstPersonController controller, SelectionManager selectionManager = null)
    {
        pController = controller;
        this.selectionManager = selectionManager ?? controller.selectionManager;
    }

    public override void Enter()
    {
        if (!pController.enabled)
            pController.enabled = true;
    }

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

    public override void LateUpdate()
    {
        if (pController.StateMachine.CurrentOverlayState is InventoryState)
            return;

        pController.CameraRotation();
    }

    protected abstract void UpdateLocomotion();

    protected virtual void UpdateSelection()
    {
        selectionManager?.HandleSelection();
    }

    protected virtual void ApplyLocomotionInputMode()
    {
        PlayerInputMode mode = selectionManager != null && selectionManager.IsUsingWorldInteraction
            ? PlayerInputMode.WorldInteraction
            : PlayerInputMode.Gameplay;

        pController.ApplyInputMode(mode);
    }

    private void UpdateInventoryOverlay()
    {
        // Inventory is an overlay, so it should not replace walking or piloting.
        if (pController.InventoryToggleManager.GetIsOpen())
            pController.StateMachine.SetOverlayState(new InventoryState(pController));
    }
}
