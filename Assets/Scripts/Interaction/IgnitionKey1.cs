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

    // Called by SelectionManager when the player interacts with the ignition key.
    public override void Interact(PlayerContext playerContext, SelectionManager selectionManager)
    {
        Debug.Log("Interacting with Ignition Key 1");
        HandleKeyInsertion();
        ToggleIgnition(playerContext, selectionManager);
    }

    // Inserts the key visual the first time the player interacts with this ignition.
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

    // Placeholder for later inventory/key validation; currently every interaction can insert the key.
    private bool TryInsertKey()
    {
        return true;
    }

    // Forces the key into the inserted state, useful for scripted setup or future inventory logic.
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

    // Toggles piloting/walking state and rotates the key visual to match the ignition state.
    public void ToggleIgnition(PlayerContext playerContext, SelectionManager selectionManager)
    {
        if (playerContext == null || playerContext.StateMachine == null || playerContext.FirstPersonController == null)
            return;

        isOn = !isOn;

        if (isOn)
        {
            key1.localRotation = Quaternion.Euler(0, 90, 0);
            playerContext.StateMachine.SwitchState(new MovementState(playerContext.FirstPersonController));

        }
        else
        {
            key1.localRotation = Quaternion.Euler(0, 0, 0);
            playerContext.StateMachine.SwitchState(new PilotingState(BoatController, playerContext.FirstPersonController, selectionManager));

        }
    }
}
