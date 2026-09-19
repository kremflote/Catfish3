using StarterAssets;
using UnityEngine;

// Central place for scripts on the player root to find common player references.
[DisallowMultipleComponent]
public class PlayerContext : MonoBehaviour
{
    [SerializeField] private PlayerInputState input;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private InventoryToggleManager inventoryToggleManager;
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private Camera mainCamera;

    public PlayerInputState Input => input;
    public PlayerStateMachine StateMachine => stateMachine;
    public InventoryToggleManager InventoryToggleManager => inventoryToggleManager;
    public FirstPersonController FirstPersonController => firstPersonController;
    public Camera MainCamera => mainCamera;

    private void Awake()
    {
        ResolveReferences();
    }

    public void ResolveReferences()
    {
        if (input == null)
            input = GetComponentInChildren<PlayerInputState>(true);

        if (stateMachine == null)
            stateMachine = GetComponent<PlayerStateMachine>();

        if (inventoryToggleManager == null)
            inventoryToggleManager = GetComponentInChildren<InventoryToggleManager>(true);

        if (firstPersonController == null)
            firstPersonController = GetComponentInChildren<FirstPersonController>(true);

        if (mainCamera == null)
            mainCamera = GetComponentInChildren<Camera>(true);
    }
}
