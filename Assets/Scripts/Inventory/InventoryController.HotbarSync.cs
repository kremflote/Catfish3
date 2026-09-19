using UnityEngine;

public partial class InventoryController
{
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
        int width = mainItemGrid.inventoryItemSlot.GetLength(0);
        bool[] occupied = new bool[width];

        for (int i = 0; i < width; i++)
            hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);

        for (int i = 0; i < width; i++)
        {
            if (occupied[i])
                continue;

            InventoryItem item = mainItemGrid.inventoryItemSlot[i, hotbarRow];

            if (item != null && item.Height <= 1)
            {
                for (int j = 0; j < item.Width; j++)
                {
                    if (i + j < width)
                        occupied[i + j] = true;
                }

                hotbarController.SetInventoryItemSlot(item, i, hotbarTargetRow);
            }
            else
            {
                hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);
            }
        }

        hotbarController.SetHotbar();
    }

    internal void UpdateInventoryFromHotbar()
    {
        if (hotbarController == null)
        {
            Debug.LogError("HotbarController is not set.");
            return;
        }

        if (hotbarController.IsHotbarEmpty())
            return;

        int hotbarWidth = hotbarController.GetHotbarItems().GetLength(0);

        if (mainItemGrid == null)
        {
            Debug.LogError("Main item grid is not set.");
            return;
        }

        int mainGridWidth = mainItemGrid.inventoryItemSlot.GetLength(0);
        int targetRow = 2;

        for (int i = 0; i < mainGridWidth; i++)
        {
            InventoryItem existingItem = mainItemGrid.inventoryItemSlot[i, targetRow];

            if (existingItem != null && existingItem.GetonGridPositionY() == targetRow)
                mainItemGrid.ClearItem(existingItem);
        }

        for (int i = 0; i < hotbarWidth; i++)
        {
            InventoryItem item = hotbarController.GetHotbarItems()[i, 0];
            if (item == null)
                continue;

            int targetColumn = i;
            if (targetColumn + item.Width <= mainGridWidth)
            {
                mainItemGrid.PlaceItem(item, targetColumn, targetRow);
            }
            else
            {
                Debug.LogWarning($"Item '{item.itemData.name}' doesn't fit at ({targetColumn}, {targetRow}) in main grid.");
            }
        }
    }
}
