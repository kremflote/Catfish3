using FishNet.Object;
using StarterAssets;
using UnityEngine;

public class BoatController : NetworkBehaviour
{
    // This class is responsible for controlling the boat's state and interactions with the player.
    // Not playermovement, that is controlled by FirstPersonController.

    public PlayerStateMachine stateMachine { get; private set; }
    public PlayerInputState _input { get; set; }
    public InventoryToggleManager inventoryToggleManager;
    public Boat boat;
    private int pilotClientId = -1;


    // Gives the boat controller references from setup code without requiring direct Inspector wiring.
    public void Initialize(InventoryToggleManager inventoryToggleManager, PlayerStateMachine stateMachine)
    {
        this.inventoryToggleManager = inventoryToggleManager;
        this.stateMachine = stateMachine;

    }

    // Converts local player movement input into steering input for the boat.
    public void Steer()
    {
        if (boat == null || _input == null) {
            Debug.LogError("BoatController is not properly initialized. Boat or input is null.", this);
            return;
        }
        float steerInput = _input.move.x;

        boat.SetSteeringInput(steerInput, pilotClientId);
    }

    // Starts reading input from the player who entered piloting mode.
    public void BeginPiloting(PlayerInputState input)
    {
        _input = input;
        pilotClientId = input != null && input.Owner.IsValid ? input.Owner.ClientId : -1;

        if (boat != null)
            boat.SetPilot(pilotClientId);
    }

    // Releases the pilot slot so the boat stops accepting this player's steering input.
    public void EndPiloting()
    {
        if (boat != null)
            boat.ClearPilot(pilotClientId);

        pilotClientId = -1;
        _input = null;
    }
}
