using UnityEngine;
using static ItemGrid;

public partial class InventoryController
{
    private void HandleHighlight()
    {
        Vector2 position = GetPointerPosition();
        if (selectedItem != null)
            AdjustMousePosition(ref position);

        Vector2Int positionOnGrid = selectedItemGrid.GetTileGridPosition(position);
        if (!selectedItemGrid.PositionCheck(positionOnGrid.x, positionOnGrid.y, 1, 1))
        {
            inventoryHighlight.Show(false);
            return;
        }

        if (selectedItem == null && selectedItemGrid != null)
        {
            highlightItem = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

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
        else if (selectedItem != null)
        {
            PlacementValidationResult results = selectedItemGrid.PlacementCheck(
                positionOnGrid.x,
                positionOnGrid.y,
                selectedItem.Width,
                selectedItem.Height);

            if (results.outOfBounds)
            {
                inventoryHighlight.Show(false);
            }
            else
            {
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(selectedItem);
                inventoryHighlight.SetParent(selectedItemGrid);
                inventoryHighlight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
            }
        }
    }

    private Vector2Int GetMouseTileGridPosition()
    {
        Vector2 mousePosition = GetPointerPosition();

        if (selectedItem != null)
            AdjustMousePosition(ref mousePosition);

        return GetTileGridPosition(mousePosition);
    }

    private void AdjustMousePosition(ref Vector2 position)
    {
        float canvasScale = GetCanvasScaleFactor();
        position.x -= (selectedItem.Width - 1) * ItemGrid.tileSizeWidth * canvasScale / 2f;
        position.y += (selectedItem.Height - 1) * ItemGrid.tileSizeHeight * canvasScale / 2f;
    }

    private void SetParentToCanvas()
    {
        Transform selectedItemTransform = selectedItem.transform;

        Transform canvas = canvasTransform != null ? canvasTransform : selectedItemTransform.GetComponentInParent<Canvas>(true)?.transform;
        if (canvas == null)
        {
            Debug.LogError("Canvas reference is missing.", this);
            return;
        }

        selectedItemTransform.SetParent(canvas, false);
        selectedItemTransform.localScale = Vector3.one;
    }

    private void UpdateHeldItemIcon()
    {
        if (selectedItem != null)
            rectTransform = selectedItem.GetComponent<RectTransform>();
    }

    private void HandleItemIconDrag()
    {
        if (selectedItem == null)
            return;

        RectTransform canvasRect = canvasTransform as RectTransform;
        if (canvasRect == null)
        {
            rectTransform.position = GetPointerPosition();
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            GetPointerPosition(),
            GetCanvasEventCamera(),
            out Vector2 localPoint);

        rectTransform.localPosition = localPoint;
    }

    private Camera GetCanvasEventCamera()
    {
        Canvas canvas = canvasTransform != null ? canvasTransform.GetComponent<Canvas>() : null;
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera;
    }

    private float GetCanvasScaleFactor()
    {
        Canvas canvas = canvasTransform != null ? canvasTransform.GetComponent<Canvas>() : null;
        return canvas != null ? canvas.scaleFactor : 1f;
    }

    private bool IsPointerOffGrid()
    {
        return selectedItemGrid == null;
    }

    private Vector2Int GetTileGridPosition(Vector2 position)
    {
        return selectedItemGrid?.GetTileGridPosition(position) ?? Vector2Int.zero;
    }

    private Vector2 GetPointerPosition()
    {
        return playerInputState != null ? playerInputState.pointerPosition : Vector2.zero;
    }

    private Transform FindDescendantByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}
