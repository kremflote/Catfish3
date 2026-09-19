using StarterAssets;

// On-foot movement with a shorter collider, mainly for crouch-jumping onto ledges.
public class CrouchState : LocomotionState
{
    public CrouchState(FirstPersonController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
        pController.SetCrouched(true);
    }

    public override void Exit()
    {
        pController.SetCrouched(false);
    }

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
