using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemGrid))]

public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    InventoryController inventoryController;
    ItemGrid itemGrid;
    public bool pointerOnGrid;

    private void Awake()
    {
        Transform parent = transform.parent.parent.parent;
        Transform playerCamera = parent.Find("MainCamera");

        pointerOnGrid = false;
        inventoryController = playerCamera.GetComponent<InventoryController>();
        itemGrid = GetComponent<ItemGrid>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryController.SetLastPlacementItemGrid(itemGrid);
        inventoryController.SetItemGrid(itemGrid);
        pointerOnGrid = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
        inventoryController.SetItemGrid(null);
        pointerOnGrid = false;
    }

}
