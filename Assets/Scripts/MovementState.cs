using StarterAssets;

public class MovementState : PlayerState
{

    private SelectionManager selectionManager;
    public MovementState(FirstPersonController controller)
    {
        pController = controller;
        selectionManager = controller.selectionManager;

        if (selectionManager == null)
        {

        }
    }

    private FirstPersonController pController;

    public override void Update()
    {
        pController.UpdatePlayerParent();

        pController.GroundedCheck();
        pController.JumpAndGravity();
        pController.Move();
        pController.UpdateCursorLock();
        
        selectionManager.HandleSelection();
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
