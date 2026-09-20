using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using StarterAssets;

public class InventoryVisibilityController : MonoBehaviour
{
    [FormerlySerializedAs("playerScreens")]
    [SerializeField] private List<GameObject> inventoryScreens = new List<GameObject>();
    [FormerlySerializedAs("playerHUDs")]
    [SerializeField] private List<GameObject> hudElements = new List<GameObject>();
    private bool isOpen = false;
    private bool isHUDVisible = true;
    [SerializeField] private PlayerInputState playerInputState;

    public bool IsOpen => isOpen;

    // Subscribes to the player's Tab/input event when this manager becomes active.
    void OnEnable()
    {
        if (playerInputState == null)
            InitializeReferences();

        if (playerInputState != null)
            playerInputState.OnTabPressed += ToggleInventory;
    }

    // Unsubscribes so disabled UI managers do not continue toggling screens.
    void OnDisable()
    {
        if (playerInputState != null)
            playerInputState.OnTabPressed -= ToggleInventory;
    }

    private void Awake()
    {
        InitializeReferences();
    }

    // Finds PlayerInputState on the spawned player hierarchy.
    private void InitializeReferences()
    {
        if (playerInputState == null)
            playerInputState = transform.root.GetComponentInChildren<PlayerInputState>(true);

        if (playerInputState == null)
            Debug.LogWarning("PlayerInputState reference is missing.", this);
    }

    // Flips inventory visibility, used by Tab input.
    public void ToggleInventory()
    {
        SetInventoryOpen(!isOpen);
    }

    // Explicitly opens inventory screens and hides normal HUD elements.
    public void OpenInventory()
    {
        SetInventoryOpen(true);
    }

    // Explicitly closes inventory screens and restores normal HUD elements.
    public void CloseInventory()
    {
        SetInventoryOpen(false);
    }

    // Applies one visibility state to every registered inventory/HUD object.
    public void SetInventoryOpen(bool open)
    {
        isOpen = open;

        foreach (GameObject screen in inventoryScreens)
        {
            if (screen != null)
                screen.SetActive(isOpen);
        }

        SetHUDVisible(!isOpen);
    }

    // Registers an always-visible HUD element, such as the hotbar, so it can hide while inventory is open.
    public void AddHUD(GameObject hudToAdd)
    {
        if (hudToAdd == null)
        {
            Debug.LogWarning("Attempted to add a null HUD element.");
            return;
        }

        if (hudElements.Contains(hudToAdd))
        {
            Debug.LogWarning($"{hudToAdd.name} is already in the HUD list.");
            return;
        }

        hudElements.Add(hudToAdd);
        if (isHUDVisible) // Ensure newly added HUD is visible if HUDs are currently visible
        {
            hudToAdd.SetActive(true);
        }

    }

    // Removes a HUD element from the toggle list when UI is rebuilt or no longer relevant.
    public void RemoveHUD(GameObject hudToRemove)
    {
        if (hudToRemove == null) return;

        if (hudElements.Remove(hudToRemove))
        {
            Debug.Log($"Removed {hudToRemove.name} from the HUD list.");
        }
        else
        {
            Debug.LogWarning($"{hudToRemove.name} was not found in the HUD list.");
        }
    }

    // Shows or hides every registered HUD element together.
    public void SetHUDVisible(bool show)
    {
        isHUDVisible = show;
        foreach (GameObject hud in hudElements)
        {
            if (hud != null)
            {
                hud.SetActive(show);
            }
        }
    }

    // Removes a full-screen inventory panel from the open/close list.
    public void RemovePlayerScreen(GameObject screenToRemove)
    {
        if (screenToRemove == null) return;

        if (inventoryScreens.Remove(screenToRemove))
        {
            Debug.Log($"Removed {screenToRemove.name} from inventory screens.");
        }
        else
        {
            Debug.LogWarning($"Screen {screenToRemove.name} was not found in inventory screens.");
        }
    }

    // Registers an inventory screen so it follows the inventory open/close state.
    public void AddPlayerScreen(GameObject screenToAdd)
    {
        if (screenToAdd == null)
        {
            return;
        }

        if (inventoryScreens.Contains(screenToAdd))
        {

            return;
        }

        inventoryScreens.Add(screenToAdd);
        screenToAdd.SetActive(isOpen);
    }
}
