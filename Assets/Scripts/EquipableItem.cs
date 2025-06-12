using UnityEngine;

[CreateAssetMenu(menuName = "Item/Equipable Item")]
public class EquipableItem : ScriptableObject
{
    // representasjon av et item når equipped

    public GameObject modelPrefab;
    public int damage;
}
