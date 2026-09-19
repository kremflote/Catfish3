using StarterAssets;

// Boat-control movement while the player remains the local input source.
public class PilotingState : LocomotionState
{
    private readonly BoatController bController;

    public PilotingState(BoatController bController, InventoryToggleManager inventoryToggleManager, FirstPersonController firstPersonController, SelectionManager selectionManager)
        : base(firstPersonController, selectionManager)
    {
        this.bController = bController;
    }

    public override void Enter()
    {
        base.Enter();
        // The boat reads the same input object as the player while piloting.
        bController._input = pController._input;
    }

    protected override void UpdateLocomotion()
    {
        bController.Steer();
    }

    protected override void UpdateSelection()
    {
        base.UpdateSelection();
        selectionManager?.CheckMovableObject();
        selectionManager?.HandleMovableObject();
    }
}
