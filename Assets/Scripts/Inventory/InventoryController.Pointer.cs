using UnityEngine;

public partial class InventoryController
{
    // Updates the tile highlight so the player can see what slot or item the cursor is targeting.
    private void HandleHighlight()
    {
        Vector2 position = GetPointerPosition();
        InventoryItemUI heldItem = inventoryCursor.HeldItem;

        if (heldItem != null)
            AdjustMousePosition(ref position);

        Vector2Int positionOnGrid = selectedItemGrid.GetTileGridPosition(position);
        if (!selectedItemGrid.PositionCheck(positionOnGrid.x, positionOnGrid.y, 1, 1))
        {
            inventoryHighlight.Show(false);
            return;
        }

        if (heldItem == null && selectedItemGrid != null)
        {
            highlightItem = selectedItemGrid.GetItemAt(positionOnGrid.x, positionOnGrid.y);

            if (highlightItem != null)
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(highlightItem);
                inventoryHighlight.SetBehind();
                inventoryHighlight.SetParent(selectedItemGrid);
                inventoryHighlight.SetPosition(selectedItemGrid, highlightItem);
            }
            else
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(null);
                inventoryHighlight.SetParent(selectedItemGrid);
                inventoryHighlight.SetPosition(selectedItemGrid, positionOnGrid.x, positionOnGrid.y);
            }
        }
        else if (heldItem != null)
        {
            InventoryPlacementResult results = selectedItemGrid.PlacementCheck(
                positionOnGrid.x,
                positionOnGrid.y,
                heldItem.Width,
                heldItem.Height);

            if (results.outOfBounds)
            {
                inventoryHighlight.Show(false);
            }
            else
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(heldItem);
                inventoryHighlight.SetParent(selectedItemGrid);
                inventoryHighlight.SetPosition(selectedItemGrid, heldItem, positionOnGrid.x, positionOnGrid.y);
            }
        }
    }

    // Converts pointer screen position into grid tile coordinates, with held-item centering applied.
    private Vector2Int GetMouseTileGridPosition()
    {
        Vector2 mousePosition = GetPointerPosition();

        if (inventoryCursor.HasItem)
            AdjustMousePosition(ref mousePosition);

        return GetTileGridPosition(mousePosition);
    }

    // Shifts the pointer so multi-tile items are placed by their top-left tile instead of their visual center.
    private void AdjustMousePosition(ref Vector2 position)
    {
        float canvasScale = GetCanvasScaleFactor();
        InventoryItemUI heldItem = inventoryCursor.HeldItem;
        if (heldItem == null)
            return;

        position.x -= (heldItem.Width - 1) * ItemGrid.tileSizeWidth * canvasScale / 2f;
        position.y += (heldItem.Height - 1) * ItemGrid.tileSizeHeight * canvasScale / 2f;
    }

    // Returns the camera used for UI coordinate conversion; overlay canvases intentionally use null.
    private Camera GetCanvasEventCamera()
    {
        Canvas canvas = canvasTransform != null ? canvasTransform.GetComponent<Canvas>() : null;
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    // Accounts for Canvas scaling so grid math stays correct at different UI scale settings.
    private float GetCanvasScaleFactor()
    {
        Canvas canvas = canvasTransform != null ? canvasTransform.GetComponent<Canvas>() : null;
        return canvas != null ? canvas.scaleFactor : 1f;
    }

    // Treats no selected grid as "off grid" for click and highlight logic.
    private bool IsPointerOffGrid()
    {
        return selectedItemGrid == null;
    }

    // Safely asks the active grid to convert a screen position into a grid tile.
    private Vector2Int GetTileGridPosition(Vector2 position)
    {
        return selectedItemGrid?.GetTileGridPosition(position) ?? Vector2Int.zero;
    }

    // Reads the pointer from PlayerInputState so inventory does not talk directly to Unity's mouse API.
    private Vector2 GetPointerPosition()
    {
        return playerInputState != null ? playerInputState.pointerPosition : Vector2.zero;
    }

}
