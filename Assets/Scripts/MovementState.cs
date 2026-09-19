using StarterAssets;

// Normal on-foot movement.
public class MovementState : LocomotionState
{
    public MovementState(FirstPersonController controller) : base(controller)
    {
    }

    protected override void UpdateLocomotion()
    {
        pController.GroundedCheck();
        pController.JumpAndGravity();
        pController.Move();
    }

}
