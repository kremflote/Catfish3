using UnityEngine;

public partial class InventoryController
{
    private void ResolveInventoryScreens(Transform canvas)
    {
        if (expandableBotleft == null)
            expandableBotleft = FindDescendantByName(canvas, "expandable_botleft");
        if (expandableBotright == null)
            expandableBotright = FindDescendantByName(canvas, "expandable_botright");
        if (expandableTopleft == null)
            expandableTopleft = FindDescendantByName(canvas, "expandable_topleft");
        if (expandableTopright == null)
            expandableTopright = FindDescendantByName(canvas, "expandable_topright");
        if (playerScreen == null)
            playerScreen = FindDescendantByName(canvas, "player_screen");
        if (worldScreen == null)
            worldScreen = FindDescendantByName(canvas, "world_screen");
        if (itemDescriptionScreen == null)
            itemDescriptionScreen = FindDescendantByName(canvas, "item_description_screen");
        if (hotbar == null)
            hotbar = FindDescendantByName(canvas, "hotbar");
    }

    private void InitializeHotbarHighlight()
    {
        Transform player = transform.root;

        if (inventoryHotbarHighlight == null)
            inventoryHotbarHighlight = player.GetComponentInChildren<MultipleHighlighter>(true);

        Transform canvas = canvasTransform != null ? canvasTransform : player.GetComponentInChildren<Canvas>(true)?.transform;
        if (canvas != null)
            ResolveInventoryScreens(canvas);

        Transform greyGrid = FindDescendantByName(expandableBotleft, "GreyGrid");
        ItemGrid eblItemGrid = greyGrid != null ? greyGrid.GetComponent<ItemGrid>() : null;

        if (inventoryHotbarHighlight == null || eblItemGrid == null || canvas == null)
        {
            Debug.LogWarning("Hotbar highlight references are incomplete.", this);
            return;
        }

        int numberOfHighlights = eblItemGrid.GetGridSizeWidth();

        inventoryHotbarHighlight.GenerateHighlighters(numberOfHighlights, canvas);
        inventoryHotbarHighlight.SetSize(1, 1);
        inventoryHotbarHighlight.SetBehind();
        inventoryHotbarHighlight.SetParent(eblItemGrid);
        inventoryHotbarHighlight.SetPosition(eblItemGrid, 0, 2);
    }

    private void AddUIHUD(Transform uiElement)
    {
        if (inventoryToggleManager == null || uiElement == null)
            return;

        inventoryToggleManager.AddHUD(uiElement.gameObject);
        InitializeGridInteract(uiElement);
        InitializeGridToHUD(uiElement);
    }

    private void InitializeGridToHUD(Transform uiElement)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("InitializeGrid was called with a null uiElement.");
            return;
        }

        Transform greyGrid = FindDescendantByName(uiElement, "GreyGrid");
        if (greyGrid == null)
        {
            Debug.LogWarning($"GreyGrid not found as a child of {uiElement.name}.");
            return;
        }

        GridInteract interact = greyGrid.GetComponent<GridInteract>();
        if (interact == null)
        {
            Debug.LogWarning($"GridInteract component missing on GreyGrid under {uiElement.name}.");
            return;
        }

        inventoryToggleManager.AddHUD(greyGrid.gameObject);
    }

    private void AddUIInventory(Transform uiElement)
    {
        if (inventoryToggleManager == null || uiElement == null)
            return;

        inventoryToggleManager.AddPlayerScreen(uiElement.gameObject);
        InitializeGridInteract(uiElement);
        InitializeGridToInventory(uiElement);
    }

    private void InitializeGridToInventory(Transform uiElement)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("InitializeGrid was called with a null uiElement.");
            return;
        }

        Transform greyGrid = FindDescendantByName(uiElement, "GreyGrid");
        if (greyGrid == null)
            return;

        GridInteract interact = greyGrid.GetComponent<GridInteract>();
        if (interact == null)
        {
            Debug.LogWarning($"GridInteract component missing on GreyGrid under {uiElement.name}.");
            return;
        }

        inventoryToggleManager.AddPlayerScreen(greyGrid.gameObject);
    }

    private void InitializeGridInteract(Transform uiElement)
    {
        Transform greyGrid = FindDescendantByName(uiElement, "GreyGrid");
        if (greyGrid == null)
            return;

        gridInteract = greyGrid.GetComponent<GridInteract>();
        if (gridInteract == null)
            Debug.LogError("GridInteract component is missing on GreyGrid.");
    }

    private void InsertAllUIElements()
    {
        foreach (Transform screen in playerScreens)
            AddUIInventory(screen);
    }
}
