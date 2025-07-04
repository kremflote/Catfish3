using StarterAssets;

public class MovementState : PlayerState
{
    public MovementState(FirstPersonController controller)
    {
        pController = controller;
    }

    private FirstPersonController pController;

    public override void Update()
    {
        pController.GroundedCheck();
        pController.JumpAndGravity();
        pController.Move();
        pController.UpdateCursorLock();
    }

    public override void LateUpdate()
    {
        if (!pController.InventoryToggleManager.GetIsOpen())
            pController.CameraRotation();
    }

    public override void Enter()
    {
        if (!pController.enabled)
            pController.enabled = true;
    }

    public override void Exit()
    {
        // move player to pilot position
        // disable controller
    }

}
