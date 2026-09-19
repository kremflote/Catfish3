using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemGrid;
using StarterAssets;

public partial class InventoryController : MonoBehaviour
{
    [HideInInspector]
    public ItemGrid selectedItemGrid;

    [SerializeField]  public ItemGrid mainItemGrid { get; private set; }
    public GridInteract gridInteract { get; private set; }

    private const float RotationAngle = 90f;

    private List<Transform> playerScreens;
    private List<Transform> playerHUDs;
    private InventoryItem selectedItem;
    private InventoryItem highlightItem;
    private ItemGrid lastHoveredGrid;
    private SingleHighlighter inventoryHighlight;
    private RectTransform rectTransform;
    [SerializeField] private PlayerInputState playerInputState;
    [SerializeField] private InventoryToggleManager inventoryToggleManager;
    [SerializeField] private HotbarController hotbarController;
    [SerializeField] private MultipleHighlighter inventoryHotbarHighlight;
    [SerializeField] private Transform expandableBotleft;
    [SerializeField] private Transform expandableBotright;
    [SerializeField] private Transform expandableTopleft;
    [SerializeField] private Transform expandableTopright;
    [SerializeField] private Transform playerScreen;
    [SerializeField] private Transform worldScreen;
    [SerializeField] private Transform itemDescriptionScreen;
    [SerializeField] private Transform hotbar;

    // Remembers where the held item came from so cancel can put it back.
    public LastPlacement? lastPlacement;

    [SerializeField] private List<ItemData> items;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform canvasTransform;

    private void Awake()
    {
        InitializeComponents();
        if (inventoryHighlight != null)
            inventoryHighlight.Show(false);
    }
    private void Update()
    {
        HandleItemIconDrag();
        if (inventoryToggleManager.GetIsOpen() == true && !IsPointerOffGrid())
        {
            HandleHighlight();
        }
    }
    public bool SelectedItemIsNull()
    {
        return selectedItem == null;
    }
    private void InitializeComponents()
    {
        playerScreens = new List<Transform>();
        playerHUDs = new List<Transform>();
        inventoryHighlight = GetComponent<SingleHighlighter>();
        InitializeInventoryToggleManager();

        Transform player = transform.root;
        Transform canvas = canvasTransform != null ? canvasTransform : player.GetComponentInChildren<Canvas>(true)?.transform;
        if (canvas == null)
        {
            Debug.LogError("Canvas reference is missing.", this);
            return;
        }
        canvasTransform = canvas;

        if (playerInputState == null)
            playerInputState = player.GetComponentInChildren<PlayerInputState>(true);

        if (playerInputState == null)
            Debug.LogError("PlayerInputState reference is missing.", this);

        ResolveInventoryScreens(canvas);
        InitializeHotbarHighlight();

        Transform greyGrid = FindDescendantByName(expandableBotleft, "GreyGrid");
        ItemGrid itemGrid = greyGrid != null ? greyGrid.GetComponent<ItemGrid>() : null;
        mainItemGrid = mainItemGrid != null ? mainItemGrid : itemGrid;

        if (hotbarController == null)
            hotbarController = player.GetComponentInChildren<HotbarController>(true);

        playerScreens.Add(expandableBotleft);
        playerScreens.Add(expandableBotright);
        playerScreens.Add(expandableTopleft);
        playerScreens.Add(expandableTopright);
        playerScreens.Add(playerScreen);
        playerScreens.Add(worldScreen);
        playerScreens.Add(itemDescriptionScreen);
        playerHUDs.Add(hotbar);

        AddUIHUD(hotbar);
        AddUIInventory(expandableBotleft);
        AddUIInventory(playerScreen);
    }

    private void InitializeInventoryToggleManager()
    {
        if (inventoryToggleManager == null)
            inventoryToggleManager = transform.root.GetComponentInChildren<InventoryToggleManager>(true);

        if (inventoryToggleManager == null)
            Debug.LogError("InventoryToggleManager reference is missing.", this);
    }
    private void InsertRandomItem()
    {
        CreateRandomItem();
        InventoryItem itemToInsert = selectedItem;
        selectedItem = null;
        InsertItem(itemToInsert);
    }
    public bool InsertItem(InventoryItem itemToInsert)
    {
        if (itemToInsert == null)
            return false;

        ItemGrid targetGrid = GetPlacementTargetGrid();
        if (targetGrid == null)
        {
            Debug.LogError("No inventory grid is available for item insertion.", this);
            return false;
        }

        Vector2Int? positionOnGrid = targetGrid.FindSpaceForObject(itemToInsert);

        if (positionOnGrid == null) {
            FlipSelectedItem(itemToInsert);

            positionOnGrid = targetGrid.FindSpaceForObject(itemToInsert);
            if (positionOnGrid == null)
            {
                Debug.Log("No space found for the item after flipping.");
                return false;
            }
        }
        targetGrid.PlaceItem(itemToInsert, positionOnGrid.Value.x, positionOnGrid.Value.y);
        return true;
    }
    public void SetItemGrid(ItemGrid itemGrid)
    {
        this.selectedItemGrid = itemGrid;
    }
    private void CreateRandomItem()
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("No items available to create.");
            return;
        }

        InventoryItem inventoryItem = Instantiate(itemPrefab, canvasTransform, false).GetComponent<InventoryItem>();
        if (inventoryItem == null)
        {
            Debug.LogError("Failed to instantiate InventoryItem.");
            return;
        }

        selectedItem = inventoryItem;
        rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        int selectedItemID = UnityEngine.Random.Range(0, items.Count);
        inventoryItem.Set(items[selectedItemID]);
    }
    private void FlipSelectedItem(InventoryItem itemToFlip = null)
    {
        //flips given item, or defeaults to selecteditem
        InventoryItem item = itemToFlip ?? selectedItem;

        if (item == null)
        {
            Debug.Log("No item selected to flip.");
            return;
        }

        item.FlipItemInventory(RotationAngle);
        item.transform.localRotation = Quaternion.Euler(0, 0, item.zRotation);
    }
    private void InteractWithItem()
    {
        if(inventoryToggleManager.GetIsOpen() == true) {
            Vector2Int tileGridPosition = GetMouseTileGridPosition();

            if (selectedItem == null)
                {

                PickUpItem(tileGridPosition);
                }
            else
                {
            TryPlaceSelectedItem(tileGridPosition);
                }
            }
    }
    private void TryPlaceSelectedItem(Vector2Int tileGridPosition)
    {
        if (selectedItemGrid == null)
        {
            Debug.LogError("SelectedItemGrid is not set.");
            return;
        }

        bool success = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, out var overlappingItems);

        if (success)
        {
            selectedItem = null;
            lastPlacement = null;
        }
        else if (overlappingItems != null && overlappingItems.Count > 0)
        {
            if (overlappingItems.Count > 1)
            {
                Debug.LogWarning("Multiple overlapping items detected; can't auto-swap.");
                return;  // Don’t do anything if more than 1 overlaps
            }

            var overlapItem = overlappingItems[0];

            // Pick up the overlapping item first:
            selectedItemGrid.PickUpItem(overlapItem.GetonGridPositionX(), overlapItem.GetonGridPositionY());

            // Now, try again to place the item:
            bool retrySuccess = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, out var dummy);

            if (retrySuccess)
            {
                selectedItem = overlapItem; // Now we're holding the replaced item
                lastPlacement = new LastPlacement(selectedItemGrid, new Vector2Int(overlapItem.GetonGridPositionX(), overlapItem.GetonGridPositionY()));
                SetParentToCanvas();
                UpdateHeldItemIcon();
            }
            else
            {
                Debug.LogWarning("Failed to place item even after swapping.");
            }
        }
        else
        {
            Debug.Log("Failed to place item and no overlapping item to swap.");
        }
    }
    private void PickUpItem(Vector2Int tileGridPosition)
    {
        selectedItem = selectedItemGrid.PickUpItem(tileGridPosition.x, tileGridPosition.y);

        if (selectedItem == null)
        {

            return;
        }
        SetParentToCanvas();
        UpdateHeldItemIcon();
        lastPlacement = new LastPlacement(selectedItemGrid, tileGridPosition);
    }
    public bool CancelPickupItem()
    {
        if (selectedItem != null)
        {
            if (lastPlacement.HasValue && lastPlacement.Value.itemGrid != null && lastPlacement.Value.position.HasValue)
            {
                Vector2Int position = lastPlacement.Value.position.Value;
                lastPlacement.Value.itemGrid.PlaceItem(
                    selectedItem,
                    position.x,
                    position.y
                );
            }
            else
            {
                bool success = InsertItem(selectedItem);
                if (!success)
                {
                    return false;
                }
            }
        }
        selectedItem = null;
        lastPlacement = null;
        return true;
    }
    public void SetHoveredGrid(ItemGrid itemGrid)
    {
        lastHoveredGrid = itemGrid;
    }

    private ItemGrid GetPlacementTargetGrid()
    {
        if (selectedItemGrid != null)
            return selectedItemGrid;

        if (lastHoveredGrid != null)
            return lastHoveredGrid;

        if (lastPlacement.HasValue && lastPlacement.Value.itemGrid != null)
            return lastPlacement.Value.itemGrid;

        return mainItemGrid;
    }
    public struct LastPlacement
    {
        public ItemGrid itemGrid;
        public Vector2Int? position;

        public LastPlacement(ItemGrid itemGrid, Vector2Int position)
        {
            this.itemGrid = itemGrid;
            this.position = position;
        }

        public LastPlacement(ItemGrid itemGrid)
        {
            this.itemGrid = itemGrid;
            this.position = null;
        }
    }
}
