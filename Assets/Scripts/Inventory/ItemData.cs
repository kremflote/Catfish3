using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    public string itemName;
    public Sprite itemIcon;
    public int width = 1;
    public int height = 1;
    public int maxStack = 1;
    public bool equipable;

    public EquipableItem equipableData; // Reference to separate SO

    public string ItemId => itemId;

    // Unity editor hook: gives new item assets a stable ID if the designer has not typed one yet.
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemId))
            itemId = CreateDefaultId(name);

        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        maxStack = Mathf.Max(1, maxStack);
    }

    // Creates a readable default ID from the asset name, falling back to a GUID if there is no name.
    private static string CreateDefaultId(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return Guid.NewGuid().ToString("N");

        return source.Trim().ToLowerInvariant().Replace(" ", "_");
    }
}
