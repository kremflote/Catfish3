using UnityEngine;

public partial class InventoryController
{
    // Finds the known inventory screen transforms under the player's canvas.
    private void ResolveInventoryScreens(Transform canvas)
    {
        uiReferences.ResolveMissingReferences(canvas);

        expandableBotleft = expandableBotleft != null ? expandableBotleft : uiReferences.ExpandableBotleft;
        expandableBotright = expandableBotright != null ? expandableBotright : uiReferences.ExpandableBotright;
        expandableTopleft = expandableTopleft != null ? expandableTopleft : uiReferences.ExpandableTopleft;
        expandableTopright = expandableTopright != null ? expandableTopright : uiReferences.ExpandableTopright;
        playerScreen = playerScreen != null ? playerScreen : uiReferences.PlayerScreen;
        worldScreen = worldScreen != null ? worldScreen : uiReferences.WorldScreen;
        itemDescriptionScreen = itemDescriptionScreen != null ? itemDescriptionScreen : uiReferences.ItemDescriptionScreen;
        hotbar = hotbar != null ? hotbar : uiReferences.Hotbar;
    }

    // Builds the row highlight that marks which inventory row is mirrored into the hotbar.
    private void InitializeHotbarHighlight()
    {
        Transform player = transform.root;

        if (inventoryHotbarHighlight == null)
            inventoryHotbarHighlight = GetOrCreateHotbarHighlight();

        Transform canvas = canvasTransform != null ? canvasTransform : player.GetComponentInChildren<Canvas>(true)?.transform;
        if (canvas != null)
            ResolveInventoryScreens(canvas);

        ItemGrid eblItemGrid = uiReferences != null ? uiReferences.GetItemGrid(expandableBotleft) : null;

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
        inventoryHotbarHighlight.SetPosition(eblItemGrid, 0, hotbarInventoryRow);
    }

    // Reuses an existing highlighter instance if present, otherwise creates one for hotbar-row highlighting.
    private InventoryHighlighter GetOrCreateHotbarHighlight()
    {
        InventoryHighlighter[] highlighters = transform.root.GetComponentsInChildren<InventoryHighlighter>(true);
        foreach (InventoryHighlighter highlighter in highlighters)
        {
            if (highlighter != null && highlighter != inventoryHighlight)
                return highlighter;
        }

        return gameObject.AddComponent<InventoryHighlighter>();
    }

    // Registers HUD elements with the toggle manager so they hide while inventory screens are open.
    private void AddUIHUD(Transform uiElement)
    {
        if (inventoryVisibilityController == null || uiElement == null)
            return;

        inventoryVisibilityController.AddHUD(uiElement.gameObject);
        InitializeGridInteract(uiElement);
        InitializeGridToHUD(uiElement);
    }

    // Registers a grid that belongs to HUD mode, such as the always-visible hotbar.
    private void InitializeGridToHUD(Transform uiElement)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("InitializeGrid was called with a null uiElement.");
            return;
        }

        GameObject greyGrid = uiReferences != null ? uiReferences.GetGridObject(uiElement) : null;
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

        inventoryVisibilityController.AddHUD(greyGrid);
    }

    // Registers a player inventory screen so it opens/closes with the inventory toggle.
    private void AddUIInventory(Transform uiElement)
    {
        if (inventoryVisibilityController == null || uiElement == null)
            return;

        inventoryVisibilityController.AddPlayerScreen(uiElement.gameObject);
        InitializeGridInteract(uiElement);
        InitializeGridToInventory(uiElement);
    }

    // Registers the grid inside an inventory screen so clicks and hover can target it.
    private void InitializeGridToInventory(Transform uiElement)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("InitializeGrid was called with a null uiElement.");
            return;
        }

        GameObject greyGrid = uiReferences != null ? uiReferences.GetGridObject(uiElement) : null;
        if (greyGrid == null)
            return;

        GridInteract interact = greyGrid.GetComponent<GridInteract>();
        if (interact == null)
        {
            Debug.LogWarning($"GridInteract component missing on GreyGrid under {uiElement.name}.");
            return;
        }

        inventoryVisibilityController.AddPlayerScreen(greyGrid);
    }

    // Finds the GridInteract component that reports pointer enter/exit to the inventory controller.
    private void InitializeGridInteract(Transform uiElement)
    {
        gridInteract = uiReferences != null ? uiReferences.GetGridInteract(uiElement) : null;
        if (gridInteract == null)
            Debug.LogError("GridInteract component is missing on GreyGrid.");
    }

    // Debug/prototype helper for bulk registering every known inventory screen.
    private void InsertAllUIElements()
    {
        foreach (Transform screen in playerScreens)
            AddUIInventory(screen);
    }
}
