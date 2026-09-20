using StarterAssets;

// Normal on-foot movement.
public class MovementState : LocomotionState
{
    public MovementState(FirstPersonController controller) : base(controller)
    {
    }

    // Normal walking state; crouch becomes a slide when sprinting or already airborne.
    protected override void UpdateLocomotion()
    {
        pController.GroundedCheck();

        if (pController.Input.crouch)
        {
            if (pController.IsTryingToSprint || !pController.Grounded)
            {
                pController.StateMachine.SwitchLocomotionState(new SlidingState(pController));
                return;
            }

            pController.StateMachine.SwitchLocomotionState(new CrouchState(pController));
            return;
        }

        pController.JumpAndGravity();
        pController.Move();
    }

}
