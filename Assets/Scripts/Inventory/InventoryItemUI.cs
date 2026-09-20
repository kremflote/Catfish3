using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{

    public ItemData itemData;
    public InventoryItemEntry Entry { get; private set; }
    private int onGridPositionX;
    private int onGridPositionY;

    public float zRotation
    {
        get { return Entry != null ? Entry.Rotation : 0f; }
    }

    public int Width => Entry != null ? Entry.Width : itemData != null ? itemData.width : 1;
    public int Height => Entry != null ? Entry.Height : itemData != null ? itemData.height : 1;

    // Unity calls this when the icon is enabled; it backfills an entry for prefab-created items.
    private void Start()
    {
        EnsureEntry();
    }

    // Creates a new runtime entry from an item definition and updates the icon art/size.
    internal void Set(ItemData itemData)
    {
        this.itemData = itemData;
        Entry = new InventoryItemEntry(itemData);
        SyncVisualFromEntry();
    }

    // Attaches this icon to an existing entry, used when loading saved inventory.
    internal void Set(InventoryItemEntry entry)
    {
        Entry = entry;
        SyncVisualFromEntry();
    }

    // Ensures old prefab-style items still get a runtime entry before model code uses them.
    public InventoryItemEntry EnsureEntry()
    {
        if (Entry == null && itemData != null)
        {
            Entry = new InventoryItemEntry(itemData);
            Entry.GridPosition = new Vector2Int(onGridPositionX, onGridPositionY);
        }

        return Entry;
    }

    // Applies entry data to the visible UI icon so loaded/rotated items look correct.
    private void SyncVisualFromEntry()
    {
        if (Entry == null)
            return;

        itemData = Entry.ItemData;
        transform.localRotation = Quaternion.Euler(0, 0, Entry.Rotation);

        Image image = GetComponent<Image>();
        if (image != null && itemData != null)
            image.sprite = itemData.itemIcon;

        SetSizeDelta();
    }

    // Resizes the UI RectTransform to match how many grid tiles this item covers.
    private void SetSizeDelta()
    {
        Vector2 size = new Vector2();
        size.x = Width * ItemGrid.tileSizeWidth;
        size.y = Height * ItemGrid.tileSizeHeight;
        GetComponent<RectTransform>().sizeDelta = size;
    }

    public int GetonGridPositionX ()
    {  return Entry != null ? Entry.GridPosition.x : onGridPositionX; }
    public int GetonGridPositionY ()
    { return Entry != null ? Entry.GridPosition.y : onGridPositionY; }

    // Updates the entry's X tile while preserving fallback fields for older prefab data.
    public void SetonGridPositionX(int x)
    {
        onGridPositionX = x;

        if (Entry != null)
            Entry.GridPosition = new Vector2Int(x, Entry.GridPosition.y);
    }

    // Updates the entry's Y tile while preserving fallback fields for older prefab data.
    public void SetonGridPositionY(int y)
    {
        onGridPositionY = y;

        if (Entry != null)
            Entry.GridPosition = new Vector2Int(Entry.GridPosition.x, y);
    }

    // Rotates the runtime entry; ItemGrid/InventoryController handle the visual rotation.
    internal void FlipItemInventory(float zRotation)
    {
        EnsureEntry();

        if (Entry == null)
            return;

        Entry.Rotate(zRotation);
    }
}
