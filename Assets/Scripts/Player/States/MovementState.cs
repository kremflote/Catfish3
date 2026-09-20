using StarterAssets;

// Normal on-foot movement.
public class MovementState : LocomotionState
{
    public MovementState(FirstPersonController controller) : base(controller)
    {
    }

    // Normal walking state; switches to crouch when crouch input is held.
    protected override void UpdateLocomotion()
    {
        if (pController.Input.crouch)
        {
            pController.StateMachine.SwitchLocomotionState(new CrouchState(pController));
            return;
        }

        pController.GroundedCheck();
        pController.JumpAndGravity();
        pController.Move();
    }

}
