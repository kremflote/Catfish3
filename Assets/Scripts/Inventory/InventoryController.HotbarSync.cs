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

        int hotbarRow = 2;
        int hotbarTargetRow = 0;
        int width = mainItemGrid.GetGridSizeWidth();
        bool[] occupied = new bool[width];

        for (int i = 0; i < width; i++)
            hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);

        for (int i = 0; i < width; i++)
        {
            if (occupied[i])
                continue;

            InventoryItemEntry entry = mainItemGrid.GetEntryAt(i, hotbarRow);
            InventoryItemUI item = mainItemGrid.GetItemAt(i, hotbarRow);

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
        int targetRow = 2;

        for (int i = 0; i < mainGridWidth; i++)
        {
            InventoryItemUI existingItem = mainItemGrid.GetItemAt(i, targetRow);

            if (existingItem != null && existingItem.GetonGridPositionY() == targetRow)
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
                mainItemGrid.PlaceItem(item, targetColumn, targetRow);
            }
            else
            {
                Debug.LogWarning($"Item '{entry.ItemData.name}' doesn't fit at ({targetColumn}, {targetRow}) in main grid.");
            }
        }
    }
}
