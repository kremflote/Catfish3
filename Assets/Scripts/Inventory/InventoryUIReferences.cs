using UnityEngine;

// Central place for inventory UI references so gameplay code does not depend on scattered object-name searches.
public class InventoryUIReferences : MonoBehaviour
{
    [SerializeField] private Transform expandableBotleft;
    [SerializeField] private Transform expandableBotright;
    [SerializeField] private Transform expandableTopleft;
    [SerializeField] private Transform expandableTopright;
    [SerializeField] private Transform playerScreen;
    [SerializeField] private Transform worldScreen;
    [SerializeField] private Transform itemDescriptionScreen;
    [SerializeField] private Transform hotbar;

    public Transform ExpandableBotleft => expandableBotleft;
    public Transform ExpandableBotright => expandableBotright;
    public Transform ExpandableTopleft => expandableTopleft;
    public Transform ExpandableTopright => expandableTopright;
    public Transform PlayerScreen => playerScreen;
    public Transform WorldScreen => worldScreen;
    public Transform ItemDescriptionScreen => itemDescriptionScreen;
    public Transform Hotbar => hotbar;

    // Returns the standard grid child for one inventory panel.
    public ItemGrid GetItemGrid(Transform panel)
    {
        Transform greyGrid = FindGreyGrid(panel);
        return greyGrid != null ? greyGrid.GetComponent<ItemGrid>() : null;
    }

    // Returns the component that reports pointer enter/exit for one inventory panel.
    public GridInteract GetGridInteract(Transform panel)
    {
        Transform greyGrid = FindGreyGrid(panel);
        return greyGrid != null ? greyGrid.GetComponent<GridInteract>() : null;
    }

    // Returns the grid GameObject so toggle code can show/hide it with the owning panel.
    public GameObject GetGridObject(Transform panel)
    {
        Transform greyGrid = FindGreyGrid(panel);
        return greyGrid != null ? greyGrid.gameObject : null;
    }

    // Fills missing references from the canvas once, keeping name fallback contained to this setup helper.
    public void ResolveMissingReferences(Transform canvas)
    {
        if (canvas == null)
            return;

        expandableBotleft ??= FindDescendantByName(canvas, "expandable_botleft");
        expandableBotright ??= FindDescendantByName(canvas, "expandable_botright");
        expandableTopleft ??= FindDescendantByName(canvas, "expandable_topleft");
        expandableTopright ??= FindDescendantByName(canvas, "expandable_topright");
        playerScreen ??= FindDescendantByName(canvas, "player_screen");
        worldScreen ??= FindDescendantByName(canvas, "world_screen");
        itemDescriptionScreen ??= FindDescendantByName(canvas, "item_description_screen");
        hotbar ??= FindDescendantByName(canvas, "hotbar");
    }

    // Finds the grid child used by the current prefab layout.
    private Transform FindGreyGrid(Transform panel)
    {
        return panel != null ? FindDescendantByName(panel, "GreyGrid") : null;
    }

    // Finds legacy prefab children by name until those references are wired directly in the Inspector.
    private Transform FindDescendantByName(Transform root, string childName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}
