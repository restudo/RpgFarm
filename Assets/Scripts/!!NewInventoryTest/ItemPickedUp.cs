using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickedUp : MonoBehaviour
{
    public InventoryItemData itemData;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var inventory = collision.transform.GetComponent<InventoryHolder>();
        if (!inventory)
        {
            return;
        }
        if (inventory.InventorySystem.AddToInventory(itemData, 1))
        {
            Destroy(gameObject);
        }
    }
}
