using StarterAssets;
using UnityEngine;

// Interactable key that swaps the local player between walking and piloting.
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
        return true;
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
            key1.localRotation = Quaternion.Euler(0, 90, 0);
            stateMachine.SwitchState(new MovementState(playerController));

        }
        else
        {
            key1.localRotation = Quaternion.Euler(0, 0, 0);
            stateMachine.SwitchState(new PilotingState(BoatController, inventoryToggleManager, playerController, selectionManager));

        }
    }
}
