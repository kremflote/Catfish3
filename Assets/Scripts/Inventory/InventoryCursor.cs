using System;
using System.Collections.Generic;
using UnityEngine;

// Owns the item currently carried by the pointer, separate from grid placement rules.
public class InventoryCursor : MonoBehaviour
{
    public InventoryItemUI HeldItem { get; private set; }
    public CursorPlacement? LastPlacement { get; private set; }
    public bool HasItem => HeldItem != null;
    public bool HasAnyItem => HeldItem != null || queuedItems.Count > 0;

    private Transform canvasTransform;
    private RectTransform heldRectTransform;
    private readonly List<QueuedCursorItem> queuedItems = new List<QueuedCursorItem>();
    private Vector2 lastPointerLocalPosition;

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
        LayoutQueuedItems();
    }

    // Clears the held item after placement, then promotes the next queued item if one exists.
    public InventoryItemUI Release()
    {
        InventoryItemUI releasedItem = HeldItem;
        HeldItem = null;
        heldRectTransform = null;
        LastPlacement = null;
        PromoteNextQueuedItem();
        return releasedItem;
    }

    // Adds displaced items behind the currently held item so the player can place them one by one.
    public void EnqueueDisplacedItems(List<InventoryItemUI> items, ItemGrid sourceGrid)
    {
        if (items == null || items.Count == 0)
            return;

        foreach (InventoryItemUI item in items)
        {
            if (item == null)
                continue;

            CursorPlacement placement = new CursorPlacement(
                sourceGrid,
                new Vector2Int(item.GetonGridPositionX(), item.GetonGridPositionY()));

            queuedItems.Add(new QueuedCursorItem(item, placement));
            MoveQueuedItemToCanvas(item);
        }

        if (HeldItem == null)
            PromoteNextQueuedItem();

        LayoutQueuedItems();
    }

    // Rotates the held item entry; InventoryItemUI updates the child icon while the layout root stays stable.
    public void RotateHeld(float angle)
    {
        if (HeldItem == null)
            return;

        HeldItem.FlipItemInventory(angle);
    }

    // Restores the held item to its source grid if possible, otherwise asks the caller for a fallback placement.
    public bool Cancel(Func<InventoryItemUI, bool> fallbackPlace)
    {
        if (!HasAnyItem)
            return true;

        List<QueuedCursorItem> carriedItems = GetCarriedItems();
        ClearCursorState();

        for (int i = 0; i < carriedItems.Count; i++)
        {
            QueuedCursorItem carriedItem = carriedItems[i];
            if (TryPlaceCarriedItem(carriedItem, fallbackPlace))
                continue;

            RestoreUnplacedItems(carriedItems, i);
            return false;
        }

        return true;
    }

    // Moves the held icon and queued preview icons with the pointer.
    public void UpdateDrag(Vector2 pointerPosition, Camera eventCamera)
    {
        if (HeldItem == null && queuedItems.Count > 0)
            PromoteNextQueuedItem();

        if (HeldItem == null)
            return;

        EnsureHeldRectTransform();
        if (heldRectTransform == null)
            return;

        RectTransform canvasRect = canvasTransform as RectTransform;
        if (canvasRect == null)
        {
            heldRectTransform.position = pointerPosition;
            LayoutQueuedItems();
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            pointerPosition,
            eventCamera,
            out Vector2 localPoint);

        lastPointerLocalPosition = localPoint;
        heldRectTransform.localPosition = localPoint;
        LayoutQueuedItems();
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
        selectedItemTransform.SetAsLastSibling();
        EnsureHeldRectTransform();
    }

    // Parents queued icons under the canvas; their layout is visual only, not real inventory placement.
    private void MoveQueuedItemToCanvas(InventoryItemUI item)
    {
        if (item == null)
            return;

        Transform targetCanvas = canvasTransform != null
            ? canvasTransform
            : item.transform.GetComponentInParent<Canvas>(true)?.transform;

        if (targetCanvas == null)
            return;

        canvasTransform = targetCanvas;
        item.transform.SetParent(canvasTransform, false);
        item.transform.localScale = Vector3.one;
    }

    // Promotes the first queued item into the actual held item slot.
    private void PromoteNextQueuedItem()
    {
        if (HeldItem != null || queuedItems.Count == 0)
            return;

        QueuedCursorItem nextItem = queuedItems[0];
        queuedItems.RemoveAt(0);

        HeldItem = nextItem.item;
        LastPlacement = nextItem.placement;
        heldRectTransform = null;
        MoveHeldItemToCanvas();
        LayoutQueuedItems();
    }

    // Arranges waiting items in a small 3-column preview next to the cursor.
    private void LayoutQueuedItems()
    {
        for (int i = 0; i < queuedItems.Count; i++)
        {
            InventoryItemUI item = queuedItems[i].item;
            if (item == null)
                continue;

            RectTransform rectTransform = item.GetComponent<RectTransform>();
            if (rectTransform == null)
                continue;

            int column = i % 3;
            int row = i / 3;
            Vector2 offset = new Vector2(
                ItemGrid.tileSizeWidth * (column + 1.5f),
                -ItemGrid.tileSizeHeight * row);

            rectTransform.localPosition = lastPointerLocalPosition + offset;
            rectTransform.SetAsLastSibling();
        }

        if (HeldItem != null)
            HeldItem.transform.SetAsLastSibling();
    }

    // Builds a placement list in the order items should be resolved when inventory closes.
    private List<QueuedCursorItem> GetCarriedItems()
    {
        List<QueuedCursorItem> carriedItems = new List<QueuedCursorItem>();
        if (HeldItem != null)
            carriedItems.Add(new QueuedCursorItem(HeldItem, LastPlacement));

        carriedItems.AddRange(queuedItems);
        return carriedItems;
    }

    // Clears cursor bookkeeping without modifying the item GameObjects themselves.
    private void ClearCursorState()
    {
        HeldItem = null;
        LastPlacement = null;
        heldRectTransform = null;
        queuedItems.Clear();
    }

    // Tries original placement first, then lets the inventory controller auto-insert if needed.
    private bool TryPlaceCarriedItem(QueuedCursorItem carriedItem, Func<InventoryItemUI, bool> fallbackPlace)
    {
        if (carriedItem.item == null)
            return true;

        if (carriedItem.placement.HasValue &&
            carriedItem.placement.Value.itemGrid != null &&
            carriedItem.placement.Value.position.HasValue)
        {
            Vector2Int position = carriedItem.placement.Value.position.Value;
            if (carriedItem.placement.Value.itemGrid.PlaceItem(carriedItem.item, position.x, position.y, out _))
                return true;
        }

        return fallbackPlace != null && fallbackPlace(carriedItem.item);
    }

    // If cancel cannot place every item, keep the failed item and everything after it on the cursor.
    private void RestoreUnplacedItems(List<QueuedCursorItem> carriedItems, int failedIndex)
    {
        if (carriedItems == null || failedIndex < 0 || failedIndex >= carriedItems.Count)
            return;

        QueuedCursorItem failedItem = carriedItems[failedIndex];
        HeldItem = failedItem.item;
        LastPlacement = failedItem.placement;
        heldRectTransform = null;
        MoveHeldItemToCanvas();

        for (int i = failedIndex + 1; i < carriedItems.Count; i++)
        {
            queuedItems.Add(carriedItems[i]);
            MoveQueuedItemToCanvas(carriedItems[i].item);
        }

        LayoutQueuedItems();
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

    private struct QueuedCursorItem
    {
        public InventoryItemUI item;
        public CursorPlacement? placement;

        public QueuedCursorItem(InventoryItemUI item, CursorPlacement? placement)
        {
            this.item = item;
            this.placement = placement;
        }
    }
}
