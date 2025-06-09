using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    private GameObject currentEquippedModel;

    [SerializeField] public Transform PlayerCapsule;

    public void EquipItem(InventoryItem item)
    {
        EquipableItem equipable = item.itemData.equipableData;

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

    public void Unequip()
    {
        if (currentEquippedModel != null)
        {
            Destroy(currentEquippedModel);
            currentEquippedModel = null;
        }
    }
}
