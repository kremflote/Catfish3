using StarterAssets;

// Overlay state for inventory UI. Locomotion can keep running underneath it.
public class InventoryState : PlayerState
{
    private readonly FirstPersonController pController;

    public InventoryState(FirstPersonController controller)
    {
        pController = controller;
    }

    // Locks cursor/look for inventory UI while leaving locomotion state alive underneath.
    public override void Enter()
    {
        pController.ApplyInputMode(PlayerInputMode.Inventory);
    }

    // Keeps inventory input mode active until the UI closes, then removes this overlay state.
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
