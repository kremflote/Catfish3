using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryHighlighter : MonoBehaviour
{
    [SerializeField] private RectTransform highlighter;
    [SerializeField] private List<RectTransform> highlighters = new List<RectTransform>();

    // Lazily normalizes single-rectangle and multi-rectangle highlight modes into one list.
    private IReadOnlyList<RectTransform> ActiveHighlighters
    {
        get
        {
            if (highlighters == null)
                highlighters = new List<RectTransform>();

            if (highlighters.Count == 0 && highlighter != null)
                highlighters.Add(highlighter);

            return highlighters;
        }
    }

    // Shows or hides every rectangle managed by this highlighter.
    public void Show(bool isVisible)
    {
        foreach (RectTransform target in ActiveHighlighters)
        {
            if (target != null)
                target.gameObject.SetActive(isVisible);
        }
    }

    // Sizes the highlight to match a held or hovered inventory item.
    public void SetSize(InventoryItemUI targetItem)
    {
        int width = targetItem != null ? targetItem.Width : 1;
        int height = targetItem != null ? targetItem.Height : 1;
        SetSize(width, height);
    }

    // Sizes the highlight directly in grid tiles, used by hotbar row highlighting.
    public void SetSize(int width, int height)
    {
        Vector2 size = new Vector2(width * ItemGrid.tileSizeWidth, height * ItemGrid.tileSizeHeight);

        foreach (RectTransform target in ActiveHighlighters)
        {
            if (target != null)
                target.sizeDelta = size;
        }
    }

    // Places the highlight on an existing item's current grid position.
    public void SetPosition(ItemGrid targetGrid, InventoryItemUI targetItem)
    {
        SetParent(targetGrid);
        SetPosition(targetGrid, targetItem, targetItem.GetonGridPositionX(), targetItem.GetonGridPositionY());
    }

    // Places the highlight at a specific grid position using an item's footprint size.
    public void SetPosition(ItemGrid targetGrid, InventoryItemUI targetItem, int posX, int posY)
    {
        if (targetGrid == null || targetItem == null)
            return;

        Vector2 position = targetGrid.CalculatePositionOnGrid(targetItem, posX, posY);
        SetAllPositions(position);
    }

    // Places each highlighter rectangle across consecutive tiles, used for the hotbar row band.
    public void SetPosition(ItemGrid targetGrid, int posX, int posY)
    {
        if (targetGrid == null)
            return;

        int index = 0;
        foreach (RectTransform target in ActiveHighlighters)
        {
            if (target == null)
                continue;

            target.localPosition = targetGrid.CalculatePositionOnGrid(posX + index, posY);
            index++;
        }
    }

    // Sends highlight graphics behind item icons so they read as selection/hover backgrounds.
    public void SetBehind()
    {
        foreach (RectTransform target in ActiveHighlighters)
        {
            if (target != null)
                target.SetAsFirstSibling();
        }
    }

    // Parents highlight rectangles under the target grid so local tile positions line up.
    public void SetParent(ItemGrid targetGrid)
    {
        if (targetGrid == null)
            return;

        RectTransform gridTransform = targetGrid.GetComponent<RectTransform>();
        foreach (RectTransform target in ActiveHighlighters)
        {
            if (target == null)
                continue;

            target.SetParent(gridTransform, false);
            target.localScale = Vector3.one;
        }
    }

    // Creates or reuses enough rectangles to highlight multiple tiles independently.
    internal void GenerateHighlighters(int count, Transform parent)
    {
        if (highlighters == null)
            highlighters = new List<RectTransform>();

        foreach (RectTransform target in highlighters)
        {
            if (target != null && target != highlighter)
                Destroy(target.gameObject);
        }

        highlighters.Clear();

        for (int i = 0; i < count; i++)
        {
            RectTransform target = i == 0 && highlighter != null
                ? highlighter
                : CreateHighlighter(i, parent);

            target.SetParent(parent, false);
            target.localScale = Vector3.one;
            highlighters.Add(target);
        }
    }

    // Builds one runtime UI rectangle for generated multi-tile highlights.
    private RectTransform CreateHighlighter(int index, Transform parent)
    {
        GameObject highlighterObject = new GameObject($"InventoryHighlighter_{index}", typeof(RectTransform), typeof(Image));
        RectTransform target = highlighterObject.GetComponent<RectTransform>();
        target.SetParent(parent, false);

        Image image = highlighterObject.GetComponent<Image>();
        image.color = new Color(0.560f, 0.412f, 0.263f, 0.039f);

        return target;
    }

    // Applies the same local position to every active rectangle.
    private void SetAllPositions(Vector2 position)
    {
        foreach (RectTransform target in ActiveHighlighters)
        {
            if (target != null)
                target.localPosition = position;
        }
    }
}
