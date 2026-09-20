using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    private GameObject currentEquippedModel;

    [SerializeField] public Transform PlayerCapsule;

    // Backwards-compatible entry point for code that still passes the UI item.
    public void EquipItem(InventoryItemUI item)
    {
        EquipEntry(item != null ? item.EnsureEntry() : null);
    }

    // Equips from the runtime item instance, which keeps equipment independent of inventory UI.
    public void EquipEntry(InventoryItemEntry entry)
    {
        EquipItemData(entry != null ? entry.ItemData : null);
    }

    // Spawns the held/equipped world model defined by ItemData's equipable data.
    public void EquipItemData(ItemData itemData)
    {
        EquipableItem equipable = itemData != null ? itemData.equipableData : null;

        if (currentEquippedModel != null)
        {
            Destroy(currentEquippedModel);
        }

        if (equipable != null && equipable.modelPrefab != null)
        {
            // Instantiate the model and parent it to the correct slot
            currentEquippedModel = Instantiate(equipable.modelPrefab, PlayerCapsule);

            // Set local transform
            // currentEquippedModel.transform.localPosition = equipable.localPositionOffset;
            // currentEquippedModel.transform.localRotation = Quaternion.Euler(equipable.localRotationOffset);
        }
    }

    // Removes the currently equipped model from the player.
    public void Unequip()
    {
        if (currentEquippedModel != null)
        {
            Destroy(currentEquippedModel);
            currentEquippedModel = null;
        }
    }
}
