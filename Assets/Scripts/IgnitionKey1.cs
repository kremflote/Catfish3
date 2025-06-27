using System;
using FishNet.Object;
using StarterAssets;
using UnityEngine;

public class IgnitionKey1 : InteractableObject
{
    public bool isOn = false; 
    public bool isIn = false; 

    [SerializeField] private Transform key1;

    public new void Interact()
    {
        if (!isIn)
        {
            bool success = TryInsertKey();
            if (success)
            {
                isIn = true;
                // fix some animation for the gameobject
            }
        }
        ToggleIgnition();
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

    public void ToggleIgnition()
    {
        isOn = !isOn; // Toggle the state of the ignition key

        if (isOn)
        {
            // Rotate the key to the "on" position
            key1.localRotation = Quaternion.Euler(0, 90, 0); // Adjust the angle as needed
        }
        else
        {
            // Rotate the key back to the "off" position
            key1.localRotation = Quaternion.Euler(0, 0, 0); // Adjust the angle as needed
        }
    }
}
