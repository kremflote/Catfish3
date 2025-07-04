using StarterAssets;

public class PilotingState : PlayerState
{
    private FirstPersonController pController;
    private BoatController bController;
    private InventoryToggleManager inventoryToggleManager;
    public PilotingState(BoatController bController, InventoryToggleManager inventoryToggleManager, FirstPersonController firstPersonController)
    {
        this.pController = firstPersonController;
        this.bController = bController;
        this.inventoryToggleManager = inventoryToggleManager;
    }

    public override void Update()
    {
        bController.Steer();
        pController.UpdateCursorLock();
    }

    public override void LateUpdate()
    {
        if (!inventoryToggleManager.GetIsOpen())
            
            pController.CameraRotation();
    }

    public override void Enter()
    {
        // pController.PilotMode(bController.GetPilotPosition());
        // move to pilot position
    }

    public override void Exit()
    {

    }
}
