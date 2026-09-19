using FishNet.Object;

public class InteractableObject : NetworkBehaviour
{
    public bool pickupEnabled;

    public virtual void Interact(PlayerContext playerContext, SelectionManager selectionManager)
    {
        // Base interactables do nothing unless a child class implements behavior.
    }
}
