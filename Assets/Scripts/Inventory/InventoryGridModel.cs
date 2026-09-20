using System.Collections.Generic;
using UnityEngine;

// Data owner for one grid's item occupancy. UI scripts should ask this model what is in each tile.
public sealed class InventoryGridModel
{
    public int Width { get; }
    public int Height { get; }
    public InventoryItemEntry[,] Slots { get; }

    // Creates an empty grid model with fixed dimensions.
    public InventoryGridModel(int width, int height)
    {
        Width = width;
        Height = height;
        Slots = new InventoryItemEntry[width, height];
    }

    // Returns the item entry occupying a tile, or null when the tile is empty/outside the grid.
    public InventoryItemEntry GetEntry(int x, int y)
    {
        return PositionCheck(x, y) ? Slots[x, y] : null;
    }

    // Removes whichever entry occupies the tile and returns it to the caller.
    public InventoryItemEntry PickUpEntry(int x, int y)
    {
        InventoryItemEntry entry = GetEntry(x, y);
        ClearEntry(entry);
        return entry;
    }

    // Marks every tile covered by this entry as occupied by that entry.
    public void PlaceEntry(InventoryItemEntry entry, int posX, int posY)
    {
        if (entry == null || !PositionCheck(posX, posY, entry.Width, entry.Height))
            return;

        entry.GridPosition = new Vector2Int(posX, posY);

        for (int x = 0; x < entry.Width; x++)
        {
            for (int y = 0; y < entry.Height; y++)
            {
                Slots[posX + x, posY + y] = entry;
            }
        }
    }

    // Clears every tile currently occupied by this entry.
    public void ClearEntry(InventoryItemEntry entry)
    {
        if (entry == null)
            return;

        for (int x = 0; x < entry.Width; x++)
        {
            for (int y = 0; y < entry.Height; y++)
            {
                int slotX = entry.GridPosition.x + x;
                int slotY = entry.GridPosition.y + y;

                if (PositionCheck(slotX, slotY) && Slots[slotX, slotY] == entry)
                    Slots[slotX, slotY] = null;
            }
        }
    }

    // Scans the grid from top-left to bottom-right for a free area matching the entry size.
    public Vector2Int? FindSpaceForObject(InventoryItemEntry entry)
    {
        if (entry == null || entry.Width > Width || entry.Height > Height)
            return null;

        int maxX = Width - entry.Width;
        int maxY = Height - entry.Height;

        for (int y = 0; y <= maxY; y++)
        {
            for (int x = 0; x <= maxX; x++)
            {
                if (CheckAvailableSpace(x, y, entry.Width, entry.Height))
                    return new Vector2Int(x, y);
            }
        }

        return null;
    }

    // Checks both bounds and collisions for a rectangular footprint.
    public bool CheckAvailableSpace(int posX, int posY, int width, int height)
    {
        if (!PositionCheck(posX, posY, width, height))
            return false;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (Slots[posX + x, posY + y] != null)
                    return false;
            }
        }

        return true;
    }

    // Returns each unique entry once, even though multi-tile items occupy multiple slots.
    public List<InventoryItemEntry> GetEntries()
    {
        List<InventoryItemEntry> entries = new List<InventoryItemEntry>();
        HashSet<InventoryItemEntry> uniqueEntries = new HashSet<InventoryItemEntry>();

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                InventoryItemEntry entry = Slots[x, y];
                if (entry != null && uniqueEntries.Add(entry))
                    entries.Add(entry);
            }
        }

        return entries;
    }

    // Converts runtime entries into serializable save records.
    public List<InventoryItemSnapshot> CreateSnapshot()
    {
        List<InventoryItemSnapshot> snapshot = new List<InventoryItemSnapshot>();

        foreach (InventoryItemEntry entry in GetEntries())
        {
            snapshot.Add(new InventoryItemSnapshot(entry));
        }

        return snapshot;
    }

    // Empties the grid without knowing anything about UI icons.
    public void ClearAll()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Slots[x, y] = null;
            }
        }
    }

    public bool PositionCheck(int posX, int posY)
    {
        return !(posX < 0 || posY < 0 || posX >= Width || posY >= Height);
    }

    // Returns whether a full rectangular footprint is inside the grid.
    public bool PositionCheck(int posX, int posY, int width, int height)
    {
        return !(posX < 0 || posY < 0 || posX + width > Width || posY + height > Height);
    }

    // Produces details for UI feedback: out of bounds, overflow direction, and collisions.
    public InventoryPlacementResult PlacementCheck(int posX, int posY, int width, int height)
    {
        InventoryPlacementResult result = new InventoryPlacementResult();
        result.overflowX = 0;
        result.overflowY = 0;

        bool fullyInBounds = PositionCheck(posX, posY, width, height);
        result.outOfBounds = !fullyInBounds;

        if (result.outOfBounds)
            CalculateOverflow(posX, posY, width, height, ref result);

        result.collisionWithObject = false;
        if (fullyInBounds)
        {
            result.collisionWithObject = HasCollision(posX, posY, width, height, out List<InventoryItemEntry> overlappingEntries);
            result.overlappingEntries = overlappingEntries;
        }

        return result;
    }

    // Finds unique entries hit by a potential placement footprint.
    private bool HasCollision(int posX, int posY, int width, int height, out List<InventoryItemEntry> overlappingEntries)
    {
        overlappingEntries = new List<InventoryItemEntry>();
        HashSet<InventoryItemEntry> uniqueEntries = new HashSet<InventoryItemEntry>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                InventoryItemEntry entry = GetEntry(posX + x, posY + y);
                if (entry != null && uniqueEntries.Add(entry))
                    overlappingEntries.Add(entry);
            }
        }

        return overlappingEntries.Count > 0;
    }

    // Measures how far a placement hangs outside the grid, useful for hiding invalid highlights.
    private void CalculateOverflow(int posX, int posY, int width, int height, ref InventoryPlacementResult result)
    {
        int bottomRightX = posX + width - 1;
        int bottomRightY = posY + height - 1;

        if (posX < 0)
            result.overflowX = posX;
        else if (bottomRightX >= Width)
            result.overflowX = bottomRightX - (Width - 1);

        if (posY < 0)
            result.overflowY = posY;
        else if (bottomRightY >= Height)
            result.overflowY = bottomRightY - (Height - 1);
    }
}

[System.Serializable]
public struct InventoryPlacementResult
{
    public bool outOfBounds;
    public bool collisionWithObject;
    public int overflowX;
    public int overflowY;
    public List<InventoryItemEntry> overlappingEntries;
}

[System.Serializable]
public struct InventoryItemSnapshot
{
    public string id;
    public string itemId;
    public int x;
    public int y;
    public float rotation;
    public int quantity;

    public InventoryItemSnapshot(InventoryItemEntry entry)
    {
        id = entry.Id;
        itemId = entry.ItemData != null ? entry.ItemData.ItemId : string.Empty;
        x = entry.GridPosition.x;
        y = entry.GridPosition.y;
        rotation = entry.Rotation;
        quantity = entry.Quantity;
    }
}

[System.Serializable]
public class InventorySaveData
{
    public List<InventoryItemSnapshot> items = new List<InventoryItemSnapshot>();

    public InventorySaveData()
    {
    }

    // Wraps snapshot items because Unity JsonUtility needs a serializable root object.
    public InventorySaveData(List<InventoryItemSnapshot> items)
    {
        this.items = items ?? new List<InventoryItemSnapshot>();
    }
}
