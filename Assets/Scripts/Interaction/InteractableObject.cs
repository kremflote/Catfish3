using FishNet.Object;

public class InteractableObject : NetworkBehaviour
{
    public bool pickupEnabled;

    // Override this in child classes to define what happens when the player presses interact.
    public virtual void Interact(PlayerContext playerContext, SelectionManager selectionManager)
    {
        // Base interactables do nothing unless a child class implements behavior.
    }
}
