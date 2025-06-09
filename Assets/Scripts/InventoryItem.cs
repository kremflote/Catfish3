using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{

    public ItemData itemData;
    private int width;
    private int height;
    private int onGridPositionX;
    private int onGridPositionY;
    public float zRotation;

    public int Width
    {
        get { return width; }
        set
        {
            width = value;
        }
    }

    public int Height
    {
        get { return height; }
        set
        {
            height = value;
        }
    }
    private void Start()
    {
        if (Width == 0 && Height == 0)
        {
            Width = itemData.width;
            Height = itemData.height;
        }
        zRotation = 0f;
    }

    internal void Set(ItemData itemData)
    {
        this.itemData = itemData;
        this.Width = itemData.width;
        this.Height = itemData.height;

        GetComponent<Image>().sprite = itemData.itemIcon;
        SetSizeDelta();
    }

    private void SetSizeDelta()
    {
        Vector2 size = new Vector2();
        size.x = Width * ItemGrid.tileSizeWidth;
        size.y = Height * ItemGrid.tileSizeHeight;
        GetComponent<RectTransform>().sizeDelta = size;
    }

    public int GetonGridPositionX ()
    {  return onGridPositionX; }
    public int GetonGridPositionY ()
    { return onGridPositionY; }

    public void SetonGridPositionX(int x)
    { onGridPositionX = x; }

    public void SetonGridPositionY(int y)
    { onGridPositionY = y; }

    internal void FlipItemInventory(float zRotation)
    {
        this.zRotation += zRotation;
        int oldWidth = Width;
        int oldHeight = Height;
        Width = oldHeight;
        Height = oldWidth;

    }
}
