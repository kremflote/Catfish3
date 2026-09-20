using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemGrid : MonoBehaviour
{   //This class handles icon behavour on the itemgrid gameobject it is placed on

    // used for sizing and positioning in the UI
    RectTransform rectTransform;
    private InventoryGridModel model;
    private readonly Dictionary<InventoryItemEntry, InventoryItemUI> itemViews = new Dictionary<InventoryItemEntry, InventoryItemUI>();
    // mouse's position relative to grid
    Vector2 positionOnTheGrid = new Vector2();
    // Stores the calculated grid coordinates (tile X and Y) based on mouse position
    Vector2Int tileGridPosition = new Vector2Int();
    // the amount of squares in the itemGrid per axiom. aka this is a 3 by 6 grid
    [SerializeField] int gridSizeWidth = 6;
    [SerializeField] int gridSizeHeight = 3;
    // the base from which to generate items
    [SerializeField] GameObject inventoryItemPrefab;
    // the size of each tile in pixels
    public const float tileSizeWidth = 70;
    public const float tileSizeHeight = 70;
    
    // Unity calls this when the grid UI object wakes; it creates the backing model and sizes the RectTransform.
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Init(gridSizeWidth, gridSizeHeight);
    }

    public int GetGridSizeWidth()
    {
        return gridSizeWidth;
    }

    public int GetGridSizeHeight()
    {
        return gridSizeHeight;
    }

    // Creates the non-visual grid model and makes the UI rectangle match the tile count.
    private void Init(int width, int heigth)
    {
        model = new InventoryGridModel(width, heigth);
        Vector2 size = new Vector2(width * tileSizeWidth, heigth * tileSizeHeight);
        rectTransform.sizeDelta = size;
    }

    // Converts a model entry at a tile into its visible UI icon, if that icon exists.
    public InventoryItemUI GetItemAt(int x, int y)
    {
        InventoryItemEntry entry = GetEntryAt(x, y);
        return entry != null && itemViews.TryGetValue(entry, out InventoryItemUI item) ? item : null;
    }

    // Returns the data entry at a tile; this is the model-level version of GetItemAt.
    public InventoryItemEntry GetEntryAt(int x, int y)
    {
        return model.GetEntry(x, y);
    }

    // Removes an item entry from the model and returns its UI icon so the cursor can hold it.
    public InventoryItemUI PickUpItem(int x, int y)
    {
        InventoryItemEntry entry = model.PickUpEntry(x, y);
        return entry != null && itemViews.TryGetValue(entry, out InventoryItemUI item) ? item : null;
    }

    // Validates placement first, returning any items that block the requested spot.
    public bool PlaceItem(InventoryItemUI itemUI, int posX, int posY, out List<InventoryItemUI> overlappingItems)
    {
        overlappingItems = null;
        if (itemUI == null)
            return false;

        InventoryPlacementResult checkResult = PlacementCheck(posX, posY, itemUI.Width, itemUI.Height);
        overlappingItems = GetItemViews(checkResult.overlappingEntries);

        if (checkResult.outOfBounds)
        {
            Debug.Log($"Item is out of bounds by {checkResult.overflowX} on X and {checkResult.overflowY} on Y.");
            return false;
        }
        else if (checkResult.collisionWithObject)
        {
            Debug.Log($"Item collision with another item");
            return false;
        }

        PlaceItem(itemUI, posX, posY);

        return true;
    }

    // Places an item even when other items are in the way, returning displaced icons for cursor queueing.
    public bool PlaceItemAndDisplace(InventoryItemUI itemUI, int posX, int posY, out List<InventoryItemUI> displacedItems)
    {
        displacedItems = null;
        if (itemUI == null)
            return false;

        InventoryPlacementResult checkResult = PlacementCheck(posX, posY, itemUI.Width, itemUI.Height);
        if (checkResult.outOfBounds)
        {
            Debug.Log($"Item is out of bounds by {checkResult.overflowX} on X and {checkResult.overflowY} on Y.");
            return false;
        }

        displacedItems = GetItemViews(checkResult.overlappingEntries);
        if (displacedItems != null)
        {
            foreach (InventoryItemUI displacedItem in displacedItems)
            {
                ClearItem(displacedItem);
            }
        }

        PlaceItem(itemUI, posX, posY);
        return true;
    }

    // Tries to merge a held item into the stack under a specific tile.
    public bool TryStackItemAt(InventoryItemUI sourceItem, int posX, int posY)
    {
        InventoryItemEntry sourceEntry = sourceItem != null ? sourceItem.EnsureEntry() : null;
        InventoryItemEntry targetEntry = model.GetEntry(posX, posY);

        if (!TryMoveQuantity(sourceEntry, targetEntry))
            return false;

        RefreshItemView(sourceEntry);
        RefreshItemView(targetEntry);
        return true;
    }

    // Tries to move only part of a held stack into the stack under a specific tile.
    public bool TryStackItemAt(InventoryItemUI sourceItem, int posX, int posY, int amount)
    {
        InventoryItemEntry sourceEntry = sourceItem != null ? sourceItem.EnsureEntry() : null;
        InventoryItemEntry targetEntry = model.GetEntry(posX, posY);

        if (!TryMoveQuantity(sourceEntry, targetEntry, amount))
            return false;

        RefreshItemView(sourceEntry);
        RefreshItemView(targetEntry);
        return true;
    }

    // Tries to top up all matching stacks already in this grid before using empty space.
    public bool TryStackIntoExistingItems(InventoryItemUI sourceItem)
    {
        InventoryItemEntry sourceEntry = sourceItem != null ? sourceItem.EnsureEntry() : null;
        if (sourceEntry == null || !sourceEntry.IsStackable)
            return false;

        bool movedAny = false;
        foreach (InventoryItemEntry targetEntry in model.GetEntries())
        {
            if (sourceEntry.IsEmpty)
                break;

            if (TryMoveQuantity(sourceEntry, targetEntry))
            {
                movedAny = true;
                RefreshItemView(targetEntry);
            }
        }

        if (movedAny)
            RefreshItemView(sourceEntry);

        return sourceEntry.IsEmpty;
    }

    // Places the model entry and moves the UI icon to the matching tile position.
    public void PlaceItem(InventoryItemUI itemUI, int posX, int posY)
    {
        if (itemUI == null)
            return;

        InventoryItemEntry entry = itemUI.EnsureEntry();
        if (entry == null || !PositionCheck(posX, posY, entry.Width, entry.Height))
            return;

        // Parent the icon under this grid so local UI coordinates match the tile layout.
        RectTransform rectTransform = itemUI.GetComponent<RectTransform>();
        rectTransform.SetParent(this.rectTransform, false);
        rectTransform.localScale = Vector3.one;

        model.PlaceEntry(entry, posX, posY);
        itemViews[entry] = itemUI;

        Vector2 position = CalculatePositionOnGrid(itemUI, posX, posY);

        rectTransform.localPosition = position;
    }

    // Calculates the icon center position for an item that may cover multiple tiles.
    public Vector2 CalculatePositionOnGrid(InventoryItemUI itemUI, int posX, int posY)
    {
        Vector2 position = new Vector2();
        position.x = posX * tileSizeWidth + tileSizeWidth * itemUI.Width / 2f;
        position.y = -(posY * tileSizeHeight + tileSizeHeight * itemUI.Height / 2f);
        return position;
    }

    // Calculates the icon center position for a single tile highlight.
    public Vector2 CalculatePositionOnGrid(int posX, int posY)
    {
        Vector2 position = new Vector2();
        position.x = posX * tileSizeWidth + tileSizeWidth / 2f;
        position.y = -(posY * tileSizeHeight + tileSizeHeight / 2f);
        return position;
    }

    // Converts a screen-space pointer position into tile coordinates inside this UI grid.
    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        Camera eventCamera = GetEventCamera();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePosition, eventCamera, out Vector2 localPoint);

        positionOnTheGrid.x = localPoint.x + rectTransform.rect.width * rectTransform.pivot.x;
        positionOnTheGrid.y = rectTransform.rect.height * (1f - rectTransform.pivot.y) - localPoint.y;

        tileGridPosition.x = Mathf.FloorToInt(positionOnTheGrid.x / tileSizeWidth);
        tileGridPosition.y = Mathf.FloorToInt(positionOnTheGrid.y / tileSizeHeight);
        return tileGridPosition;
    }

    // Returns the camera needed by Unity UI coordinate conversion; overlay canvases use null.
    private Camera GetEventCamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera;
    }

    // Asks the model for the first empty area that fits this item shape.
    public Vector2Int? FindSpaceForObject(InventoryItemUI itemToInsert)
    {
        InventoryItemEntry entry = itemToInsert != null ? itemToInsert.EnsureEntry() : null;
        return model.FindSpaceForObject(entry);
    }

    // Checks whether a rectangular item footprint can be placed at a tile.
    public bool CheckAvailableSpace(int posX, int posY, int width, int height)
    {
        return model.CheckAvailableSpace(posX, posY, width, height);
    }

    // Creates save-friendly data for every unique item currently in this grid.
    public List<InventoryItemSnapshot> CreateSnapshot()
    {
        return model.CreateSnapshot();
    }

    // Returns visible item icons once each, useful for controller-level stack actions.
    public List<InventoryItemUI> GetItems()
    {
        List<InventoryItemUI> items = new List<InventoryItemUI>();
        foreach (InventoryItemEntry entry in model.GetEntries())
        {
            if (entry != null && itemViews.TryGetValue(entry, out InventoryItemUI item))
                items.Add(item);
        }

        return items;
    }

    // Clears the grid model and optionally destroys the UI item icons, used before loading a save.
    public void ClearAllItems(bool destroyItemViews)
    {
        foreach (InventoryItemUI item in itemViews.Values)
        {
            if (destroyItemViews && item != null)
                Destroy(item.gameObject);
        }

        itemViews.Clear();
        model.ClearAll();
    }

    public bool PositionCheck(int posX, int posY)
    {
        return model.PositionCheck(posX, posY);
    }

    public bool PositionCheck(int posX, int posY, int width, int height)
    {
        return model.PositionCheck(posX, posY, width, height);
    }

    // Returns detailed placement info for highlights and swap behavior.
    public InventoryPlacementResult PlacementCheck(int posX, int posY, int width, int height)
    {
        return model.PlacementCheck(posX, posY, width, height);
    }

    // Clears whichever item occupies a specific tile in the grid.
    internal void ClearSlot(int i, int targetRow)
    {
        ClearEntry(model.GetEntry(i, targetRow));
    }

    // Clears an item view from the model while leaving the icon object alive.
    internal void ClearItem(InventoryItemUI item)
    {
        if (item == null)
            return;

        ClearEntry(item.EnsureEntry());
    }

    // Clears an entry from the backing model; the UI lookup remains so pickup/replace can reuse the icon.
    internal void ClearEntry(InventoryItemEntry entry)
    {
        model.ClearEntry(entry);
    }

    // Converts model collision entries back into UI icons for controller-level swap logic.
    private List<InventoryItemUI> GetItemViews(List<InventoryItemEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return null;

        List<InventoryItemUI> items = new List<InventoryItemUI>();
        foreach (InventoryItemEntry entry in entries)
        {
            if (entry != null && itemViews.TryGetValue(entry, out InventoryItemUI item))
                items.Add(item);
        }

        return items;
    }

    // Moves quantity between compatible stacks, leaving item placement untouched.
    private bool TryMoveQuantity(InventoryItemEntry sourceEntry, InventoryItemEntry targetEntry)
    {
        return TryMoveQuantity(sourceEntry, targetEntry, sourceEntry != null ? sourceEntry.Quantity : 0);
    }

    // Moves a requested quantity between compatible stacks, leaving item placement untouched.
    private bool TryMoveQuantity(InventoryItemEntry sourceEntry, InventoryItemEntry targetEntry, int amount)
    {
        if (sourceEntry == null || targetEntry == null || sourceEntry == targetEntry)
            return false;

        return sourceEntry.TransferQuantityTo(targetEntry, amount) > 0;
    }

    // Updates a visible item icon after its underlying entry changes quantity.
    public void RefreshItemView(InventoryItemEntry entry)
    {
        if (entry != null && itemViews.TryGetValue(entry, out InventoryItemUI item))
            item.Refresh();
    }

    public struct PlacementOutcome
    {
        public bool success;
        public InventoryItemUI overlappingItem;

        public PlacementOutcome(bool success, InventoryItemUI overlappingItem)
        {
            this.success = success;
            this.overlappingItem = overlappingItem;
        }
    }
}
