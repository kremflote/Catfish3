using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ItemGrid;

public class InventoryController : MonoBehaviour
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
    private SingleHighlighter inventoryHighlight;
    private MultipleHighlighter inventoryHotbarHighlight;
    private RectTransform rectTransform;
    private InventoryToggleManager inventoryToggleManager;  
    InputManager inputManager;
    HotbarController hotbarController;

    // lagrer hvilken itemgrid og posisjonen siste plasserte item var i 
    public LastPlacement? lastPlacement;

    [SerializeField] private List<ItemData> items;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform canvasTransform;

    void OnEnable()
    {
        inputManager.OnQPressed += HandleCreateRandomItem;
        inputManager.OnTPressed += HandleInsertAll;
        inputManager.OnUPressed += HandleInsertRandom;
        inputManager.OnMouseClick += HandleMouseClick;
    }

    void OnDisable()
    {
        inputManager.OnQPressed -= HandleCreateRandomItem;
        inputManager.OnTPressed -= HandleInsertAll;
        inputManager.OnUPressed -= HandleInsertRandom;
        inputManager.OnMouseClick -= HandleMouseClick;
    }

    private void HandleCreateRandomItem()
    {
        if (selectedItem == null)
        {
            CreateRandomItem();
        }
    }

    private void HandleInsertAll()
    {
        InsertAllUIElements();
    }

    private void HandleInsertRandom()
    {
        InsertRandomItem();
    }

    private void HandleMouseClick(string button)
    {
        if (selectedItemGrid == null || IsPointerOffGrid()) return;

        if (button == "Left Click")
        {
            HandleLeftMouseClick();
        }
        else if (button == "Right Click")
        {
            if (selectedItem != null)
            {
                FlipSelectedItem();
            }
        }
    }

    private void HandleLeftMouseClick()
    {
        if (IsPointerOffGrid())
        {
            Debug.Log("Pointer is not on the grid.");
            return;
        }
        InteractWithItem();
    }
    private void Awake()
    {
        InitializeComponents();
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
        InitializeHotbarHighlight();
        InitializeInventoryToggleManager();

        Transform player = transform.parent;
        Transform canvas = player.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in player hierarchy.");
        }
        Transform managers = player.Find("Managers");
        Transform inputManagerTransform = managers.Find("InputManager");
        inputManager = inputManagerTransform.GetComponent<InputManager>();

        if (inputManager == null)
        {
            Debug.LogError("InputManager not found in Managers hierarchy.");
        }
        Transform expandableBotleft = canvas.Find("expandable_botleft");
        Transform greyGrid = expandableBotleft.Find("GreyGrid");
        ItemGrid itemGrid = greyGrid.GetComponent<ItemGrid>();
        mainItemGrid = itemGrid;
        Transform expandableBotright = canvas.Find("expandable_botright");
        Transform expandableTopleft = canvas.Find("expandable_topleft");
        Transform expandableTopright = canvas.Find("expandable_topright");
        Transform playerScreen = canvas.Find("player_screen");
        Transform worldScreen = canvas.Find("world_screen");
        Transform itemDescriptionScreen = canvas.Find("item_description_screen");
        Transform hotbar = canvas.Find("hotbar");

        Transform hotbarManager = managers.Find("HotbarManager");
        hotbarController = hotbarManager.GetComponent<HotbarController>();

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
    private void InitializeHotbarHighlight()
    {
        Transform player = transform.parent;

        Transform mainCamera = player.Find("MainCamera");
        MultipleHighlighter inventoryHotbarHighlight2 = mainCamera.GetComponent<MultipleHighlighter>();
        inventoryHotbarHighlight = inventoryHotbarHighlight2;

        Transform canvas = transform.parent.Find("Canvas");

        Transform expandable_botleft = canvas.Find("expandable_botleft");

        Transform greyGrid = expandable_botleft.Find("GreyGrid");

        ItemGrid eblItemGrid = greyGrid.GetComponent<ItemGrid>();

        int numberOfHighlights = eblItemGrid.GetGridSizeWidth();


        inventoryHotbarHighlight.GenerateHighlighters(numberOfHighlights, canvas);
        inventoryHotbarHighlight.SetSize(1, 1);
        inventoryHotbarHighlight.SetBehind();
        inventoryHotbarHighlight.SetParent(eblItemGrid);
        // start posisjonen settes her, så blir de neste highlighteren som genereres plassert til høyre
        inventoryHotbarHighlight.SetPosition(eblItemGrid, 0, 2);
    }
    private void AddUIHUD(Transform uiElement)
    {

        inventoryToggleManager.AddHUD(uiElement.gameObject);
        InitializeGridInteract(uiElement);
        InitializeGridToHUD(uiElement);
    }
    private void InitializeGridToHUD(Transform uiElement)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("InitializeGrid was called with a null uiElement.");
            return;
        }

        Transform greyGrid = uiElement.Find("GreyGrid");
        if (greyGrid == null)
        {
            Debug.LogWarning($"GreyGrid not found as a child of {uiElement.name}.");
            return;
        }

        GridInteract interact = greyGrid.GetComponent<GridInteract>();
        if (interact == null)
        {
            Debug.LogWarning($"GridInteract component missing on GreyGrid under {uiElement.name}.");
            return;
        }
        inventoryToggleManager.AddHUD(greyGrid.gameObject);
    }
    private void InitializeInventoryToggleManager()
    {
        Transform playerGO = transform.parent;
        Transform managers = playerGO.Find("Managers");
        Transform inventoryToggleGO = managers.Find("InventoryToggleManager");
        inventoryToggleManager = inventoryToggleGO.GetComponent<InventoryToggleManager>();
    }
    private void AddUIInventory(Transform uiElement)
    {

        inventoryToggleManager.AddPlayerScreen(uiElement.gameObject);
        InitializeGridInteract(uiElement);
        InitializeGridToInventory(uiElement);
    }
    private void InitializeGridToInventory(Transform uiElement)
    {
        if (uiElement == null)
        {
            Debug.LogWarning("InitializeGrid was called with a null uiElement.");
            return;
        }

        Transform greyGrid = uiElement.Find("GreyGrid");
        if (greyGrid == null)
        {
            return;
        }

        GridInteract interact = greyGrid.GetComponent<GridInteract>();
        if (interact == null)
        {
            Debug.LogWarning($"GridInteract component missing on GreyGrid under {uiElement.name}.");
            return;
        }
        inventoryToggleManager.AddPlayerScreen(greyGrid.gameObject);

    }
    private void InitializeGridInteract(Transform uiElement)
    {
        Transform greyGrid = uiElement?.Find("GreyGrid");
        if (greyGrid == null)
        {
            return;
        }

        gridInteract = greyGrid.GetComponent<GridInteract>();
        if (gridInteract == null)
        {
            Debug.LogError("GridInteract component is missing on GreyGrid.");
        }
    }
    private void InsertAllUIElements()
    {
        foreach (Transform screen in playerScreens)
        {
            AddUIInventory(screen);
        }
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
        if (selectedItemGrid == null)
        {
            selectedItemGrid = lastPlacement.Value.itemGrid;
            if (selectedItemGrid == null)
            {
                Debug.LogError("SelectedItemGrid is not set.");
                return false;
            }
        }
        Vector2Int? positionOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);

        if (positionOnGrid == null) {
            FlipSelectedItem(itemToInsert);
            Debug.Log($"FLIPPED: {itemToInsert.name}, ID: {itemToInsert.GetInstanceID()}, Size: {itemToInsert.Width}x{itemToInsert.Height}");


            positionOnGrid = selectedItemGrid.FindSpaceForObject(itemToInsert);
            if (positionOnGrid == null)
            {
                Debug.Log("No space found for the item after flipping.");
                return false;
            }
        }
        selectedItemGrid.PlaceItem(itemToInsert, positionOnGrid.Value.x, positionOnGrid.Value.y);
        return true;
    }
    private void HandleHighlight()
    {
            Vector2 position = Input.mousePosition;
            // Adjust the mouse position based on the selected item's size if holding something
            if (selectedItem != null)
            {
                AdjustMousePosition(ref position);
            }
            // Get the tile grid position based on the mouse position
            Vector2Int positionOnGrid = selectedItemGrid.GetTileGridPosition(position);
            // Check if the position is within the bounds of the grid
            if (selectedItemGrid.PositionCheck(positionOnGrid.x, positionOnGrid.y, 1, 1) == false) 
            {
                inventoryHighlight.Show(false);
                return;
            }


        if (selectedItem == null && selectedItemGrid != null)
        {

            highlightItem = selectedItemGrid.GetItem(positionOnGrid.x, positionOnGrid.y);

            if (highlightItem != null)
            {
                // Debug.Log($"HIGHLIGHT: {highlightItem.name}, ID: {highlightItem.GetInstanceID()}, Size: {highlightItem.Width}x{highlightItem.Height}");

                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(highlightItem);
                inventoryHighlight.SetBehind();
                inventoryHighlight.SetParent(selectedItemGrid);
                inventoryHighlight.SetPosition(selectedItemGrid, highlightItem);
            }
            else
            {
                // Highlight empty square based on mouse position
                inventoryHighlight.Show(true);
                inventoryHighlight.SetSize(null);
                inventoryHighlight.SetParent(selectedItemGrid);
                inventoryHighlight.SetPosition(selectedItemGrid, positionOnGrid.x, positionOnGrid.y);
            }
        }

        else if (selectedItem != null)
        {
                PlacementValidationResult results = selectedItemGrid.PlacementCheck(
                    positionOnGrid.x,
                    positionOnGrid.y,
                    selectedItem.Width,
                    selectedItem.Height
                    );


                if (results.outOfBounds)
                { inventoryHighlight.Show(false); }

                else
                {
                    inventoryHighlight.Show(true);
                    inventoryHighlight.SetSize(selectedItem);
                    inventoryHighlight.SetParent(selectedItemGrid);
                    inventoryHighlight.SetPosition(selectedItemGrid, selectedItem, positionOnGrid.x, positionOnGrid.y);
                }
        }
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

        InventoryItem inventoryItem = Instantiate(itemPrefab).GetComponent<InventoryItem>();
        if (inventoryItem == null)
        {
            Debug.LogError("Failed to instantiate InventoryItem.");
            return;
        }

        selectedItem = inventoryItem;
        rectTransform = inventoryItem.GetComponent<RectTransform>();
        rectTransform.SetParent(canvasTransform);

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
        item.transform.Rotate(0, 0, RotationAngle);
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
    private Vector2Int GetMouseTileGridPosition()
    {
        Vector2 mousePosition = Input.mousePosition;

        if (selectedItem != null)
        {
            AdjustMousePosition(ref mousePosition);
        }
        Vector2Int tileGridPosition = GetTileGridPosition(mousePosition);

        return tileGridPosition;
    }
    private void AdjustMousePosition(ref Vector2 position)
    {
        position.x -= (selectedItem.Width - 1) * ItemGrid.tileSizeWidth / 2;
        position.y += (selectedItem.Height - 1) * ItemGrid.tileSizeHeight / 2;
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
    private void SetParentToCanvas()
    {
        Transform selectedItemTransform = selectedItem.transform;

        Transform canvas = selectedItemTransform.parent.parent.parent;

        selectedItemTransform.SetParent(canvas);

    }
    public bool CancelPickupItem()
    {
        if (selectedItem != null)
        {
            if (lastPlacement.HasValue && lastPlacement.Value.position.HasValue)
            {
                Vector2Int position = lastPlacement.Value.position.Value; // Access the value of the nullable Vector2Int
                Debug.Log($"Last placement: {lastPlacement.Value.itemGrid}, {position}");
                lastPlacement.Value.itemGrid.PlaceItem(
                    selectedItem,
                    position.x,
                    position.y
                );
            }
            else
            {
                Debug.Log("No last placement found.");

                bool success = InsertItem(selectedItem);
                Debug.Log($"InsertItem success: {success}");
                if (!success)
                {
                    return false;
                }
            }
        }
        selectedItem = null;
        return true;
    }
    public void SetLastPlacementItemGrid(ItemGrid itemGrid)
    {
        if (lastPlacement.HasValue)
        {
            // Create a copy of the struct, modify it, and assign it back
            var placement = lastPlacement.Value;
            placement.SetItemGrid(itemGrid);
            lastPlacement = placement;
        }
        else
        {
            // If lastPlacement is null, create a new instance
            lastPlacement = new LastPlacement(itemGrid);
        }
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

        public void SetItemGrid (ItemGrid itemGrid)
        {
            this.itemGrid = itemGrid;
            position = Vector2Int.zero;
        }
    }
    private void UpdateHeldItemIcon()
    {
        if (selectedItem != null)
        {
            rectTransform = selectedItem.GetComponent<RectTransform>();
        }
    }
    private void HandleItemIconDrag()
    {
        if (selectedItem != null)
        {
            rectTransform.position = Input.mousePosition;
        }
    }
    private bool IsPointerOffGrid()
    {
        return selectedItemGrid == null;
    }
    private Vector2Int GetTileGridPosition(Vector2 position)
    {
        return selectedItemGrid?.GetTileGridPosition(position) ?? Vector2Int.zero;
    }
    internal void UpdateHotbar()
    {
        if (mainItemGrid == null)
        {
            Debug.LogError("mainItemGrid is not set.");
            return;
        }
        if (inventoryHotbarHighlight == null)
        {
            Debug.LogError("InventoryHotbarHighlight is not set.");
            return;
        }

        int hotbarRow = 2; // The row in the main grid that maps to the hotbar
        int hotbarTargetRow = 0; // The row in the hotbar to place items
        int width = mainItemGrid.inventoryItemSlot.GetLength(0);
        bool[] occupied = new bool[width]; // Track which slots are already occupied by wide items

        for (int i = 0; i < width; i++)
        {
            hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);
        }

        for (int i = 0; i < width; i++)
        {

            // Skip if already covered by a wide item
            if (occupied[i]) continue;

            InventoryItem item = mainItemGrid.inventoryItemSlot[i, hotbarRow];

            if (item != null && item.Height <= 1)
            {
                int itemWidth = item.Width;

                // Mark all slots that this item covers
                for (int j = 0; j < itemWidth; j++)
                {
                    if (i + j < width)
                        occupied[i + j] = true;
                }

                // Place item in hotbar at the same column as its origin
                hotbarController.SetInventoryItemSlot(item, i, hotbarTargetRow);
                Debug.Log($"Item added to hotbar: {item.itemData.name}");
            }
            else
            {
                hotbarController.SetInventoryItemSlot(null, i, hotbarTargetRow);
            }
        }
        hotbarController.SetHotbar();
    }
    internal void UpdateInventoryFromHotbar()
    {
        if (hotbarController.IsHotbarEmpty() == true)
        {
            return;
        }
        int hotbarWidth = hotbarController.GetHotbarItems().GetLength(0);

        if (mainItemGrid == null)
        {
            Debug.LogError("Main item grid is not set.");
            return;
        }
        int mainGridWidth = mainItemGrid.inventoryItemSlot.GetLength(0);
        int targetRow = 2; // Row in the main grid where hotbar items go

        // Clear target row
        for (int i = 0; i < mainGridWidth; i++)
        {
            InventoryItem existingItem = mainItemGrid.inventoryItemSlot[i, targetRow];

            if (existingItem != null)
            {
                // Only clear if this cell is the origin of the item (top-left corner)
                if (existingItem.GetonGridPositionY() == targetRow)
                {
                    mainItemGrid.ClearSlot(i, targetRow);
                }
                // Else: do NOT clear the cell — it’s still part of a taller item
            }
        }

        // Step 2: Place hotbar items back into main grid
        for (int i = 0; i < hotbarWidth; i++)
        {
            InventoryItem item = hotbarController.GetHotbarItems()[i, 0];

            if (item != null)
            {
                int itemWidth = item.Width;
                int targetColumn = i;

                // Check if the item fits at its original hotbar position
                if (targetColumn + itemWidth <= mainGridWidth)
                {
                    mainItemGrid.PlaceItem(item, targetColumn, targetRow);
                }
                else
                {
                    Debug.LogWarning($"Item '{item.itemData.name}' doesn't fit at ({targetColumn}, {targetRow}) in main grid.");
                }
            }
        }
    }
}