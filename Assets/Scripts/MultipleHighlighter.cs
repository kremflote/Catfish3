using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


public class MultipleHighlighter : MonoBehaviour
{
    // Singlehighlighter har kun 1 instans som man kan �ke bredde og h�yde p�
    // denne har heller mange celler av 1x1 som oppgj�r highlight effekten.
    // hvorfor lagde jeg to forskjellige n�r jeg egt kunne gjenbrukt singlehighlighter for samme effekt? idk
    // denne brukes hvertfall p� hotbaren og singlehighlighter brukes p� inventory gridet

    [SerializeField] int width;

    [SerializeField] List<RectTransform> hotbarHighlighters;
    [SerializeField] public InventoryItem[,] inventoryItemSlot;

    public void Show(bool b)
    {
        foreach (RectTransform hotbarHighlighter in hotbarHighlighters)
        {
            hotbarHighlighter.gameObject.SetActive(b);
        }
    }
    public void SetSize(InventoryItem targetItem)
    {
        foreach (RectTransform hotbarHighlighter in hotbarHighlighters)
        {
            if (targetItem == null)
            {
                Vector2 nullSize = new Vector2();
                nullSize.x = 1 * ItemGrid.tileSizeWidth;
                nullSize.y = 1 * ItemGrid.tileSizeHeight;
                hotbarHighlighter.sizeDelta = nullSize;

            }
            else
            {
                Vector2 size = new Vector2();
                size.x = targetItem.Width * ItemGrid.tileSizeWidth;
                size.y = targetItem.Height * ItemGrid.tileSizeHeight;
                hotbarHighlighter.sizeDelta = size;
            }
        }

    }
    public void SetSize(int width, int heigth)
    {
        foreach (RectTransform hotbarHighlighter in hotbarHighlighters) {
            Vector2 size = new Vector2();
            size.x = width * ItemGrid.tileSizeWidth;
            size.y = heigth * ItemGrid.tileSizeHeight;
            hotbarHighlighter.sizeDelta = size;
        }
    }
    public void SetBehind()
    {
        foreach (RectTransform hotbarHighlighter in hotbarHighlighters)
        {
            hotbarHighlighter.SetAsFirstSibling();
        }


    }
    public void SetParent(ItemGrid targetGrid)
    {
        foreach (RectTransform hotbarHighlighter in hotbarHighlighters)
        {
            hotbarHighlighter.SetParent(targetGrid.GetComponent<RectTransform>(), false);
            hotbarHighlighter.localScale = Vector3.one;
        }
    }
    public void SetPosition(ItemGrid targetGrid, int posX, int posY)
    {
        for (int i = 0; i < hotbarHighlighters.Count; i++)
        {
            Vector2 pos = targetGrid.CalculatePositionOnGrid(
            posX + i,
            posY
            );

            hotbarHighlighters[i].localPosition = pos;
        }

    }
    internal void GenerateHighlighters(int numberOfHighlights, Transform parent)
        {
            if (hotbarHighlighters == null)
            {
                hotbarHighlighters = new List<RectTransform>();
            }

            // Clean up existing ones
            foreach (var highlighter in hotbarHighlighters)
            {
                if (highlighter != null)
                {
                    Destroy(highlighter.gameObject);
                }
            }
            hotbarHighlighters.Clear();

            for (int i = 0; i < numberOfHighlights; i++)
            {
            // Create GameObject with required UI components
            GameObject highlighterObject = new GameObject($"HotbarHighlighter_{i}", typeof(RectTransform), typeof(UnityEngine.UI.Image));


            // Set parent and position
            RectTransform rectTransform = highlighterObject.GetComponent<RectTransform>();
                rectTransform.SetParent(parent, false); // false keeps local scale and position

            // Set visual appearance
            UnityEngine.UI.Image image = highlighterObject.GetComponent<UnityEngine.UI.Image>();

            image.color = new Color(0.560f, 0.412f, 0.263f, 0.039f); // semi-transparent yellow

            width++;

            // Add to list
            hotbarHighlighters.Add(rectTransform);
            }
        InitializeInventoryItemSlots();
        }
    public void InitializeInventoryItemSlots()
    {
        inventoryItemSlot = new InventoryItem[1, width];
    }
    public void SetInventoryItemSlot(InventoryItem item, int x, int y)
    {

        if (item == null)
        {
            Debug.Log("Item is null, clearing hotbar slot");
            inventoryItemSlot[x, y] = null;
            return;
        }
            inventoryItemSlot[x, y] = item;
            Debug.Log("Item added to slot");
    }

    public InventoryItem[,] GetInventoryItemSlot()
    {
        return inventoryItemSlot;
    }
}
