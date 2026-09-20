using StarterAssets;
using UnityEngine;

// Central place for scripts on the player root to find common player references.
[DisallowMultipleComponent]
public class PlayerContext : MonoBehaviour
{
    [SerializeField] private PlayerInputState input;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private InventoryVisibilityController inventoryVisibilityController;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private Camera mainCamera;

    public PlayerInputState Input => input;
    public PlayerStateMachine StateMachine => stateMachine;
    public InventoryVisibilityController InventoryVisibilityController => inventoryVisibilityController;
    public FirstPersonController FirstPersonController => firstPersonController;
    public Camera MainCamera => mainCamera;

    // Resolves references as soon as Unity creates this component.
    private void Awake()
    {
        ResolveReferences();
    }

    // Finds important player components once so other scripts do not repeat hierarchy searches.
    public void ResolveReferences()
    {
        if (input == null)
            input = GetComponentInChildren<PlayerInputState>(true);

        if (stateMachine == null)
            stateMachine = GetComponent<PlayerStateMachine>();

        if (inventoryVisibilityController == null)
            inventoryVisibilityController = GetComponentInChildren<InventoryVisibilityController>(true);

        if (firstPersonController == null)
            firstPersonController = GetComponentInChildren<FirstPersonController>(true);

        if (mainCamera == null)
            mainCamera = GetComponentInChildren<Camera>(true);
    }
}
