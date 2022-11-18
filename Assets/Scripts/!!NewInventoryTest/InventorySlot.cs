using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private int stackSize;

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;

    public InventorySlot(InventoryItemData source, int amount)
    {
        itemData = source;
        stackSize = amount;
    }

    public InventorySlot()
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        itemData = null;
        stackSize = -1;
    }

    public void UpdateInventorySlot(InventoryItemData data, int amount)
    {
        itemData = data;
        stackSize = amount;
    }

    public bool RoomLeftInStack(int amountToAdd, out int amountRemaining)
    {
        amountRemaining = itemData.maxStackSize - stackSize;
        return RoomLeftInStack(amountToAdd);
    }

    public bool RoomLeftInStack(int amountToAdd)
    {
        if (stackSize + amountToAdd <= itemData.maxStackSize)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
    }

    public void RemoveToStack(int amount)
    {
        stackSize -= amount;
    }
}
