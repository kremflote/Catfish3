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

    private int BaseWidth => ItemData != null ? ItemData.width : 1;
    private int BaseHeight => ItemData != null ? ItemData.height : 1;
    private bool IsRotated => Mathf.RoundToInt(NormalizedRotation / 90f) % 2 != 0;
    private float NormalizedRotation => Mathf.Repeat(Rotation, 360f);

    // Creates a fresh item instance from an item definition.
    public InventoryItemEntry(ItemData itemData)
    {
        Id = System.Guid.NewGuid().ToString();
        ItemData = itemData;
        GridPosition = Vector2Int.zero;
        Rotation = 0f;
    }

    // Recreates an existing item instance from save data.
    public InventoryItemEntry(ItemData itemData, InventoryItemSnapshot snapshot)
    {
        Id = string.IsNullOrWhiteSpace(snapshot.id) ? System.Guid.NewGuid().ToString() : snapshot.id;
        ItemData = itemData;
        GridPosition = new Vector2Int(snapshot.x, snapshot.y);
        Rotation = Mathf.Repeat(snapshot.rotation, 360f);
    }

    // Rotates this instance; its grid footprint is computed from ItemData plus this rotation.
    public void Rotate(float angle)
    {
        Rotation = Mathf.Repeat(Rotation + angle, 360f);
    }
}
