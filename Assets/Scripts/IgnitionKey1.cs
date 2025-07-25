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

    public MeshRenderer key1MeshRenderer; 

    public BoatController BoatController;

    public override void Interact(PlayerStateMachine stateMachine, InventoryToggleManager inventoryToggleManager, FirstPersonController playerController, SelectionManager selectionManager)
    {
        Debug.Log("Interacting with Ignition Key 1");
        HandleKeyInsertion();
        ToggleIgnition(stateMachine, inventoryToggleManager, playerController, selectionManager);
    }

    private void HandleKeyInsertion()
    {
        if (!isIn)
        {
            bool success = TryInsertKey();
            if (success)
            {
                isIn = true;
                key1MeshRenderer.enabled = true; 
            }
        }
    }

    private bool TryInsertKey()
    {
        return true; // This method can be expanded to include logic for checking if the key can be inserted
    }

    public void InsertKey()
    {
        isIn = true; 
        key1.localRotation = Quaternion.Euler(0, 0, 0); 
        Debug.Log("Key inserted into ignition.");
    }

    public bool IsOn()
    {
        return isOn; 
    }

    public bool IsIn()
    {
        return isIn; 
    }

    public void ToggleIgnition(PlayerStateMachine stateMachine, InventoryToggleManager inventoryToggleManager, FirstPersonController playerController, SelectionManager selectionManager)
    {
        isOn = !isOn;

        if (isOn)
        {
            // Rotate the key back to the "off" position
            key1.localRotation = Quaternion.Euler(0, 90, 0);
            stateMachine.SwitchState(new MovementState(playerController)); // endre tilbake til vanlig movement state når player skrur nøkkelen av 

        }
        else
        {
            // Rotate the key to the "on" position
            key1.localRotation = Quaternion.Euler(0, 0, 0);
            stateMachine.SwitchState(new PilotingState(BoatController, inventoryToggleManager, playerController, selectionManager)); // Viktig linje. Den sender båtens kontroller til spillerens state machine som setter han i pilotmode til båten. Konsulter drawio filen for visualisering

        }
    }
}
