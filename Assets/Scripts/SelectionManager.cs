using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Globalization;
using FishNet.Object;


public class SelectionManager : NetworkBehaviour
{
    public bool onTarget;
    public GameObject selectedObject;
    public PlayerInput _playerInput;
    public PlayerStateMachine StateMachine;
    public InventoryToggleManager InventoryToggleManager;
    public FirstPersonController FirstPersonController;
    public Camera mainCamera;

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
        if (!IsOwner) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable && interactable.playerInRange)
            {
                onTarget = true;
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
        Debug.Log("OnInteract called with value: " + value.isPressed);

        if (onTarget && selectedObject != null)
        {
            InteractableObject interactable = selectedObject.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                Debug.Log("Interacting with: " + interactable.name + FirstPersonController);
                interactable.Interact(StateMachine, InventoryToggleManager, FirstPersonController);
            }
        }
    }
}