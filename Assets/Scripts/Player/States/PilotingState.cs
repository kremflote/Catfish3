using StarterAssets;

// Boat-control movement while the player remains the local input source.
public class PilotingState : LocomotionState
{
    private readonly BoatController bController;

    public PilotingState(BoatController bController, FirstPersonController firstPersonController, SelectionManager selectionManager)
        : base(firstPersonController, selectionManager)
    {
        this.bController = bController;
    }

    // Gives the boat access to the player's input while this state is active.
    public override void Enter()
    {
        base.Enter();
        // The boat reads the same input object as the player while piloting.
        bController.BeginPiloting(pController.Input);
    }

    // Releases boat control when the player leaves piloting state.
    public override void Exit()
    {
        bController.EndPiloting();
    }

    // Uses movement input to steer the boat instead of moving the player body.
    protected override void UpdateLocomotion()
    {
        bController.Steer();
    }

    // While piloting, also process draggable boat controls like throttle and gear.
    protected override void UpdateSelection()
    {
        base.UpdateSelection();
        selectionManager?.CheckMovableObject();
        selectionManager?.HandleMovableObject();
    }
}
