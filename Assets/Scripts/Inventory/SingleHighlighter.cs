using UnityEngine;

public class SingleHighlighter : MonoBehaviour
{
    [SerializeField] RectTransform highlighter;

    public void Show(bool b)
    {
        highlighter.gameObject.SetActive(b);
    }
    public void SetSize(InventoryItem targetItem)
    {
        if ( targetItem == null)
        {
            Vector2 nullSize = new Vector2();
            nullSize.x = 1 * ItemGrid.tileSizeWidth;
            nullSize.y = 1 * ItemGrid.tileSizeHeight;
            highlighter.sizeDelta = nullSize;
            
        }
        else { 
            Vector2 size = new Vector2();
        size.x = targetItem.Width * ItemGrid.tileSizeWidth;
        size.y = targetItem.Height * ItemGrid.tileSizeHeight;
        highlighter.sizeDelta = size;
        }
    }

    public void SetPosition (ItemGrid targetGrid, InventoryItem targetItem)
    {
        SetParent(targetGrid);
        Vector2 pos = targetGrid.CalculatePositionOnGrid(
            targetItem,
            targetItem.GetonGridPositionX(),
            targetItem.GetonGridPositionY()
            );

        highlighter.localPosition = pos;
    }

    public void SetBehind() 
    {
        highlighter.SetAsFirstSibling();

    }

    public void SetParent(ItemGrid targetGrid)
    {
        highlighter.SetParent(targetGrid.GetComponent<RectTransform>(), false);
        highlighter.localScale = Vector3.one;
    }

    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem, int posX, int posY)
    {
        Vector2 pos = targetGrid.CalculatePositionOnGrid(
            targetItem,
            posX,
            posY
            );
        highlighter.localPosition = pos;
    }

    public void SetPosition(ItemGrid targetGrid, int posX, int posY)
    {
        Vector2 pos = targetGrid.CalculatePositionOnGrid(
            posX,
            posY
            );
        highlighter.localPosition = pos;
    }
}
