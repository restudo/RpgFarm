using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] MouseItemData mouseInventoryItem;
    protected InventorySystem inventorySystem;
    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary;
    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    public abstract void AssignSlot(InventorySystem invToDisplay);

    protected virtual void Start()
    {

    }

    protected virtual void UpdateSLot(InventorySlot updatedSlot)
    {
        foreach (var slot in SlotDictionary)
        {
            if (slot.Value == updatedSlot) // slot value - "the under the hood" inventory slot
            {
                slot.Key.UpdateUISlot(updatedSlot); // slot key - the ui representation of the value
            }
        }
    }

    public void SLotClicked(InventorySlot_UI clickedUISlot)
    {

        // clicked slot has an item and mouse doesnt have an item so pick up that item
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.assignedInventorySlot.ItemData == null)
        {
            // if player hold shift key? split the stack.

            mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
            clickedUISlot.ClearSlot();
            return;
        }

        // clicked slot doesnt have an item and mouse does have an item so place the mouse item into empty slot
        if (clickedUISlot.AssignedInventorySlot.ItemData == null && mouseInventoryItem.assignedInventorySlot.ItemData != null)
        {
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.assignedInventorySlot);
            clickedUISlot.UpdateUISlot();

            mouseInventoryItem.ClearSlot();
        }

        // both slots have an item, so decide...
        // are both items are the same? if so, combine the item
        // is the slot stack size + mouse stack size > the slot max stack size? if so, take from mouse
        // if different items, then swap the items
    }

}
