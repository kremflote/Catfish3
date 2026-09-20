using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class InventoryToggleManager : MonoBehaviour
{
    public GameObject[] playerScreens;
    public List<GameObject> playerHUDs = new List<GameObject>();
    private bool isOpen = false;
    private bool isHUDVisible = true;
    [SerializeField] private PlayerInputState playerInputState;


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

    public bool GetIsOpen()
    {
        return isOpen;
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

    // Opens/closes registered inventory screens and inversely toggles the normal HUD.
    void ToggleInventory()
    {
        if (playerScreens == null)
        {
            Debug.LogWarning("playerScreens is null. Cannot toggle inventory.");
            return;
        }
        isOpen = !isOpen;
        foreach (GameObject screen in playerScreens)
        {
                
                if (screen != null)
                    screen.SetActive(isOpen);
                
        }
        ToggleHUD(!isOpen);
    }

    // Registers an always-visible HUD element, such as the hotbar, so it can hide while inventory is open.
    public void AddHUD(GameObject hudToAdd)
    {
        if (hudToAdd == null)
        {
            Debug.LogWarning("Attempted to add a null HUD element.");
            return;
        }

        if (playerHUDs.Contains(hudToAdd))
        {
            Debug.LogWarning($"{hudToAdd.name} is already in the HUD list.");
            return;
        }

        playerHUDs.Add(hudToAdd);
        if (isHUDVisible) // Ensure newly added HUD is visible if HUDs are currently visible
        {
            hudToAdd.SetActive(true);
        }

    }

    // Removes a HUD element from the toggle list when UI is rebuilt or no longer relevant.
    public void RemoveHUD(GameObject hudToRemove)
    {
        if (hudToRemove == null) return;

        if (playerHUDs.Remove(hudToRemove))
        {
            Debug.Log($"Removed {hudToRemove.name} from the HUD list.");
        }
        else
        {
            Debug.LogWarning($"{hudToRemove.name} was not found in the HUD list.");
        }
    }

    // Shows or hides every registered HUD element together.
    public void ToggleHUD(bool show)
    {
        isHUDVisible = show;
        if (!isHUDVisible)
        {

        }
        foreach (GameObject hud in playerHUDs)
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
        if (playerScreens == null || screenToRemove == null) return;

        List<GameObject> screenList = new List<GameObject>(playerScreens);
        if (screenList.Remove(screenToRemove))
        {
            playerScreens = screenList.ToArray();
            Debug.Log($"Removed {screenToRemove.name} from playerScreens.");
        }
        else
        {
            Debug.LogWarning($"Screen {screenToRemove.name} was not found in playerScreens.");
        }
    }

    // Registers an inventory screen so it follows the inventory open/close state.
    public void AddPlayerScreen(GameObject screenToAdd)
    {
        if (screenToAdd == null)
        {
            return;
        }

        List<GameObject> screenList = new List<GameObject>(playerScreens ?? new GameObject[0]);

        if (screenList.Contains(screenToAdd))
        {

            return;
        }

        screenList.Add(screenToAdd);
        playerScreens = screenList.ToArray();

    }
}
