using StarterAssets;

// Overlay state for inventory UI. Locomotion can keep running underneath it.
public class InventoryState : PlayerState
{
    private readonly FirstPersonController pController;

    public InventoryState(FirstPersonController controller)
    {
        pController = controller;
    }

    public override void Enter()
    {
        pController.ApplyInputMode(PlayerInputMode.Inventory);
    }

    public override void Update()
    {
        if (!pController.InventoryToggleManager.GetIsOpen())
        {
            pController.StateMachine.ClearOverlayState(this);
            return;
        }

        pController.ApplyInputMode(PlayerInputMode.Inventory);
    }
}
