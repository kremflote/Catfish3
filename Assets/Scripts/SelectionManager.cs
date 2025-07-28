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
using UnityEditor.Timeline.Actions;


public class SelectionManager : NetworkBehaviour
{
    public bool onTarget;
    public GameObject selectedObject;
    public GameObject draggingObject;
    public PlayerInput _playerInput;
    public StarterAssetsInputs _input;
    public PlayerStateMachine StateMachine;
    public InventoryToggleManager InventoryToggleManager;
    public FirstPersonController FirstPersonController;
    public Camera mainCamera;
    public float maxDistance = 5f; // Maximum distance for interaction

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

    public void HandleSelection()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Use a mask to ignore objects in the "Visor" layer
        int mask = ~LayerMask.GetMask("Visor");


        if (Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            
            var selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();
            // If not found on hit object, try its parent
            if (interactable == null)
            {
                interactable = selectionTransform.GetComponentInParent<InteractableObject>();
            }

            // Debug.Log("Raycast hit: " + selectionTransform.name + " with interactable: " + (interactable != null ? interactable.name : "null"));

            if (interactable)
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
        // Debug.Log("OnInteract called with value: " + value.isPressed);

        if (onTarget && selectedObject != null)
        {
            InteractableObject interactable = selectedObject.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                // Debug.Log("Interacting with: " + interactable.name + FirstPersonController);
                interactable.Interact(StateMachine, InventoryToggleManager, FirstPersonController, this);
            }
        }
    }

    private Vector2 previousMousePosition;
    private Vector2 currentMousePosition;
    private int frameCounter = 0;
    private int frameDelay = 5;
    private bool gearClicked = false;
    private MovableObject currentMovableObject;
    public void CheckMovableObject()
    {
        if (onTarget && selectedObject != null)
        {
            MovableObject movableObject = selectedObject.GetComponent<MovableObject>();
            currentMousePosition = Vector2.zero;
            currentMovableObject = movableObject;
        }
    }

    public void HandleMovableObject()
    {
        HandleGear();
        HandleThrottle();
    }

    private void HandleThrottle()
    {
        if (currentMovableObject is Throttle throttle)
        {
            if (_input.clickHeld)
            {
                FirstPersonController.canRotate = false; // disable camera rotation while interacting
                throttle.Move(Mouse.current.delta.ReadValue().y);
            }
            if (!_input.clickHeld)
            {
                FirstPersonController.canRotate = true;
                currentMovableObject = null; // reset the current movable object
            }
        }
    }

    private void HandleGear()
    {
        if (currentMovableObject is Gear gear)
        {
            if (_input.clickHeld)
            {
                FirstPersonController.canRotate = false; // disable camera rotation while interacting with gear
                if (!gearClicked)
                {
                    gearClicked = true;
                }

                // gear.move y axis based on mouse y position
                gear.Move(Mouse.current.delta.ReadValue().y);

                // lock mouse to gear's position

            }
            if (!_input.clickHeld && gearClicked)
            {
                FirstPersonController.canRotate = true;
                gearClicked = false;
                gear.UpdateGearState(); // when player releases the mouse button, update the gear's state
                gear.moved = true; // mark that the gear has been moved
                currentMovableObject = null; // reset the current movable object
            }
        }
    }
}