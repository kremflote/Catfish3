using System;
using FishNet.Example.Scened;
using FishNet.Object;
using StarterAssets;
using UnityEngine;

public class IgnitionKey1 : InteractableObject
{
    public bool isOn = false; 
    public bool isIn = false; 

    [SerializeField] private Transform key1;

    public BoatController BoatController;

    public override void Interact(PlayerStateMachine stateMachine, InventoryToggleManager inventoryToggleManager, FirstPersonController playerController)
    {
        Debug.Log("Interacting with Ignition Key 1");
        HandleKeyInsertion();
        ToggleIgnition(stateMachine, inventoryToggleManager, playerController);
    }

    private void HandleKeyInsertion()
    {
        if (!isIn)
        {
            bool success = TryInsertKey();
            if (success)
            {
                isIn = true;
                // make key visible
            }
        }
    }

    private bool TryInsertKey()
    {
        return true; // This method can be expanded to include logic for checking if the key can be inserted
    }

    public void InsertKey()
    {
        isIn = true; // Set the key as inserted
        key1.localRotation = Quaternion.Euler(0, 0, 0); // Reset the key rotation when inserted
        Debug.Log("Key inserted into ignition.");
    }

    public bool IsOn()
    {
        return isOn; // Return the current state of the ignition key
    }

    public bool IsIn()
    {
        return isIn; // Return whether the key is inserted in the ignition
    }

    public void ToggleIgnition(PlayerStateMachine stateMachine, InventoryToggleManager inventoryToggleManager, FirstPersonController playerController)
    {
        isOn = !isOn; // Toggle the state of the ignition key

        if (isOn)
        {
            // Rotate the key back to the "off" position
            key1.localRotation = Quaternion.Euler(0, 0, 0); // Adjust the angle as needed
            stateMachine.SwitchState(new PilotingState(BoatController, inventoryToggleManager, playerController)); // Viktig linje. Den sender båtens kontroller til spillerens state machine som setter han i pilotmode til den båten. Konsulter drawio filen for visualisering

        }
        else
        {
            // Rotate the key to the "on" position
            key1.localRotation = Quaternion.Euler(0, 90, 0); // Adjust the angle as needed
            stateMachine.SwitchState(new MovementState(playerController)); // endre tilbake til vanlig movement state når player skrur nøkkelen av 
        }
    }
}
