using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using StarterAssets;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InteractableObject : NetworkBehaviour
{
    public bool pickupEnabled;

    public virtual void Interact(PlayerStateMachine playerState, InventoryToggleManager inventoryToggleManager, FirstPersonController playerController, SelectionManager selectionManager)
    {
        throw new NotImplementedException();
    }

    internal void PickUp()
    {
        throw new NotImplementedException();
    }
}