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

    public override void Interact(PlayerContext playerContext, SelectionManager selectionManager)
    {
        Debug.Log("Interacting with Ignition Key 1");
        HandleKeyInsertion();
        ToggleIgnition(playerContext, selectionManager);
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
