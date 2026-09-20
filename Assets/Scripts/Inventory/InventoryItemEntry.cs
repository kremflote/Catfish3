using UnityEngine;

// Runtime state for one item instance in an inventory.
public sealed class InventoryItemEntry
{
    public string Id { get; }
    public ItemData ItemData { get; }
    public int Width => IsRotated ? BaseHeight : BaseWidth;
    public int Height => IsRotated ? BaseWidth : BaseHeight;
    public Vector2Int GridPosition { get; set; }
    public float Rotation { get; private set; }
    public int Quantity { get; private set; }
    public int MaxStack => ItemData != null ? Mathf.Max(1, ItemData.maxStack) : 1;
    public bool IsStackable => MaxStack > 1;
    public bool IsEmpty => Quantity <= 0;
    public int AvailableStackSpace => Mathf.Max(0, MaxStack - Quantity);

    private int BaseWidth => ItemData != null ? ItemData.width : 1;
    private int BaseHeight => ItemData != null ? ItemData.height : 1;
    private bool IsRotated => Mathf.RoundToInt(NormalizedRotation / 90f) % 2 != 0;
    private float NormalizedRotation => Mathf.Repeat(Rotation, 360f);

    // Creates a fresh item instance from an item definition.
    public InventoryItemEntry(ItemData itemData, int quantity = 1)
    {
        Id = System.Guid.NewGuid().ToString();
        ItemData = itemData;
        GridPosition = Vector2Int.zero;
        Rotation = 0f;
        Quantity = ClampQuantity(Mathf.Max(1, quantity));
    }

    // Recreates an existing item instance from save data.
    public InventoryItemEntry(ItemData itemData, InventoryItemSnapshot snapshot)
    {
        Id = string.IsNullOrWhiteSpace(snapshot.id) ? System.Guid.NewGuid().ToString() : snapshot.id;
        ItemData = itemData;
        GridPosition = new Vector2Int(snapshot.x, snapshot.y);
        Rotation = Mathf.Repeat(snapshot.rotation, 360f);
        Quantity = ClampQuantity(snapshot.quantity > 0 ? snapshot.quantity : 1);
    }

    // Rotates this instance; its grid footprint is computed from ItemData plus this rotation.
    public void Rotate(float angle)
    {
        Rotation = Mathf.Repeat(Rotation + angle, 360f);
    }

    // Updates stack count while respecting this item definition's stack limit.
    public void SetQuantity(int quantity)
    {
        Quantity = ClampQuantity(quantity);
    }

    // Returns true when two runtime entries represent the same stackable item definition.
    public bool CanStackWith(InventoryItemEntry other)
    {
        if (other == null || !IsStackable)
            return false;

        string thisId = ItemData != null ? ItemData.ItemId : string.Empty;
        string otherId = other.ItemData != null ? other.ItemData.ItemId : string.Empty;
        return !string.IsNullOrWhiteSpace(thisId) && thisId == otherId;
    }

    // Adds as much quantity as possible and returns how many items were accepted.
    public int AddQuantity(int amount)
    {
        if (amount <= 0)
            return 0;

        int accepted = Mathf.Min(amount, AvailableStackSpace);
        Quantity += accepted;
        return accepted;
    }

    // Removes up to the requested amount and returns how many items were removed.
    public int RemoveQuantity(int amount)
    {
        if (amount <= 0)
            return 0;

        int removed = Mathf.Min(amount, Quantity);
        Quantity -= removed;
        return removed;
    }

    // Moves quantity from this entry into another stack and returns how many items moved.
    public int TransferQuantityTo(InventoryItemEntry target)
    {
        if (!CanStackWith(target))
            return 0;

        int moved = target.AddQuantity(Quantity);
        RemoveQuantity(moved);
        return moved;
    }

    // Keeps quantities valid even when loading older saves or designer-authored data.
    private int ClampQuantity(int quantity)
    {
        return Mathf.Clamp(quantity, 0, MaxStack);
    }
}
