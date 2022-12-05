using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private bool isItemCanBeAdd = true;
    private int sumItemInInventory = 0;

    private int maxStack = 3;

    private Dictionary<int, ItemDetails> itemDetailsDictionary;

    private int[] selectedInventoryItem; // the index of the array is the inventory list, and the value is the item code
    public List<InventoryItem>[] inventoryLists;

    [HideInInspector] public int[] inventoryListCapacityIntArray;   // the index of the array is the inventory list (from the InventoryLocation enum), 
                                                                    //and the value is the capacity of that inventory list

    [SerializeField] private SO_ItemList itemList = null;


    protected override void Awake()
    {
        base.Awake();

        // Create Inventory Lists
        CreateInventoryLists();

        // Create item details dictionary
        CreateItemDetailsDictionary();

        // Initailise selected inventory item array
        selectedInventoryItem = new int[(int)InventoryLocation.count];

        for (int i = 0; i < selectedInventoryItem.Length; i++)
        {
            selectedInventoryItem[i] = -1;
        }

    }

    private void Update()
    {
        // test to advance the backpack by 1
        if (Input.GetKeyDown(KeyCode.M) && inventoryListCapacityIntArray[(int)InventoryLocation.player] < Settings.playerMaximumInventoryCapacity)
        {
            inventoryListCapacityIntArray[(int)InventoryLocation.player] += 1;
        }
    }

    private void CreateInventoryLists()
    {
        inventoryLists = new List<InventoryItem>[(int)InventoryLocation.count];

        for (int i = 0; i < (int)InventoryLocation.count; i++)
        {
            inventoryLists[i] = new List<InventoryItem>();
        }

        // initialise inventory list capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        // initialise player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;
        // Debug.Log(inventoryListCapacityIntArray[(int)InventoryLocation.player]);

        // initialise chest inventory list capacity
        // inventoryListCapacityIntArray[(int)InventoryLocation.chest] = Settings.ChestInitialInventoryCapacity;
    }

    /// <summary>
    ///  Populates the itemDetailsDictionary from the scriptable object items list 
    /// </summary>
    private void CreateItemDetailsDictionary()
    {
        itemDetailsDictionary = new Dictionary<int, ItemDetails>();

        foreach (ItemDetails itemDetails in itemList.itemDetails)
        {
            itemDetailsDictionary.Add(itemDetails.itemCode, itemDetails);
        }
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation and then destroy the gameObjectToDelete
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item, GameObject gameObjectToDelete)
    {
        AddItem(inventoryLocation, item);

        if (isItemCanBeAdd)
        {
            Destroy(gameObjectToDelete);
        }
    }

    /// <summary>
    /// Add an item to the inventory list for the inventoryLocation
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, Item item)
    {
        int itemCode = item.ItemCode;
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
            isItemCanBeAdd = true;
        }
        else
        {
            if (sumItemInInventory < inventoryListCapacityIntArray[(int)InventoryLocation.player])
            {
                AddItemAtPosition(inventoryList, itemCode);
                isItemCanBeAdd = true;
            }
            else
            {
                isItemCanBeAdd = false;
            }
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }

    /// <summary>
    /// Add an item of type itemCode to the inventory list for the inventoryLocation
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryList, itemCode, itemPosition);
            isItemCanBeAdd = true;
        }
        else
        {
            AddItemAtPosition(inventoryList, itemCode);
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
    }


    /// <summary>
    /// Add item to the end of the inventory
    /// </summary>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        inventoryItem.itemCode = itemCode;
        inventoryItem.itemQuantity = 1;
        inventoryList.Add(inventoryItem);

        sumItemInInventory += 1;
        Debug.Log(sumItemInInventory);

        //DebugPrintInventoryList(inventoryList);
    }

    /// <summary>
    /// Add item to position in the inventory
    /// </summary>
    private void AddItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity + 1;
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        inventoryList[position] = inventoryItem;


        //DebugPrintInventoryList(inventoryList);
    }

    ///<summary>
    ///Swap item at fromItem index with item at toItem index in inventoryLocation inventory list
    ///</summary>

    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        // if fromItem index and toItemIndex are within the bounds of the list, not the same, and greater than or equal to zero
        if (fromItem < inventoryLists[(int)inventoryLocation].Count && toItem < inventoryLists[(int)inventoryLocation].Count
             && fromItem != toItem && fromItem >= 0 && toItem >= 0)
        {
            InventoryItem fromInventoryItem = inventoryLists[(int)inventoryLocation][fromItem];
            InventoryItem toInventoryItem = inventoryLists[(int)inventoryLocation][toItem];

            if (inventoryLists[(int)inventoryLocation][fromItem].itemCode == inventoryLists[(int)inventoryLocation][toItem].itemCode)
            {
                // just swap it
                if (inventoryLists[(int)inventoryLocation][fromItem].itemQuantity >= maxStack &&
                    inventoryLists[(int)inventoryLocation][toItem].itemQuantity >= maxStack ||
                    inventoryLists[(int)inventoryLocation][fromItem].itemQuantity >= maxStack &&
                    inventoryLists[(int)inventoryLocation][toItem].itemQuantity <= maxStack ||
                    inventoryLists[(int)inventoryLocation][fromItem].itemQuantity <= maxStack &&
                    inventoryLists[(int)inventoryLocation][toItem].itemQuantity >= maxStack)
                {
                    inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
                    inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;
                }

                // combine when the itemcode is same
                else if (inventoryLists[(int)inventoryLocation][fromItem].itemQuantity <= maxStack &&
                    inventoryLists[(int)inventoryLocation][toItem].itemQuantity <= maxStack ||
                    inventoryLists[(int)inventoryLocation][fromItem].itemQuantity < inventoryLists[(int)inventoryLocation][toItem].itemQuantity ||
                    inventoryLists[(int)inventoryLocation][toItem].itemQuantity < inventoryLists[(int)inventoryLocation][fromItem].itemQuantity)
                {
                    int beforeSwapToInventory = toInventoryItem.itemQuantity;
                    int beforeSwapFromInventory = fromInventoryItem.itemQuantity;

                    toInventoryItem.itemQuantity += fromInventoryItem.itemQuantity;
                    fromInventoryItem.itemQuantity -= toInventoryItem.itemQuantity;

                    if (toInventoryItem.itemQuantity > maxStack)
                    {
                        for (int i = toInventoryItem.itemQuantity; i > maxStack; i--)
                        {
                            toInventoryItem.itemQuantity -= 1;
                        }
                    }

                    if (fromInventoryItem.itemQuantity <= 0)
                    {
                        if (beforeSwapToInventory + beforeSwapFromInventory <= maxStack)
                        {
                            for (int i = fromInventoryItem.itemQuantity; i < 0; i++)
                            {
                                fromInventoryItem.itemQuantity += 1;
                            }
                        }
                        else
                        {
                            fromInventoryItem.itemQuantity = toInventoryItem.itemQuantity - beforeSwapToInventory;
                        }
                    }
                    inventoryLists[(int)inventoryLocation][toItem] = toInventoryItem;
                    inventoryLists[(int)inventoryLocation][fromItem] = fromInventoryItem;
                }
            }
            else
            {
                inventoryLists[(int)inventoryLocation][toItem] = fromInventoryItem;
                inventoryLists[(int)inventoryLocation][fromItem] = toInventoryItem;
            }

            //  Send event that inventory has been updated
            EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);
        }
    }

    /// <summary>
    /// Clear the selected inventory item for inventoryLocation
    /// </summary>
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }

    /// <summary>
    /// Returns the itemDetails (from the SO_ItemList) for the itemCode, or null if the item code doesn't exist
    /// </summary>

    public ItemDetails GetItemDetails(int itemCode)
    {
        ItemDetails itemDetails;

        if (itemDetailsDictionary.TryGetValue(itemCode, out itemDetails))
        {
            return itemDetails;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the itemDetails (from the SO_ItemList) for the currently selected item in the inventoryLocation , or null if an item isn't selected
    /// </summary>
    public ItemDetails GetSelectedInventoryItemDetails(InventoryLocation inventoryLocation)
    {
        int itemCode = GetSelectedInventoryItem(inventoryLocation);

        if (itemCode == -1)
        {
            return null;
        }
        else
        {
            return GetItemDetails(itemCode);
        }
    }


    /// <summary>
    /// Get the selected item for inventoryLocation - returns itemCode or -1 if nothing is selected
    /// </summary>
    private int GetSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        return selectedInventoryItem[(int)inventoryLocation];
    }

    /// <summary>
    /// Get the item type description for an item type - returns the item type description as a string for a given ItemType
    /// </summary>

    public string GetItemTypeDescription(ItemType itemType)
    {
        string itemTypeDescription;
        switch (itemType)
        {
            case ItemType.Breaking_tool:
                itemTypeDescription = Settings.BreakingTool;
                break;

            case ItemType.Chopping_tool:
                itemTypeDescription = Settings.ChoppingTool;
                break;

            case ItemType.Hoeing_tool:
                itemTypeDescription = Settings.HoeingTool;
                break;

            case ItemType.Reaping_tool:
                itemTypeDescription = Settings.ReapingTool;
                break;

            case ItemType.Watering_tool:
                itemTypeDescription = Settings.WateringTool;
                break;

            case ItemType.Collecting_tool:
                itemTypeDescription = Settings.CollectingTool;
                break;

            default:
                itemTypeDescription = itemType.ToString();
                break;
        }

        return itemTypeDescription;
    }

    /// <summary>
    /// Find if an itemCode is already in the inventory. Returns the item position
    /// in the inventory list, or -1 if the item is not in the inventory
    /// </summary>
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        for (int i = 0; i < inventoryList.Count; i++)
        {
            if (inventoryList[i].itemQuantity == maxStack)
            {
                continue;
            }
            else if (inventoryList[i].itemCode == itemCode)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Remove an item from the inventory, and create a game object at the position it was dropped
    /// </summary>
    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode, int itemPosition)
    {
        List<InventoryItem> inventoryList = inventoryLists[(int)inventoryLocation];

        // Check if inventory already contains the item
        // int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryList, itemCode, itemPosition);
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryLists[(int)inventoryLocation]);

    }

    private void RemoveItemAtPosition(List<InventoryItem> inventoryList, int itemCode, int position)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryList[position].itemQuantity - 1;

        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
            inventoryList[position] = inventoryItem;
        }
        else
        {
            inventoryList.RemoveAt(position);
            sumItemInInventory -= 1;
            Debug.Log(sumItemInInventory);
        }
    }

    /// <summary>
    /// Set the selected inventory item for inventoryLocation to itemCode
    /// </summary>
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    //private void DebugPrintInventoryList(List<InventoryItem> inventoryList)
    //{
    //    foreach (InventoryItem inventoryItem in inventoryList)
    //    {
    //        Debug.Log("Item Description:" + InventoryManager.Instance.GetItemDetails(inventoryItem.itemCode).itemDescription + "    Item Quantity: " + inventoryItem.itemQuantity);
    //    }
    //    Debug.Log("******************************************************************************");
    //}

}
