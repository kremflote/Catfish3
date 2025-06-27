using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using UnityEngine.InputSystem;


public class SelectionManager : MonoBehaviour
{
    public bool onTarget;
    public GameObject selectedObject;
    public PlayerInput _playerInput;
    public FirstPersonController firstPersonController;

    private void Start()
    {
        onTarget = false;
        GameObject cameraGO = GameObject.Find("MainCamera");
        if (cameraGO != null)
        {
            foreach (Transform child in cameraGO.transform)
            {
                _playerInput = child.GetComponent<PlayerInput>();
                if (_playerInput != null)
                {
                    break; // Found it, stop searching
                }
            }

            if (_playerInput == null)
            {
                Debug.LogWarning("No PlayerInput found in children of MainCamera.");
            }
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable && interactable.playerInRange)
            {
                onTarget = true;
                interactable.firstPersonController = this.firstPersonController;
                selectedObject = interactable.gameObject;
            }
            else
            {
                onTarget = false;
                selectedObject = null;
            }
        }
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && onTarget && selectedObject != null)
        {
            InteractableObject interactable = selectedObject.GetComponent<InteractableObject>();
            if (interactable != null && interactable.pickupEnabled)
            {
                // Handle interaction logic here, e.g., picking up the item
                // You can add more logic here to handle the interaction
            }

            else
            {
                interactable.Interact();
            }
        }
    }
}