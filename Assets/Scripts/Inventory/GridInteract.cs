using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemGrid))]

public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private InventoryController inventoryController;
    private ItemGrid itemGrid;
    public bool pointerOnGrid;

    private void Awake()
    {
        pointerOnGrid = false;
        itemGrid = GetComponent<ItemGrid>();

        if (inventoryController == null)
            inventoryController = transform.root.GetComponentInChildren<InventoryController>(true);

        if (inventoryController == null)
            Debug.LogWarning("InventoryController reference is missing.", this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryController == null)
            return;

        inventoryController.SetLastPlacementItemGrid(itemGrid);
        inventoryController.SetItemGrid(itemGrid);
        pointerOnGrid = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryController == null)
            return;

        inventoryController.SetItemGrid(null);
        pointerOnGrid = false;
    }

}
