using UnityEngine;

public partial class InventoryController
{
    // Copies the configured inventory row into the hotbar model before inventory closes.
    internal void UpdateHotbar()
    {
        if (mainItemGrid == null)
        {
            Debug.LogError("mainItemGrid is not set.");
            return;
        }

        if (inventoryHotbarHighlight == null)
        {
            Debug.LogError("InventoryHotbarHighlight is not set.");
            return;
        }

        if (hotbarController == null)
        {
            Debug.LogError("HotbarController is not set.");
            return;
        }

        int width = mainItemGrid.GetGridSizeWidth();
        bool[] occupied = new bool[width];

        for (int i = 0; i < width; i++)
            hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);

        for (int i = 0; i < width; i++)
        {
            if (occupied[i])
                continue;

            InventoryItemEntry entry = mainItemGrid.GetEntryAt(i, hotbarInventoryRow);
            InventoryItemUI item = mainItemGrid.GetItemAt(i, hotbarInventoryRow);

            if (entry != null && entry.Height <= 1)
            {
                for (int j = 0; j < entry.Width; j++)
                {
                    if (i + j < width)
                        occupied[i + j] = true;
                }

                hotbarController.SetInventoryEntrySlot(entry, item, i, hotbarTargetRow);
            }
            else
            {
                hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);
            }
        }

        hotbarController.SetHotbar();
    }

    // Restores the hotbar model back into the inventory row before inventory opens.
    internal void UpdateInventoryFromHotbar()
    {
        if (hotbarController == null)
        {
            Debug.LogError("HotbarController is not set.");
            return;
        }

        if (hotbarController.IsHotbarEmpty())
            return;

        int hotbarWidth = hotbarController.GetHotbarEntries().GetLength(0);

        if (mainItemGrid == null)
        {
            Debug.LogError("Main item grid is not set.");
            return;
        }

        int mainGridWidth = mainItemGrid.GetGridSizeWidth();
        for (int i = 0; i < mainGridWidth; i++)
        {
            InventoryItemUI existingItem = mainItemGrid.GetItemAt(i, hotbarInventoryRow);

            if (existingItem != null && existingItem.GetonGridPositionY() == hotbarInventoryRow)
                mainItemGrid.ClearItem(existingItem);
        }

        for (int i = 0; i < hotbarWidth; i++)
        {
            InventoryItemEntry entry = hotbarController.GetHotbarEntries()[i, 0];
            InventoryItemUI item = hotbarController.GetItemView(entry);
            if (entry == null || item == null)
                continue;

            int targetColumn = i;
            if (targetColumn + entry.Width <= mainGridWidth)
            {
                mainItemGrid.PlaceItem(item, targetColumn, hotbarInventoryRow);
            }
            else
            {
                Debug.LogWarning($"Item '{entry.ItemData.name}' doesn't fit at ({targetColumn}, {hotbarInventoryRow}) in main grid.");
            }
        }
    }
}
