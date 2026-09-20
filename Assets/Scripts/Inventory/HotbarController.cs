using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private PlayerInputState playerInputState;
    [SerializeField] private InventoryUIReferences uiReferences;
    [SerializeField] private ItemGrid itemGrid;
    [SerializeField] private InventoryHighlighter singleHighlighter;
    [SerializeField] private EquipmentManager equipmentManager;
    private int width;
    private InventoryItemEntry[,] hotbarEntries;
    private readonly Dictionary<InventoryItemEntry, InventoryItemUI> hotbarItemViews = new Dictionary<InventoryItemEntry, InventoryItemUI>();
    private int selectedIndex = 0;
    private InventoryItemUI selectedItem;
    private int lastIndex;
    private int lastSelectedItemID;

    // Unity calls this when the hotbar wakes; it finds related UI and subscribes to number-key input.
    private void Awake()
    {
        Transform player = transform.root;

        if (equipmentManager == null)
            equipmentManager = GetComponent<EquipmentManager>();

        if (singleHighlighter == null)
            singleHighlighter = player.GetComponentInChildren<InventoryHighlighter>(true);

        if (uiReferences == null)
            uiReferences = player.GetComponentInChildren<InventoryUIReferences>(true);

        if (itemGrid == null)
        {
            Transform canvas = player.GetComponentInChildren<Canvas>(true)?.transform;
            if (uiReferences == null && canvas != null)
                uiReferences = canvas.gameObject.AddComponent<InventoryUIReferences>();

            uiReferences?.ResolveMissingReferences(canvas);
            itemGrid = uiReferences != null ? uiReferences.GetItemGrid(uiReferences.Hotbar) : null;
        }

        if (playerInputState == null)
            playerInputState = player.GetComponentInChildren<PlayerInputState>(true);

        if (itemGrid == null)
        {
            Debug.LogError("Hotbar ItemGrid reference is missing.", this);
            enabled = false;
            return;
        }

        width = itemGrid.GetGridSizeWidth();
        InitializeHotbarItemSlots();

        if (playerInputState != null)
            playerInputState.OnHotbarKeyPressed += HandleHotbarKeyPress;
        else
            Debug.LogWarning("PlayerInputState reference is missing.", this);
    }

    // Creates the hotbar's entry array; hotbar stores item entries, not UI icons, as its source of truth.
    public void InitializeHotbarItemSlots()
    {
        hotbarEntries = new InventoryItemEntry[width, 1];
    }

    // Handles number-key selection, including equip/unequip behavior for the selected slot.
    private void HandleHotbarKeyPress(int index)
    {
        if (singleHighlighter == null || equipmentManager == null)
            return;

        if (index < 0 || index >= width)
            return;

        selectedIndex = index;

        selectedItem = itemGrid.GetItemAt(index, 0);
        InventoryItemEntry selectedEntry = itemGrid.GetEntryAt(index, 0);

        if (selectedItem != null && lastSelectedItemID != selectedItem.GetInstanceID())
        {
            singleHighlighter.Show(true);
            equipmentManager.EquipEntry(selectedEntry);
            HandleHighlight();
        }
        else
        {
            selectedItem = null; // Deselect the item if it's the same as before
            singleHighlighter.Show(false);
            equipmentManager.Unequip();
        }
        HandleHighlight();

        lastSelectedItemID = selectedItem != null ? selectedItem.GetInstanceID() : -1;
        lastIndex = index;
    }

    // Moves the selected-slot highlight to the current hotbar item.
    private void HandleHighlight()
    {
        InventoryItemUI highlightItem = selectedItem;

        if (highlightItem != null)
            {
                // Debug.Log($"HIGHLIGHT: {highlightItem.name}, ID: {highlightItem.GetInstanceID()}, Size: {highlightItem.Width}x{highlightItem.Height}");
                singleHighlighter.Show(true);
                singleHighlighter.SetSize(highlightItem);
                singleHighlighter.SetBehind();
                singleHighlighter.SetParent(itemGrid);
                singleHighlighter.SetPosition(itemGrid, highlightItem);
            }
    }

    // Compatibility wrapper for older code that still hands hotbar a UI item.
    public void SetInventoryItemSlot(InventoryItemUI item, int x, int y)
    {
        if (item == null)
        {
            SetInventoryEntrySlot(null, null, x, y);
            return;
        }

        SetInventoryEntrySlot(item.EnsureEntry(), item, x, y);
    }

    // Stores an item entry in the hotbar and remembers its icon so the UI can still show/move it.
    public void SetInventoryEntrySlot(InventoryItemEntry entry, InventoryItemUI itemView, int x, int y)
    {
        if (hotbarEntries == null || x < 0 || x >= hotbarEntries.GetLength(0) || y < 0 || y >= hotbarEntries.GetLength(1))
            return;

        if (entry == null)
        {
            hotbarEntries[x, y] = null;
            return;
        }

        hotbarEntries[x, y] = entry;

        if (itemView != null)
            hotbarItemViews[entry] = itemView;
    }

    // Debug helper for inspecting hotbar contents in the console.
    public void PrintHotbarItems()
    {
        for (int i = 0; i < hotbarEntries.GetLength(0); i++)
        {
            for (int j = 0; j < hotbarEntries.GetLength(1); j++)
            {
                if (hotbarEntries[i, j] != null)
                {
                    Debug.Log($"Item at ({i}, {j}): {hotbarEntries[i, j].ItemData.name}");
                }
                else
                {
                    Debug.Log($"No item at ({i}, {j})");
                }
            }
        }
    }

    // Rebuilds the visible hotbar row from the hotbar entry array.
    internal void SetHotbar()
    {
        int targetRow = 0; // or whatever row in inventory you want hotbar to go into
        int inventoryWidth = itemGrid.GetGridSizeWidth();
        int hotbarWidth = hotbarEntries.GetLength(0);

        // Clear the entire inventory row first
        for (int i = 0; i < inventoryWidth; i++)
        {
            InventoryItemUI existingItem = itemGrid.GetItemAt(i, targetRow);
            if (existingItem != null) {
                if (existingItem.Height == 1) // dont delete items that are too high to get added to inventory
                {
                    itemGrid.ClearItem(existingItem);
                }
            }
        }

        for (int i = 0; i < hotbarWidth; i++)
        {
            InventoryItemEntry entry = hotbarEntries[i, 0];
            InventoryItemUI item = GetItemView(entry);

            if (item != null)
            {
                int itemWidth = entry.Width;
                int hotbarColumn = i; // Original horizontal position in the hotbar

                if (itemGrid.PositionCheck(hotbarColumn, targetRow, itemWidth, entry.Height))
                {
                    itemGrid.PlaceItem(item, hotbarColumn, targetRow);
                }
                else
                {
                    Debug.LogWarning($"Could not place item '{entry.ItemData.name}' at ({hotbarColumn}, {targetRow}) - LL space blocked.");
                }
            }
        }
        HandleHighlight();
    }

    // Exposes the hotbar model so InventoryController can sync it with the inventory row.
    public InventoryItemEntry[,] GetHotbarEntries()
    {
        return hotbarEntries;
    }

    // Converts a hotbar entry back into its UI icon when the grid needs to place or highlight it.
    public InventoryItemUI GetItemView(InventoryItemEntry entry)
    {
        return entry != null && hotbarItemViews.TryGetValue(entry, out InventoryItemUI item) ? item : null;
    }

    // Used before syncing from hotbar to inventory so empty hotbars do not clear inventory unnecessarily.
    public bool IsHotbarEmpty()
    {
        for (int x = 0; x < hotbarEntries.GetLength(0); x++)
        {
            for (int y = 0; y < hotbarEntries.GetLength(1); y++)
            {
                if (hotbarEntries[x, y] != null)
                    return false;
            }
        }
        return true;
    }
    public int GetSelectedIndex() => selectedIndex;
    // Removes the input subscription so this object does not receive callbacks after destruction.
    private void OnDestroy()
    {
        if (playerInputState != null)
            playerInputState.OnHotbarKeyPressed -= HandleHotbarKeyPress;
    }
}
