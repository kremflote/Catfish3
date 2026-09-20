using StarterAssets;

// On-foot movement with a shorter collider, mainly for crouch-jumping onto ledges.
public class CrouchState : LocomotionState
{
    public CrouchState(FirstPersonController controller) : base(controller)
    {
    }

    // Shrinks the player capsule when crouch state begins.
    public override void Enter()
    {
        base.Enter();
        pController.SetCrouched(true);
    }

    // Restores the standing capsule when leaving crouch.
    public override void Exit()
    {
        pController.SetCrouched(false);
    }

    // Keeps moving with crouch speed, then returns to normal movement when crouch is released.
    protected override void UpdateLocomotion()
    {
        if (!pController.Input.crouch && pController.CanStand())
        {
            pController.StateMachine.SwitchLocomotionState(new MovementState(pController));
            return;
        }

        pController.GroundedCheck();
        pController.JumpAndGravity();
        pController.Move();
    }
}
