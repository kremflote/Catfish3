using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ItemGrid;
using StarterAssets;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private PlayerInputState playerInputState;
    [SerializeField] private ItemGrid itemGrid;
    [SerializeField] private SingleHighlighter singleHighlighter;
    [SerializeField] private EquipmentManager equipmentManager;
    private int width;
    private InventoryItem[,] hotbarItems;
    private int selectedIndex = 0;
    private InventoryItem selectedItem;
    private int lastIndex;
    private int lastSelectedItemID;


    private void Awake()
    {
        Transform player = transform.root;

        if (equipmentManager == null)
            equipmentManager = GetComponent<EquipmentManager>();

        if (singleHighlighter == null)
            singleHighlighter = player.GetComponentInChildren<SingleHighlighter>(true);

        if (itemGrid == null)
        {
            Transform canvas = player.GetComponentInChildren<Canvas>(true)?.transform;
            Transform hotbar = canvas != null ? canvas.Find("hotbar") : null;
            Transform greyGrid = hotbar != null ? hotbar.Find("GreyGrid") : null;
            itemGrid = greyGrid != null ? greyGrid.GetComponent<ItemGrid>() : null;
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
    public void InitializeHotbarItemSlots()
    {
        hotbarItems = new InventoryItem[width, 1];
    }
    private void HandleHotbarKeyPress(int index)
    {
        if (singleHighlighter == null || equipmentManager == null)
            return;

        selectedIndex = index;

        selectedItem = itemGrid.GetItem(index, 0);

        if (selectedItem != null && lastSelectedItemID != selectedItem.GetInstanceID())
        {
            singleHighlighter.Show(true);
            equipmentManager.EquipItem(selectedItem);
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
    private void HandleHighlight()
    {
        InventoryItem highlightItem = selectedItem;

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
    public void SetInventoryItemSlot(InventoryItem item, int x, int y)
    {
        if (item == null)
        {
            hotbarItems[x, y] = null;
            return;
        }
        hotbarItems[x, y] = item;
    }
    public void PrintHotbarItems()
    {
        for (int i = 0; i < hotbarItems.GetLength(0); i++)
        {
            for (int j = 0; j < hotbarItems.GetLength(1); j++)
            {
                if (hotbarItems[i, j] != null)
                {
                    Debug.Log($"Item at ({i}, {j}): {hotbarItems[i, j].itemData.name}");
                }
                else
                {
                    Debug.Log($"No item at ({i}, {j})");
                }
            }
        }
    }
    internal void SetHotbar()
    {
        int targetRow = 0; // or whatever row in inventory you want hotbar to go into
        int inventoryWidth = itemGrid.inventoryItemSlot.GetLength(0);
        int hotbarWidth = hotbarItems.GetLength(0);

        // Clear the entire inventory row first
        for (int i = 0; i < inventoryWidth; i++)
        {
            if (itemGrid.inventoryItemSlot[i, targetRow] != null) {
                if (itemGrid.inventoryItemSlot[i, targetRow].Height == 1) // dont delete items that are too high to get added to inventory
                {
                    itemGrid.ClearSlot(i, targetRow);
                }
            }
        }

        for (int i = 0; i < hotbarWidth; i++)
        {
            InventoryItem item = hotbarItems[i, 0];

            if (item != null)
            {
                int itemWidth = item.Width;
                int hotbarColumn = i; // Original horizontal position in the hotbar

                if (itemGrid.PositionCheck(hotbarColumn, targetRow, itemWidth, item.Height))
                {
                    itemGrid.PlaceItem(item, hotbarColumn, targetRow);
                    Debug.Log($"Placed item '{item.itemData.name}' at ({hotbarColumn}, {targetRow})");
                }
                else
                {
                    Debug.LogWarning($"Could not place item '{item.itemData.name}' at ({hotbarColumn}, {targetRow}) - LL space blocked.");
                }
            }
        }
        HandleHighlight();
    }
    public InventoryItem[,] GetHotbarItems()
    {
        return hotbarItems;
    }
    public bool IsHotbarEmpty()
    {
        for (int x = 0; x < hotbarItems.GetLength(0); x++)
        {
            for (int y = 0; y < hotbarItems.GetLength(1); y++)
            {
                if (hotbarItems[x, y] != null)
                    return false;
            }
        }
        return true;
    }
    public int GetSelectedIndex() => selectedIndex;
    private void OnDestroy()
    {
        if (playerInputState != null)
            playerInputState.OnHotbarKeyPressed -= HandleHotbarKeyPress;
    }

}
