using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    private Dictionary<string, ItemData> itemsById;

    // Looks up an item definition by stable ID, which is how saves/network messages refer to items.
    public bool TryGetItemData(string itemId, out ItemData itemData)
    {
        EnsureLookup();
        return itemsById.TryGetValue(itemId, out itemData);
    }

    // Prototype helper for spawning random test items from the same catalog used by saves.
    public ItemData GetRandomItemData()
    {
        List<ItemData> availableItems = new List<ItemData>();
        foreach (ItemData item in items)
        {
            if (item != null)
                availableItems.Add(item);
        }

        if (availableItems.Count == 0)
            return null;

        int index = Random.Range(0, availableItems.Count);
        return availableItems[index];
    }

    // Builds the ID lookup lazily so designer-edited lists stay cheap until used.
    private void EnsureLookup()
    {
        if (itemsById != null)
            return;

        itemsById = new Dictionary<string, ItemData>();
        foreach (ItemData item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
                continue;

            if (!itemsById.ContainsKey(item.ItemId))
            {
                itemsById.Add(item.ItemId, item);
            }
            else
            {
                Debug.LogWarning(
                    $"Duplicate item id '{item.ItemId}' found on '{item.name}'. Save/load will use the first item with that id.",
                    this);
            }
        }
    }

    // Clears the cached lookup when the asset changes in the Inspector.
    private void OnValidate()
    {
        itemsById = null;
        ValidateItemIds();
    }

    // Warns designers early when two item assets would resolve to the same saved item id.
    private void ValidateItemIds()
    {
        HashSet<string> seenIds = new HashSet<string>();
        foreach (ItemData item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemId))
                continue;

            if (!seenIds.Add(item.ItemId))
                Debug.LogWarning($"Duplicate item id '{item.ItemId}' found in {name}.", this);
        }
    }
}
