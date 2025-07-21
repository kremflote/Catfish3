using StarterAssets;

public class PilotingState : PlayerState
{
    // Denne klassen har en konstruktør som tar inn en BoatController fra IgnitionKey GameObject.
    // Den passeres da til spilleren når det opprettes PilotingState i IgnitionKey1 script.
    // MAO når spilleren vrir nøkkelen i båten, får hen dens controller og går i piloting state.


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
        // give boatcontroller starterassetinputs
        bController._input = pController._input;


        // pController.PilotMode(bController.GetPilotPosition());
        // move to pilot position
    }

    public override void Exit()
    {

    }
}
