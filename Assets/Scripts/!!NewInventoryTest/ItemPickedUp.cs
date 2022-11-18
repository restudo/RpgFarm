using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickedUp : MonoBehaviour
{
    [SerializeField] private int itemID;
    public int ItemID { get { return itemID; } set { itemID = value; } }
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
