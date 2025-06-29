using StarterAssets;

public class MovementState : PlayerState
{
    public MovementState(FirstPersonController controller) : base(controller) { }

    public override void Update()
    {
        controller.GroundedCheck();
        controller.JumpAndGravity();
        controller.Move();
        controller.UpdateCursorLock();
    }

    public override void LateUpdate()
    {
        if (!controller.InventoryToggleManager.GetIsOpen())
            controller.CameraRotation();
    }

    public override void Enter()
    {
        if (!controller.enabled)
            controller.enabled = true;
    }

    public override void Exit()
    {
        controller.enabled = false;
    }

}
