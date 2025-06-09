using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int width = 1;
    public int height = 1;
    public bool equipable;

    public EquipableItem equipableData; // Reference to separate SO
}
