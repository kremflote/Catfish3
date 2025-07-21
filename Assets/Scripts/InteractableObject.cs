using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using StarterAssets;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class InteractableObject : NetworkBehaviour
{
    public bool playerInRange;
    public bool pickupEnabled;

    public FirstPersonController firstPersonController { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // må overrides i hver child av denne klassen
    public virtual void Interact(PlayerStateMachine playerState, InventoryToggleManager inventoryToggleManager, FirstPersonController playerController)
    {
        throw new NotImplementedException();
    }

    internal void PickUp()
    {
        throw new NotImplementedException();
    }
}