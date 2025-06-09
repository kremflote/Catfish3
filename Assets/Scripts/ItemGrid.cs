using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemGrid : MonoBehaviour
{   //This class handles icon behavour on the itemgrid gameobject it is placed on

    // 2D array that keeps track of what item (if any) is in each grid slot; null means the slot is empty
    public InventoryItem[,] inventoryItemSlot;
    // used for sizing and positioning in the UI
    RectTransform rectTransform;
    // mouse's position relative to grid
    Vector2 positionOnTheGrid = new Vector2();
    // Stores the calculated grid coordinates (tile X and Y) based on mouse position
    Vector2Int tileGridPosition = new Vector2Int();
    // the amount of squares in the itemGrid per axiom. aka this is a 3 by 6 grid
    [SerializeField] int gridSizeWidth = 6;
    [SerializeField] int gridSizeHeight = 3;
    // the base from which to generate items
    [SerializeField] GameObject inventoryItemPrefab;
    // the size of each tile in pixels
    public const float tileSizeWidth = 70;
    public const float tileSizeHeight = 70;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Init(gridSizeWidth, gridSizeHeight);
    }

    public int GetGridSizeWidth()
    {
        return gridSizeWidth;
    }

    private void Init(int width, int heigth)
    {
        inventoryItemSlot = new InventoryItem[width, heigth];
        Vector2 size = new Vector2(width * tileSizeWidth, heigth * tileSizeHeight);
        rectTransform.sizeDelta = size;
    }
    internal InventoryItem GetItem(int x, int y)
    {
        return inventoryItemSlot[x, y];
    }
    public InventoryItem PickUpItem(int x, int y)
    {
        InventoryItem toReturn = inventoryItemSlot[x, y];

        if (toReturn == null) { return null; }

        // iterates through grid and clears the slot based on items width and length
        GridClear(toReturn);
        return toReturn;
    }
    private void GridClear(InventoryItem item)
    {
        for (int ix = 0; ix < item.Width; ix++)
        {
            for (int iy = 0; iy < item.Height; iy++)
            {
                inventoryItemSlot[item.GetonGridPositionX() + ix, item.GetonGridPositionY() + iy] = null;
            }
        }
    }
    public bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, out List<InventoryItem> overlappingItems)
    {
        PlacementValidationResult checkResult = PlacementCheck(posX, posY, inventoryItem.Width, inventoryItem.Height);
        overlappingItems = checkResult.overlappingItems;

        if (checkResult.outOfBounds)
        {
            Debug.Log($"Item is out of bounds by {checkResult.overflowX} on X and {checkResult.overflowY} on Y.");
            return false;
        }
        else if (checkResult.collisionWithObject)
        {
            Debug.Log($"Item collision with another item");
            return false;
        }

        PlaceItem(inventoryItem, posX, posY);

        return true;
    }
    public void PlaceItem(InventoryItem inventoryItem, int posX, int posY)
    {
        // setter parameter inventoryItem parent til denne item gridden
        RectTransform rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.SetParent(this.rectTransform);

        // Updates the item’s own data to remember where it is on the grid (top-left corner position).
        // Debug.Log($"Placing item at grid ({posX}, {posY}) with size {inventoryItem.Width}x{inventoryItem.Height}.");
        inventoryItem.SetonGridPositionX(posX);
        inventoryItem.SetonGridPositionY(posY);


        //Loops through every slot that the item should cover, x and y representing 2d grid
        //Mark the grid cells as occupied by this item
        for (int x = 0; x < inventoryItem.Width; x++)
        {
            for (int y = 0; y < inventoryItem.Height; y++)
            {
                inventoryItemSlot[posX + x, posY + y] = inventoryItem;
                // Debug.Log($"Placing item at grid ({posX + x}, {posY + y}) with size {inventoryItem.Width}x{inventoryItem.Height}.");
            }
        }

        // Position the item visually in the UI
        // Vector2 keeps track of 2d grid positions
        Vector2 position = CalculatePositionOnGrid(inventoryItem, posX, posY);

        // finally use vector2 to set position of object
        rectTransform.localPosition = position;
        // Debug.Log($"Placing item at grid ({posX}, {posY}) with size {inventoryItem.Width}x{inventoryItem.Height}, UI position: {position}");
    }
    public Vector2 CalculatePositionOnGrid(InventoryItem inventoryItem, int posX, int posY)
    {
        Vector2 position = new Vector2();
        position.x = posX * tileSizeWidth + tileSizeWidth * inventoryItem.Width / 2f;
        position.y = -(posY * tileSizeHeight + tileSizeHeight * inventoryItem.Height / 2f);
        return position;
    }
    public Vector2 CalculatePositionOnGrid(int posX, int posY)
    {
        Vector2 position = new Vector2();
        position.x = posX * tileSizeWidth + tileSizeWidth / 2f;
        position.y = -(posY * tileSizeHeight + tileSizeHeight / 2f);
        return position;
    }
    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        positionOnTheGrid.x = mousePosition.x - rectTransform.position.x;
        positionOnTheGrid.y = rectTransform.position.y -  mousePosition.y;

        tileGridPosition.x = (int)(positionOnTheGrid.x / tileSizeWidth);
        tileGridPosition.y = (int)(positionOnTheGrid.y / tileSizeHeight);
        return tileGridPosition;
    }
    public Vector2Int? FindSpaceForObject(InventoryItem itemToInsert)
    {
        int height = gridSizeHeight - itemToInsert.Height + 1;
        int width = gridSizeWidth - itemToInsert.Width + 1;

        Debug.Log($"Finding space for item of size {itemToInsert.Width}x{itemToInsert.Height} in a grid of size {gridSizeWidth}x{gridSizeHeight}.");

        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                if (CheckAvailableSpace(x, y, itemToInsert.Width, itemToInsert.Height) == true)
                {
                    Debug.Log($"Found space for item at {x}, {y}");
                    return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }
    public bool CheckAvailableSpace(int posX, int posY, int width, int height)
    {
        // bound check, if a coordinate is negative it is outside grid. we dont  want to acces outside grid
        // that creates error
        if (PositionCheck(posX, posY, width, height) == false)
        {
            Debug.LogWarning($"Item does not fit within grid bounds at position ({posX}, {posY}) with size {width}x{height}.");
            return false;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (inventoryItemSlot[posX + x, posY + y] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public bool PositionCheck(int posX, int posY)
    {
        // if a coordinate is negative it is outside grid
        bool inBounds = !(posX < 0 || posY < 0 || posX >= gridSizeWidth || posY >= gridSizeHeight);
    
        return inBounds;
    }
    public bool PositionCheck(int posX, int posY, int width, int height)
    {
        // if a coordinate is negative it is outside grid
        bool inBounds = !(posX < 0 || posY < 0 || posX + width > gridSizeWidth || posY + height > gridSizeHeight);

        return inBounds;
    }
    bool OccupiedCheck(int posX, int posY) 
    {
        if (!PositionCheck(posX, posY))
        {
            return false;
        }

        return inventoryItemSlot[posX, posY] != null;
    }
    public PlacementValidationResult PlacementCheck(int posX, int posY, int width, int height)
    {
        PlacementValidationResult result = new PlacementValidationResult();

        // Initial assumption: no overflow
        result.overflowX = 0;
        result.overflowY = 0;

        // Calculate bottom-right corner of item
        int bottomRightX = posX + width - 1;
        int bottomRightY = posY + height - 1;

        // Check bounds for the whole area
        bool fullyInBounds = IsFullyInBounds(posX, posY, width, height);
        result.outOfBounds = !fullyInBounds;

        // If out of bounds, calculate overflow
        if (result.outOfBounds)
        {
            CalculateOverflow(posX, posY, width, height, ref result);
        }

        // Collision check (only if in bounds)
        result.collisionWithObject = false;
        if (fullyInBounds)
        {
            List<InventoryItem> overlapItems = null;
            result.collisionWithObject = HasCollision(posX, posY, width, height, out overlapItems);
            result.overlappingItems = overlapItems;


        }

        return result;
    }
    bool IsFullyInBounds(int posX, int posY, int width, int height)
    {
        // Top-left
        if (!PositionCheck(posX, posY))
        {
            return false;
        }

        // Bottom-right
        int bottomRightX = posX + width - 1;
        int bottomRightY = posY + height - 1;

        if (!PositionCheck(bottomRightX, bottomRightY))
        {
            return false;
        }

        return true;
    }
    bool HasCollision(int posX, int posY, int width, int height, out List<InventoryItem> overlapItems)
    {
        overlapItems = new List<InventoryItem>();
        HashSet<InventoryItem> uniqueItems = new HashSet<InventoryItem>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int checkX = posX + x;
                int checkY = posY + y;

                if (OccupiedCheck(checkX, checkY))
                {
                    var item = inventoryItemSlot[checkX, checkY];
                    if (item != null && !uniqueItems.Contains(item))
                    {
                        uniqueItems.Add(item);
                        overlapItems.Add(item);
                    }
                }
            }
        }

        return overlapItems.Count > 0;
    }
    void CalculateOverflow(int posX, int posY, int width, int height, ref PlacementValidationResult result)
    {
        int bottomRightX = posX + width - 1;
        int bottomRightY = posY + height - 1;

        // X overflow
        if (posX < 0)
        {
            result.overflowX = posX; // negative = how far left it's off
        }
        else if (bottomRightX >= gridSizeWidth)
        {
            result.overflowX = bottomRightX - (gridSizeWidth - 1); // how far right it's off
        }

        // Y overflow
        if (posY < 0)
        {
            result.overflowY = posY; // negative = how far above it's off
        }
        else if (bottomRightY >= gridSizeHeight)
        {
            result.overflowY = bottomRightY - (gridSizeHeight - 1); // how far below it's off
        }
    }
    internal void ClearSlot(int i, int targetRow)
    {
        // Safety check to avoid index out of bounds
        if (i < 0 || i >= inventoryItemSlot.GetLength(0) ||
            targetRow < 0 || targetRow >= inventoryItemSlot.GetLength(1))
        {

            return;
        }

        InventoryItem item = inventoryItemSlot[i, targetRow];

        if (item != null)
        {
            // Optional: remove any visual/UI or data associations
            // e.g., Destroy(item.gameObject) if you're using GameObjects

            inventoryItemSlot[i, targetRow] = null;
        }
    }
    public struct PlacementValidationResult
    {
        public bool outOfBounds;
        public bool collisionWithObject;
        public int overflowX; // how much it overflows on X axis (0 = no overflow)
        public int overflowY; // how much it overflows on Y axis (0 = no overflow)
        public List<InventoryItem> overlappingItems;
    }
    public struct PlacementOutcome
    {
        public bool success;
        public InventoryItem overlappingItem;

        public PlacementOutcome(bool success, InventoryItem overlappingItem)
        {
            this.success = success;
            this.overlappingItem = overlappingItem;
        }
    }
}
