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

// Handles what the local player is looking at and routes interact input.
public class SelectionManager : NetworkBehaviour
{
    public bool onTarget;
    public GameObject selectedObject;
    public GameObject draggingObject;
    public PlayerInput _playerInput;
    public PlayerInputState _input;
    public PlayerStateMachine StateMachine;
    public InventoryToggleManager InventoryToggleManager;
    public FirstPersonController FirstPersonController;
    public Camera mainCamera;
    public float maxDistance = 5f;

    private void Start()
    {
        onTarget = false;

        if (mainCamera == null)
            mainCamera = GetComponentInChildren<Camera>(true);

        if (_playerInput == null)
            _playerInput = GetComponentInChildren<PlayerInput>(true);

        if (mainCamera == null)
            Debug.LogWarning("No Camera reference found for SelectionManager.", this);

        if (_playerInput == null)
            Debug.LogWarning("No PlayerInput reference found for SelectionManager.", this);
    }

    public void HandleSelection()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Ignore UI/visor geometry so selection hits world objects.
        int mask = ~LayerMask.GetMask("Visor");


        if (Physics.Raycast(ray, out hit, maxDistance, mask))
        {

            var selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            // Some interactables live on a parent while the ray hits a child mesh.
            if (interactable == null)
            {
                interactable = selectionTransform.GetComponentInParent<InteractableObject>();
            }

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
        if (onTarget && selectedObject != null)
        {
            InteractableObject interactable = selectedObject.GetComponent<InteractableObject>();
            if (interactable != null)
            {
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
                FirstPersonController.canRotate = false;
                throttle.Move(Mouse.current.delta.ReadValue().y);
            }
            if (!_input.clickHeld)
            {
                FirstPersonController.canRotate = true;
                currentMovableObject = null;
            }
        }
    }

    private void HandleGear()
    {
        if (currentMovableObject is Gear gear)
        {
            if (_input.clickHeld)
            {
                // Movable cockpit controls use mouse drag instead of camera look.
                FirstPersonController.canRotate = false;
                if (!gearClicked)
                {
                    gearClicked = true;
                }

                gear.Move(Mouse.current.delta.ReadValue().y);

            }
            if (!_input.clickHeld && gearClicked)
            {
                FirstPersonController.canRotate = true;
                gearClicked = false;
                gear.UpdateGearState();
                gear.moved = true;
                currentMovableObject = null;
            }
        }
    }
}
