using System.Collections.Generic;
using UnityEngine;

public class InventoryToggleManager : MonoBehaviour
{
    public GameObject[] playerScreens;
    public List<GameObject> playerHUDs = new List<GameObject>();
    private bool isOpen = false;
    private bool isHUDVisible = true;
    InputManager inputManager;


    void OnEnable()
    {
        inputManager.OnTabPressed += ToggleInventory;
    }

    void OnDisable()
    {
        inputManager.OnTabPressed -= ToggleInventory;
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    private void Awake()
    {
        Transform Managers = transform.parent;
        Transform inputManagerTransform = Managers.Find("InputManager");
        inputManager = inputManagerTransform.GetComponent<InputManager>();
    }

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