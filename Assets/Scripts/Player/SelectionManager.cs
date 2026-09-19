using UnityEngine;
using StarterAssets;
using FishNet.Object;

// Handles what the local player is looking at and routes interact input.
[RequireComponent(typeof(PlayerContext))]
public class SelectionManager : NetworkBehaviour
{
    [Header("Status")]
    [SerializeField] private bool onTarget;
    [SerializeField] private GameObject selectedObject;
    [SerializeField] private GameObject draggingObject;

    [Header("References")]
    [SerializeField] private PlayerContext context;
    [SerializeField] private float maxDistance = 5f;

    public bool IsUsingWorldInteraction { get; private set; }

    private void Start()
    {
        onTarget = false;
        ResolveContext();
    }

    public void HandleSelection()
    {
        if (IsUsingWorldInteraction)
            return;

        if (context == null || context.MainCamera == null || context.Input == null)
            return;

        Ray ray = context.MainCamera.ScreenPointToRay(context.Input.pointerPosition);
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
            return;
        }

        ClearSelection();
    }

    public void OnInteract(UnityEngine.InputSystem.InputValue value)
    {
        if (onTarget && selectedObject != null)
        {
            InteractableObject interactable = selectedObject.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.Interact(context, this);
            }
        }
    }

    private bool gearClicked = false;
    private MovableObject currentMovableObject;
    public void CheckMovableObject()
    {
        if (onTarget && selectedObject != null)
        {
            MovableObject movableObject = selectedObject.GetComponent<MovableObject>();
            currentMovableObject = movableObject;
        }
    }

    public void HandleMovableObject()
    {
        IsUsingWorldInteraction = false;
        HandleGear();
        HandleThrottle();
    }

    private void HandleThrottle()
    {
        if (currentMovableObject is Throttle throttle)
        {
            if (context.Input.clickHeld)
            {
                IsUsingWorldInteraction = true;
                throttle.Move(context.Input.pointerDelta.y, GetInputOwnerClientId());
            }
            if (!context.Input.clickHeld)
            {
                currentMovableObject = null;
            }
        }
    }

    private void HandleGear()
    {
        if (currentMovableObject is Gear gear)
        {
            if (context.Input.clickHeld)
            {
                // Movable cockpit controls use mouse drag instead of camera look.
                IsUsingWorldInteraction = true;
                if (!gearClicked)
                {
                    gearClicked = true;
                }

                gear.Move(context.Input.pointerDelta.y, GetInputOwnerClientId());

            }
            if (!context.Input.clickHeld && gearClicked)
            {
                gearClicked = false;
                gear.UpdateGearState();
                gear.moved = true;
                currentMovableObject = null;
            }
        }
    }

    private void ClearSelection()
    {
        onTarget = false;
        selectedObject = null;
    }

    private int GetInputOwnerClientId()
    {
        PlayerInputState input = context != null ? context.Input : null;
        return input != null && input.Owner.IsValid ? input.Owner.ClientId : -1;
    }

    private void ResolveContext()
    {
        if (context == null)
            context = GetComponent<PlayerContext>();

        if (context == null)
            context = gameObject.AddComponent<PlayerContext>();

        context.ResolveReferences();

        if (context.MainCamera == null)
            Debug.LogWarning("No Camera reference found for SelectionManager.", this);

        if (context.Input == null)
            Debug.LogWarning("No PlayerInputState reference found for SelectionManager.", this);
    }
}
