using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{

    public ItemData itemData;
    public InventoryItemEntry Entry { get; private set; }
    [SerializeField] private Image iconImage;
    [SerializeField] private RectTransform iconTransform;
    [SerializeField] private TextMeshProUGUI quantityLabel;
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
        DisableRootRaycast();
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
        transform.localRotation = Quaternion.identity;

        Image image = GetOrCreateIconImage();
        if (image != null && itemData != null)
            image.sprite = itemData.itemIcon;

        DisableRootRaycast();
        SetSizeDelta();
        RefreshIconVisual();
        RefreshQuantityLabel();
    }

    // Resizes the root to the grid footprint; the child icon handles visual rotation separately.
    private void SetSizeDelta()
    {
        Vector2 size = new Vector2();
        size.x = Width * ItemGrid.tileSizeWidth;
        size.y = Height * ItemGrid.tileSizeHeight;
        GetComponent<RectTransform>().sizeDelta = size;
    }

    // Refreshes visible state after rotation, stacking, or loading changes the entry.
    public void Refresh()
    {
        if (Entry == null)
            return;

        itemData = Entry.ItemData;
        transform.localRotation = Quaternion.identity;
        DisableRootRaycast();
        SetSizeDelta();
        RefreshIconVisual();
        RefreshQuantityLabel();
    }

    // Rotates only the artwork, keeping the layout box unrotated for grid placement.
    private void RefreshIconVisual()
    {
        Image image = GetOrCreateIconImage();
        if (image != null && itemData != null)
            image.sprite = itemData.itemIcon;

        if (iconTransform == null)
            return;

        int visualWidth = itemData != null ? itemData.width : Width;
        int visualHeight = itemData != null ? itemData.height : Height;

        iconTransform.anchorMin = new Vector2(0.5f, 0.5f);
        iconTransform.anchorMax = new Vector2(0.5f, 0.5f);
        iconTransform.pivot = new Vector2(0.5f, 0.5f);
        iconTransform.anchoredPosition = Vector2.zero;
        iconTransform.sizeDelta = new Vector2(
            visualWidth * ItemGrid.tileSizeWidth,
            visualHeight * ItemGrid.tileSizeHeight);
        iconTransform.localRotation = Quaternion.Euler(0, 0, Entry != null ? Entry.Rotation : 0f);
        iconTransform.localScale = Vector3.one;
    }

    // Creates a child image for the item art so the root can stay as the unrotated grid footprint.
    private Image GetOrCreateIconImage()
    {
        if (iconImage != null)
        {
            iconTransform = iconImage.GetComponent<RectTransform>();
            return iconImage;
        }

        Transform existing = transform.Find("Icon");
        if (existing != null)
        {
            iconImage = existing.GetComponent<Image>();
            iconTransform = existing.GetComponent<RectTransform>();
            if (iconImage != null)
                return iconImage;
        }

        Image rootImage = GetComponent<Image>();
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(transform, false);
        iconObject.transform.SetAsFirstSibling();

        iconTransform = iconObject.GetComponent<RectTransform>();
        iconImage = iconObject.GetComponent<Image>();
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = false;

        if (rootImage != null)
        {
            iconImage.sprite = rootImage.sprite;
            iconImage.color = rootImage.color;
            iconImage.material = rootImage.material;
            iconImage.type = rootImage.type;
            iconImage.preserveAspect = rootImage.preserveAspect;

            rootImage.sprite = null;
            rootImage.color = new Color(1f, 1f, 1f, 0f);
            rootImage.raycastTarget = false;
        }

        return iconImage;
    }

    // Item icons should never block the grid underneath; the grid owns pickup/place clicks.
    private void DisableRootRaycast()
    {
        Image rootImage = GetComponent<Image>();
        if (rootImage != null)
            rootImage.raycastTarget = false;
    }

    // Shows quantity only for actual stacks, keeping single items visually clean.
    private void RefreshQuantityLabel()
    {
        TextMeshProUGUI label = GetOrCreateQuantityLabel();
        if (label == null || Entry == null)
            return;

        bool showQuantity = Entry.Quantity > 1;
        label.gameObject.SetActive(showQuantity);
        label.text = showQuantity ? Entry.Quantity.ToString() : string.Empty;
        label.transform.SetAsLastSibling();
    }

    // Creates a small bottom-right count label if the item prefab does not already provide one.
    private TextMeshProUGUI GetOrCreateQuantityLabel()
    {
        if (quantityLabel != null)
            return quantityLabel;

        Transform existing = transform.Find("QuantityLabel");
        if (existing != null)
        {
            quantityLabel = existing.GetComponent<TextMeshProUGUI>();
            if (quantityLabel != null)
                return quantityLabel;
        }

        GameObject labelObject = new GameObject("QuantityLabel", typeof(RectTransform));
        labelObject.transform.SetParent(transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(4f, 2f);
        labelRect.offsetMax = new Vector2(-5f, -2f);

        quantityLabel = labelObject.AddComponent<TextMeshProUGUI>();
        quantityLabel.alignment = TextAlignmentOptions.BottomRight;
        quantityLabel.fontSize = 20f;
        quantityLabel.fontStyle = FontStyles.Bold;
        quantityLabel.color = Color.white;
        quantityLabel.raycastTarget = false;

        Outline outline = labelObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1f, -1f);

        return quantityLabel;
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
        Refresh();
    }
}
