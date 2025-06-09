using UnityEngine;

[CreateAssetMenu(menuName = "Item/Equipable Item")]
public class EquipableItem : ScriptableObject
{
    public GameObject modelPrefab;
    public int damage;
}
