using UnityEngine;
using StarterAssets;

public partial class InventoryController
{
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

    private void OnDisable()
    {
        if (playerInputState == null)
            return;

        playerInputState.OnQPressed -= HandleCreateRandomItem;
        playerInputState.OnTPressed -= HandleInsertAll;
        playerInputState.OnUPressed -= HandleInsertRandom;
        playerInputState.OnMouseClick -= HandleMouseClick;
    }

    private void HandleCreateRandomItem()
    {
        if (selectedItem == null)
            CreateRandomItem();
    }

    private void HandleInsertAll()
    {
        InsertAllUIElements();
    }

    private void HandleInsertRandom()
    {
        InsertRandomItem();
    }

    private void HandleMouseClick(InventoryMouseButton button)
    {
        if (selectedItemGrid == null || IsPointerOffGrid())
            return;

        if (button == InventoryMouseButton.Left)
        {
            HandleLeftMouseClick();
        }
        else if (button == InventoryMouseButton.Right && selectedItem != null)
        {
            FlipSelectedItem();
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
}
