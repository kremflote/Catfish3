using UnityEngine;

// Owns the item currently carried by the pointer, separate from grid placement rules.
public class InventoryCursor : MonoBehaviour
{
    public InventoryItemUI HeldItem { get; private set; }
    public CursorPlacement? LastPlacement { get; private set; }
    public bool HasItem => HeldItem != null;

    private Transform canvasTransform;
    private RectTransform heldRectTransform;

    // Gives the cursor the canvas used for free-floating dragged item icons.
    public void Initialize(Transform canvas)
    {
        canvasTransform = canvas;
    }

    // Starts holding an item and remembers where it came from for cancel/restore behavior.
    public void Hold(InventoryItemUI item, ItemGrid sourceGrid = null, Vector2Int? sourcePosition = null)
    {
        heldRectTransform = null;
        HeldItem = item;
        LastPlacement = sourceGrid != null ? new CursorPlacement(sourceGrid, sourcePosition) : null;
        MoveHeldItemToCanvas();
    }

    // Clears the held item after it has been successfully placed somewhere.
    public InventoryItemUI Release()
    {
        InventoryItemUI releasedItem = HeldItem;
        HeldItem = null;
        heldRectTransform = null;
        LastPlacement = null;
        return releasedItem;
    }

    // Rotates the held item entry; InventoryItemUI updates the child icon while the layout root stays stable.
    public void RotateHeld(float angle)
    {
        if (HeldItem == null)
            return;

        HeldItem.FlipItemInventory(angle);
    }

    // Restores the held item to its source grid if possible, otherwise asks the caller for a fallback placement.
    public bool Cancel(System.Func<InventoryItemUI, bool> fallbackPlace)
    {
        if (HeldItem == null)
            return true;

        if (LastPlacement.HasValue && LastPlacement.Value.itemGrid != null && LastPlacement.Value.position.HasValue)
        {
            Vector2Int position = LastPlacement.Value.position.Value;
            LastPlacement.Value.itemGrid.PlaceItem(HeldItem, position.x, position.y);
            Release();
            return true;
        }

        if (fallbackPlace != null && fallbackPlace(HeldItem))
        {
            Release();
            return true;
        }

        return false;
    }

    // Moves the held icon with the pointer while it is outside a grid.
    public void UpdateDrag(Vector2 pointerPosition, Camera eventCamera)
    {
        if (HeldItem == null)
            return;

        EnsureHeldRectTransform();
        if (heldRectTransform == null)
            return;

        RectTransform canvasRect = canvasTransform as RectTransform;
        if (canvasRect == null)
        {
            heldRectTransform.position = pointerPosition;
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            pointerPosition,
            eventCamera,
            out Vector2 localPoint);

        heldRectTransform.localPosition = localPoint;
    }

    // Parents the held icon under the canvas so it can freely follow the pointer.
    private void MoveHeldItemToCanvas()
    {
        if (HeldItem == null)
            return;

        Transform selectedItemTransform = HeldItem.transform;
        Transform targetCanvas = canvasTransform != null
            ? canvasTransform
            : selectedItemTransform.GetComponentInParent<Canvas>(true)?.transform;

        if (targetCanvas == null)
        {
            Debug.LogError("Canvas reference is missing.", this);
            return;
        }

        canvasTransform = targetCanvas;
        selectedItemTransform.SetParent(canvasTransform, false);
        selectedItemTransform.localScale = Vector3.one;
        EnsureHeldRectTransform();
    }

    // Caches the RectTransform because drag updates run every frame.
    private void EnsureHeldRectTransform()
    {
        if (HeldItem != null && heldRectTransform == null)
            heldRectTransform = HeldItem.GetComponent<RectTransform>();
    }

    public struct CursorPlacement
    {
        public ItemGrid itemGrid;
        public Vector2Int? position;

        public CursorPlacement(ItemGrid itemGrid, Vector2Int? position)
        {
            this.itemGrid = itemGrid;
            this.position = position;
        }
    }
}
