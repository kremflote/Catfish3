using System;
using System.Globalization;
using FishNet.Object;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : NetworkBehaviour
{
    // This class is responsible for controlling the boat's state and interactions with the player.
    // Not playermovement, that is controlled by FirstPersonController.

    public PlayerStateMachine stateMachine { get; private set; }
    public StarterAssetsInputs _input { get; set; }
    public InventoryToggleManager inventoryToggleManager;
    public Boat boat;
    private Wheel wheelScript;


    public void Initialize(InventoryToggleManager inventoryToggleManager, PlayerStateMachine stateMachine)
    {
        this.inventoryToggleManager = inventoryToggleManager;
        this.stateMachine = stateMachine;
        
    }

    private void Start()
    {
        wheelScript = boat.wheel.GetComponent<Wheel>();
    }

    public void Steer()
    {
        if (boat == null || wheelScript == null || _input == null) {
            Debug.LogError("BoatController is not properly initialized. Boat or wheel or input is null.", this);
            return;
        }
        float steerInput = _input.move.x;

        wheelScript.AddAngle(steerInput);
    }

    internal object GetPilotPosition()
    {
        throw new NotImplementedException();
    }

    
}
