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

        GridInteract interact = EnsureGridInteract(uiElement);
        if (interact == null)
            return;

        inventoryVisibilityController.AddHUD(interact.gameObject);
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

        GridInteract interact = EnsureGridInteract(uiElement);
        if (interact == null)
            return;

        inventoryVisibilityController.AddPlayerScreen(interact.gameObject);
    }

    // Finds the GridInteract component that reports pointer enter/exit to the inventory controller.
    private void InitializeGridInteract(Transform uiElement)
    {
        gridInteract = EnsureGridInteract(uiElement);
    }

    // Registers every known inventory grid for direct pointer lookup, even if the panel is toggled elsewhere.
    private void RegisterKnownInventoryGrids()
    {
        foreach (Transform screen in playerScreens)
            EnsureGridInteract(screen);
    }

    // Adds the hover/click helper to valid grid objects when the prefab has not been wired manually.
    private GridInteract EnsureGridInteract(Transform uiElement)
    {
        if (uiElement == null)
            return null;

        GameObject greyGrid = uiReferences != null ? uiReferences.GetGridObject(uiElement) : null;
        if (greyGrid == null)
            return null;

        ItemGrid itemGrid = greyGrid.GetComponent<ItemGrid>();
        if (itemGrid == null)
        {
            Debug.LogWarning($"GreyGrid under {uiElement.name} is missing ItemGrid.", this);
            return null;
        }

        GridInteract interact = greyGrid.GetComponent<GridInteract>();
        if (interact == null)
            interact = greyGrid.AddComponent<GridInteract>();

        RegisterItemGrid(itemGrid);
        return interact;
    }

    // Debug/prototype helper for bulk registering every known inventory screen.
    private void InsertAllUIElements()
    {
        foreach (Transform screen in playerScreens)
            AddUIInventory(screen);
    }
}
