using UnityEngine;
using StarterAssets;

public partial class InventoryController
{
    // Subscribes this inventory controller to the owning player's input events.
    private void OnEnable()
    {
        if (playerInputState == null)
            playerInputState = transform.root.GetComponentInChildren<StarterAssets.PlayerInputState>(true);

        if (playerInputState == null)
            return;

        playerInputState.OnQPressed += HandleCreateRandomItem;
        playerInputState.OnTPressed += HandleInsertAll;
        playerInputState.OnUPressed += HandleInsertRandom;
        playerInputState.OnMouseClick += HandleMouseClick;
    }

    // Unsubscribes from input events so destroyed/disabled inventories do not keep receiving callbacks.
    private void OnDisable()
    {
        if (playerInputState == null)
            return;

        playerInputState.OnQPressed -= HandleCreateRandomItem;
        playerInputState.OnTPressed -= HandleInsertAll;
        playerInputState.OnUPressed -= HandleInsertRandom;
        playerInputState.OnMouseClick -= HandleMouseClick;
    }

    // Debug input handler for creating a held random item without placing it yet.
    private void HandleCreateRandomItem()
    {
        if (inventoryCursor == null || !inventoryCursor.HasItem)
            CreateRandomItem();
    }

    // Debug input handler for registering inventory screens during UI prototyping.
    private void HandleInsertAll()
    {
        InsertAllUIElements();
    }

    // Debug input handler for creating and auto-inserting one random item.
    private void HandleInsertRandom()
    {
        InsertRandomItem();
    }

    // Routes mouse buttons to inventory actions while keeping PlayerInputState independent of inventory details.
    private void HandleMouseClick(InventoryMouseButton button, InventoryClickModifiers modifiers)
    {
        if (button == InventoryMouseButton.Right && inventoryCursor.HasItem)
        {
            FlipSelectedItem();
            return;
        }

        if (selectedItemGrid == null || IsPointerOffGrid())
            return;

        if (button == InventoryMouseButton.Left)
            HandleLeftMouseClick(modifiers);
    }

    // Left click is the inventory interaction button: pickup when empty-handed, place when holding an item.
    private void HandleLeftMouseClick(InventoryClickModifiers modifiers)
    {
        if (IsPointerOffGrid())
        {
            Debug.Log("Pointer is not on the grid.");
            return;
        }

        if (modifiers.ctrl)
        {
            InteractWithSingleStackItem();
            return;
        }

        if (modifiers.alt && !inventoryCursor.HasItem)
        {
            SplitStackFromGrid();
            return;
        }

        if (modifiers.shift)
        {
            ConsolidateMatchingStacks();
            return;
        }

        InteractWithItem();
    }
}
