using StarterAssets;

// Short crouched burst that preserves run/air momentum before settling into crouch or normal movement.
public class SlidingState : LocomotionState
{
    public SlidingState(FirstPersonController controller) : base(controller)
    {
    }

    // Enters crouch height and captures the current forward momentum for the slide.
    public override void Enter()
    {
        base.Enter();
        pController.SetCrouched(true);
        pController.BeginSlide();
    }

    // Keeps sliding while airborne; once grounded and slow enough, resolves based on held crouch input.
    protected override void UpdateLocomotion()
    {
        pController.GroundedCheck();
        pController.JumpAndGravity();
        pController.SlideMove();

        if (!pController.Grounded)
            return;

        if (!pController.Input.crouch && pController.CanStand())
        {
            pController.SetCrouched(false);
            pController.StateMachine.SwitchLocomotionState(new MovementState(pController));
            return;
        }

        if (!pController.HasSlideEnded)
            return;

        if (pController.Input.crouch)
        {
            pController.StateMachine.SwitchLocomotionState(new CrouchState(pController));
            return;
        }

        if (pController.CanStand())
        {
            pController.SetCrouched(false);
            pController.StateMachine.SwitchLocomotionState(new MovementState(pController));
        }
    }
}
