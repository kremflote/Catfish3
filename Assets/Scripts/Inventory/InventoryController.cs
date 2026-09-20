using System;
using System.Collections.Generic;
using UnityEngine;
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
    private InventoryItemUI selectedItem;
    private InventoryItemUI highlightItem;
    private ItemGrid lastHoveredGrid;
    private InventoryHighlighter inventoryHighlight;
    private RectTransform rectTransform;
    private const string InventorySaveKey = "catfish.inventory.main";
    [SerializeField] private PlayerInputState playerInputState;
    [SerializeField] private InventoryToggleManager inventoryToggleManager;
    [SerializeField] private HotbarController hotbarController;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private InventoryHighlighter inventoryHotbarHighlight;
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

    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform canvasTransform;

    // Unity calls this once when the component is created; we use it to find UI/player references early.
    private void Awake()
    {
        InitializeComponents();
        if (inventoryHighlight != null)
            inventoryHighlight.Show(false);
    }

    // Runs every frame so a held item follows the pointer and hover highlights stay current.
    private void Update()
    {
        HandleItemIconDrag();
        if (inventoryToggleManager.GetIsOpen() == true && !IsPointerOffGrid())
        {
            HandleHighlight();
        }
    }

    // Used by input/state code to know whether closing inventory must first return a held item.
    public bool SelectedItemIsNull()
    {
        return selectedItem == null;
    }

    // Collects references from the player prefab so the inventory can work when spawned over the network.
    private void InitializeComponents()
    {
        playerScreens = new List<Transform>();
        playerHUDs = new List<Transform>();
        inventoryHighlight = GetComponent<InventoryHighlighter>();
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

    // Finds the toggle manager that actually opens/closes inventory UI.
    private void InitializeInventoryToggleManager()
    {
        if (inventoryToggleManager == null)
            inventoryToggleManager = transform.root.GetComponentInChildren<InventoryToggleManager>(true);

        if (inventoryToggleManager == null)
            Debug.LogError("InventoryToggleManager reference is missing.", this);
    }

    // Debug/prototype helper: creates a random item, then tries to place it into the current target grid.
    private void InsertRandomItem()
    {
        CreateRandomItem();
        InventoryItemUI itemToInsert = selectedItem;
        selectedItem = null;
        InsertItem(itemToInsert);
    }

    // Inserts an item into the first available space, trying a rotated version if the original shape does not fit.
    public bool InsertItem(InventoryItemUI itemToInsert)
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

    // Called by GridInteract when the pointer enters/leaves a grid, so clicks target the correct grid.
    public void SetItemGrid(ItemGrid itemGrid)
    {
        this.selectedItemGrid = itemGrid;
    }

    // Instantiates a UI item icon and gives it a random ItemData from the database.
    private void CreateRandomItem()
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("Item database is missing.");
            return;
        }

        ItemData itemData = itemDatabase.GetRandomItemData();
        if (itemData == null)
        {
            Debug.LogWarning("No items available to create.");
            return;
        }

        InventoryItemUI itemUI = Instantiate(itemPrefab, canvasTransform, false).GetComponent<InventoryItemUI>();
        if (itemUI == null)
        {
            Debug.LogError("Failed to instantiate InventoryItemUI.");
            return;
        }

        selectedItem = itemUI;
        rectTransform = itemUI.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        itemUI.Set(itemData);
    }

    // Rotates the selected item entry and its icon so grid size and visuals stay in sync.
    private void FlipSelectedItem(InventoryItemUI itemToFlip = null)
    {
        // Flips given item, or defaults to selected item.
        InventoryItemUI item = itemToFlip ?? selectedItem;

        if (item == null)
        {
            Debug.Log("No item selected to flip.");
            return;
        }

        item.FlipItemInventory(RotationAngle);
        item.transform.localRotation = Quaternion.Euler(0, 0, item.zRotation);
    }

    // Handles the core click behavior: pick up from a grid, or place the currently held item.
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

    // Attempts placement and supports swapping with one overlapping item for Diablo-style inventory behavior.
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
                return;  // Do not auto-swap if more than one item would be displaced.
            }

            var overlapItem = overlappingItems[0];

            // Pick up the overlapping item first.
            selectedItemGrid.PickUpItem(overlapItem.GetonGridPositionX(), overlapItem.GetonGridPositionY());

            // Now try again to place the item.
            bool retrySuccess = selectedItemGrid.PlaceItem(selectedItem, tileGridPosition.x, tileGridPosition.y, out var dummy);

            if (retrySuccess)
            {
                selectedItem = overlapItem;
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

    // Removes an item from the grid model and turns its icon into the item currently carried by the cursor.
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

    // Puts a held item back before closing inventory; this prevents invisible items from getting stranded.
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

    // Converts the current main inventory grid into JSON using stable item IDs rather than Unity object references.
    public string CreateSaveJson()
    {
        if (mainItemGrid == null)
            return JsonUtility.ToJson(new InventorySaveData());

        return JsonUtility.ToJson(new InventorySaveData(mainItemGrid.CreateSnapshot()), true);
    }

    // Rebuilds inventory contents from JSON by resolving item IDs through the ItemDatabase.
    public bool LoadSaveJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        if (mainItemGrid == null || itemDatabase == null || itemPrefab == null || canvasTransform == null)
        {
            Debug.LogError("Inventory load references are incomplete.", this);
            return false;
        }

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
        if (saveData == null)
            return false;

        mainItemGrid.ClearAllItems(true);

        if (saveData.items == null)
            return true;

        foreach (InventoryItemSnapshot snapshot in saveData.items)
        {
            if (!itemDatabase.TryGetItemData(snapshot.itemId, out ItemData itemData))
            {
                Debug.LogWarning($"Could not load inventory item with id '{snapshot.itemId}'.", this);
                continue;
            }

            InventoryItemUI itemUI = Instantiate(itemPrefab, canvasTransform, false).GetComponent<InventoryItemUI>();
            if (itemUI == null)
                continue;

            InventoryItemEntry entry = new InventoryItemEntry(itemData, snapshot);
            itemUI.Set(entry);

            bool placed = mainItemGrid.PlaceItem(itemUI, snapshot.x, snapshot.y, out _);
            if (!placed)
                Destroy(itemUI.gameObject);
        }

        return true;
    }

    // Prototype persistence helper; useful before we add a real save-file system.
    public void SaveInventoryToPlayerPrefs()
    {
        PlayerPrefs.SetString(InventorySaveKey, CreateSaveJson());
        PlayerPrefs.Save();
    }

    // Prototype persistence helper; loads the last inventory JSON stored in PlayerPrefs.
    public bool LoadInventoryFromPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey(InventorySaveKey))
            return false;

        return LoadSaveJson(PlayerPrefs.GetString(InventorySaveKey));
    }

    // Remembers the last grid under the pointer so item insertion has a sensible target.
    public void SetHoveredGrid(ItemGrid itemGrid)
    {
        lastHoveredGrid = itemGrid;
    }

    // Chooses where new/returned items should go when there is no direct clicked grid.
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
