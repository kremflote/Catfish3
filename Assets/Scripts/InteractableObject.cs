using FishNet.Connection;
using FishNet.Object;
using System.Globalization;
using UnityEngine;

public class InteractableObject : NetworkBehaviour
{
    public string itemName;
    public InventoryItem inventoryItem; // Not synced by default — must send manually

    private bool isTaken = false;

    [ServerRpc(RequireOwnership = false)]
    public void RequestPickupServerRpc(NetworkConnection conn = null)
    {
        if (isTaken) return;

        GameObject playerObj = conn?.FirstObject?.gameObject;
        if (playerObj == null) return;

        var inv = playerObj.GetComponentInChildren<InventoryController>();
        if (inv != null)
        {
            inv.InsertItem(inventoryItem); // pass itemData from this object
            isTaken = true;
            Despawn(); // Despawn this object on all clients
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.Mouse0))
        {
            RequestPickupServerRpc();
        }
    }
}
