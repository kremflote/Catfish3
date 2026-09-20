using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemGrid))]

public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private InventoryController inventoryController;
    private ItemGrid itemGrid;
    public bool pointerOnGrid;

    // Finds the grid and inventory controller this hover detector should report to.
    private void Awake()
    {
        pointerOnGrid = false;
        itemGrid = GetComponent<ItemGrid>();

        if (inventoryController == null)
            inventoryController = transform.root.GetComponentInChildren<InventoryController>(true);

        if (inventoryController == null)
            Debug.LogWarning("InventoryController reference is missing.", this);
    }

    // Unity UI calls this when the pointer enters the grid; inventory clicks should now target this grid.
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryController == null)
            return;

        inventoryController.SetHoveredGrid(itemGrid);
        inventoryController.SetItemGrid(itemGrid);
        pointerOnGrid = true;
    }

    // Unity UI calls this when the pointer leaves the grid; inventory clicks should stop targeting it.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryController == null)
            return;

        inventoryController.SetHoveredGrid(null);
        inventoryController.SetItemGrid(null);
        pointerOnGrid = false;
    }

}
