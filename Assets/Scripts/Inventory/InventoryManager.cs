using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>, ISaveable
{
    private UIInventoryBar inventoryBar;
    private bool isItemCanBeAdd = true;
    private int sumItemInInventory = 0;

    private int maxStack = 5;

    private Dictionary<int, ItemDetails> itemDetailsDictionary;

    private int[] selectedInventoryItem; // the index of the array is the inventory list, and the value is the item code
    // public List<InventoryItem>[] inventoryLists;
    public Dictionary<int, InventoryItem>[] inventoryDictionaries;

    //create dict for player inv
    private Dictionary<int, InventoryItem> playerDict = new Dictionary<int, InventoryItem>();
    private int _playerInvIndex;
    public int PlayerInvIndex { get => _playerInvIndex; set => _playerInvIndex = value; }

    //create dict for chest inv
    private Dictionary<int, InventoryItem> chestDict = new Dictionary<int, InventoryItem>();

    [HideInInspector] public int[] inventoryListCapacityIntArray;   // the index of the array is the inventory list (from the InventoryLocation enum), 
                                                                    //and the value is the capacity of that inventory list

    [SerializeField] private SO_ItemList itemList = null;


    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }


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

        // Get unique ID for gameobject and create save data object
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();
    }

    private void OnDisable()
    {
        ISaveableDeregister();
    }


    private void OnEnable()
    {
        ISaveableRegister();
    }

    private void Start()
    {
        inventoryBar = FindObjectOfType<UIInventoryBar>();
    }

    private void Update()
    {
        // test to advance the backpack by 1
        if (Input.GetKeyDown(KeyCode.M) && inventoryListCapacityIntArray[(int)InventoryLocation.player] < Settings.playerMaximumInventoryCapacity)
        {
            // Extend Player inventory by 12
            for (int i = 0; i < 12; i++)
            {
                PlayerInvIndex = inventoryListCapacityIntArray[(int)InventoryLocation.player] += 1;
                UpdateDictionaryForPlayerInventory();
            }
        }
    }

    private void UpdateDictionaryForPlayerInventory()
    {
        InventoryItem invItem;
        invItem.itemCode = 0;
        invItem.itemQuantity = 0;
        playerDict.Add(PlayerInvIndex - 1, invItem);

        inventoryDictionaries[(int)InventoryLocation.player] = playerDict;
    }

    private void CreateInventoryLists()
    {
        // initialise inventory list capacity array
        inventoryListCapacityIntArray = new int[(int)InventoryLocation.count];

        // initialise player inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.player] = Settings.playerInitialInventoryCapacity;

        // initialise chest inventory list capacity
        inventoryListCapacityIntArray[(int)InventoryLocation.chest] = Settings.ChestInitialInventoryCapacity;
        inventoryDictionaries = new Dictionary<int, InventoryItem>[(int)InventoryLocation.count];

        // create player inventory from dictionary
        for (PlayerInvIndex = 0; PlayerInvIndex < inventoryListCapacityIntArray[(int)InventoryLocation.player]; PlayerInvIndex++)
        {
            InventoryItem invItem;
            invItem.itemCode = 0;
            invItem.itemQuantity = 0;
            playerDict.Add(PlayerInvIndex, invItem);
        }

        inventoryDictionaries[(int)InventoryLocation.player] = playerDict;

        // Create chest inventory from dictionary
        for (int i = 0; i < inventoryListCapacityIntArray[(int)InventoryLocation.chest]; i++)
        {
            InventoryItem invItem;
            invItem.itemCode = 0;
            invItem.itemQuantity = 0;
            chestDict.Add(i, invItem);
        }

        inventoryDictionaries[(int)InventoryLocation.chest] = chestDict;

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

        if (isItemCanBeAdd == true)
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

        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];

        // Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryDict, itemCode, itemPosition);
            isItemCanBeAdd = true;
        }
        else
        {
            if (sumItemInInventory < inventoryListCapacityIntArray[(int)InventoryLocation.player])
            {
                AddItemAtPosition(inventoryDict, itemCode);
                isItemCanBeAdd = true;
            }
            else
            {
                isItemCanBeAdd = false;
            }
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }

    /// <summary>
    /// Add an item of type itemCode to the inventory list for the inventoryLocation
    /// </summary>
    public void AddItem(InventoryLocation inventoryLocation, int itemCode)
    {
        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];

        // Check if inventory already contains the item
        int itemPosition = FindItemInInventory(inventoryLocation, itemCode);

        if (itemPosition != -1)
        {
            AddItemAtPosition(inventoryDict, itemCode, itemPosition);
            isItemCanBeAdd = true;
        }
        else
        {
            if (sumItemInInventory < inventoryListCapacityIntArray[(int)InventoryLocation.player])
            {
                AddItemAtPosition(inventoryDict, itemCode);
                isItemCanBeAdd = true;
            }
            else
            {
                isItemCanBeAdd = false;
            }
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }

    private int GetFirstEmptyItemSlot(Dictionary<int, InventoryItem> inventoryDict)
    {
        foreach (KeyValuePair<int, InventoryItem> item in inventoryDict)
        {
            if (item.Value.itemCode == 0)
            {
                return item.Key;
            }
        }
        return -1;
    }

    /// <summary>
    /// Add item at the end of the inv
    /// </summary>
    /// <param name="inventoryList"></param>
    /// <param name="itemCode"></param>
    private void AddItemAtPosition(Dictionary<int, InventoryItem> inventoryDict, int itemCode)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int itemSlot = GetFirstEmptyItemSlot(inventoryDict);
        if (itemSlot != -1)
        {
            inventoryItem.itemCode = itemCode;
            inventoryItem.itemQuantity = 1;
            inventoryDict[itemSlot] = inventoryItem;

            sumItemInInventory += 1;
        }
    }

    /// <summary>
    /// Add Quantity to item
    /// </summary>
    /// <param name="inventoryList"></param>
    /// <param name="itemCode"></param>
    /// <param name="itemPosition"></param>
    private void AddItemAtPosition(Dictionary<int, InventoryItem> inventoryDict, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();

        int quantity = inventoryDict[itemPosition].itemQuantity + 1;
        inventoryItem.itemQuantity = quantity;
        inventoryItem.itemCode = itemCode;
        inventoryDict[itemPosition] = inventoryItem;
    }

    /// <summary>
    /// Split item quantity
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="fromItem"></param>
    /// <param name="toItem"></param>
    /// <param name="stackSize"></param>
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem, int stackSize)
    {
        if (fromItem != toItem && fromItem >= 0)
        {
            if (inventoryDictionaries[(int)inventoryLocation].ContainsKey(toItem))
            {
                InventoryItem fromInventoryItem = inventoryDictionaries[(int)inventoryLocation][fromItem];
                InventoryItem toInventoryItem = inventoryDictionaries[(int)inventoryLocation][toItem];

                if (fromInventoryItem.itemCode != toInventoryItem.itemCode && toInventoryItem.itemCode != 0)
                {
                    inventoryDictionaries[(int)inventoryLocation][toItem] = fromInventoryItem;
                    inventoryDictionaries[(int)inventoryLocation][fromItem] = toInventoryItem;
                }

                toInventoryItem.itemQuantity += stackSize;
                fromInventoryItem.itemQuantity -= stackSize;

                // when drop to an empty slot
                if (toInventoryItem.itemCode == 0)
                {
                    toInventoryItem.itemCode = fromInventoryItem.itemCode;

                    inventoryDictionaries[(int)inventoryLocation][toItem] = toInventoryItem;
                    inventoryDictionaries[(int)inventoryLocation][fromItem] = fromInventoryItem;
                }

                // when drop to a slot with same itemcode 
                if (inventoryDictionaries[(int)inventoryLocation][fromItem].itemCode == inventoryDictionaries[(int)inventoryLocation][toItem].itemCode)
                {
                    if (toInventoryItem.itemQuantity > maxStack)
                    {
                        for (int i = toInventoryItem.itemQuantity; i > maxStack; i--)
                        {
                            toInventoryItem.itemQuantity -= 1;
                            fromInventoryItem.itemQuantity += 1;
                        }
                    }

                    inventoryDictionaries[(int)inventoryLocation][toItem] = toInventoryItem;
                    inventoryDictionaries[(int)inventoryLocation][fromItem] = fromInventoryItem;
                }
            }
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }

    ///<summary>
    ///Swap item at fromItem index with item at toItem index in inventoryLocation inventory list
    ///</summary>
    public void SwapInventoryItems(InventoryLocation inventoryLocation, int fromItem, int toItem)
    {
        // if fromItem index and toItemIndex are within the bounds of the list, not the same, and greater than or equal to zero
        if (fromItem != toItem && fromItem >= 0)
        {
            if (inventoryDictionaries[(int)inventoryLocation].ContainsKey(toItem))
            {
                InventoryItem fromInventoryItem = inventoryDictionaries[(int)inventoryLocation][fromItem];
                InventoryItem toInventoryItem = inventoryDictionaries[(int)inventoryLocation][toItem];

                if (inventoryDictionaries[(int)inventoryLocation][fromItem].itemCode == inventoryDictionaries[(int)inventoryLocation][toItem].itemCode)
                {
                    // just swap it
                    if (inventoryDictionaries[(int)inventoryLocation][fromItem].itemQuantity >= maxStack &&
                            inventoryDictionaries[(int)inventoryLocation][toItem].itemQuantity >= maxStack ||
                            inventoryDictionaries[(int)inventoryLocation][fromItem].itemQuantity >= maxStack &&
                            inventoryDictionaries[(int)inventoryLocation][toItem].itemQuantity <= maxStack ||
                            inventoryDictionaries[(int)inventoryLocation][fromItem].itemQuantity <= maxStack &&
                            inventoryDictionaries[(int)inventoryLocation][toItem].itemQuantity >= maxStack)
                    {
                        inventoryDictionaries[(int)inventoryLocation][toItem] = fromInventoryItem;
                        inventoryDictionaries[(int)inventoryLocation][fromItem] = toInventoryItem;
                    }

                    // Combine when its same itemcode
                    else if (inventoryDictionaries[(int)inventoryLocation][fromItem].itemQuantity <= maxStack &&
                    inventoryDictionaries[(int)inventoryLocation][toItem].itemQuantity <= maxStack ||
                    inventoryDictionaries[(int)inventoryLocation][fromItem].itemQuantity < inventoryDictionaries[(int)inventoryLocation][toItem].itemQuantity ||
                    inventoryDictionaries[(int)inventoryLocation][toItem].itemQuantity < inventoryDictionaries[(int)inventoryLocation][fromItem].itemQuantity)
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
                        inventoryDictionaries[(int)inventoryLocation][toItem] = toInventoryItem;
                        inventoryDictionaries[(int)inventoryLocation][fromItem] = fromInventoryItem;
                    }
                }
                // When its different itemcode just swap
                else
                {
                    inventoryDictionaries[(int)inventoryLocation][toItem] = fromInventoryItem;
                    inventoryDictionaries[(int)inventoryLocation][fromItem] = toInventoryItem;
                }
            }
        }
        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }

    /// <summary>
    /// Clear the selected inventory item for inventoryLocation
    /// </summary>
    public void ClearSelectedInventoryItem(InventoryLocation inventoryLocation)
    {
        selectedInventoryItem[(int)inventoryLocation] = -1;
    }

    /// <summary>
    /// Find if itemCode is already in the inv
    /// Returns itemPosition in List or -1
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    public int FindItemInInventory(InventoryLocation inventoryLocation, int itemCode)
    {
        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];

        foreach (KeyValuePair<int, InventoryItem> item in inventoryDict)
        {
            if (item.Value.itemQuantity == maxStack)
            {
                continue;
            }
            else if (item.Value.itemCode == itemCode)
            {
                return item.Key;
            }
        }

        return -1;
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
    /// Remove item from the inventory
    /// </summary>
    /// <param name="inventoryLocation"></param>
    /// <param name="itemCode"></param>
    public void RemoveItem(InventoryLocation inventoryLocation, int itemCode, int itemPosition)
    {
        Dictionary<int, InventoryItem> inventoryDict = inventoryDictionaries[(int)inventoryLocation];
        if (itemPosition != -1)
        {
            RemoveItemAtPosition(inventoryDict, itemCode, itemPosition);
        }
        EventHandler.CallInventoryUpdatedEvent(inventoryLocation, inventoryDictionaries[(int)inventoryLocation]);
    }

    /// <summary>
    /// Remove item at a specific position from the inventory
    /// </summary>
    /// <param name="inventoryList"></param>
    /// <param name="itemCode"></param>
    /// <param name="itemPosition"></param>
    private void RemoveItemAtPosition(Dictionary<int, InventoryItem> inventoryDict, int itemCode, int itemPosition)
    {
        InventoryItem inventoryItem = new InventoryItem();
        int quantity = inventoryDict[itemPosition].itemQuantity - 1;
        if (quantity > 0)
        {
            inventoryItem.itemQuantity = quantity;
            inventoryItem.itemCode = itemCode;
        }
        else
        {
            inventoryItem.itemQuantity = 0;
            inventoryItem.itemCode = 0;
            sumItemInInventory -= 1;

            inventoryBar.ClearCurrentlySelectedItems();
        }

        inventoryDict[itemPosition] = inventoryItem;
    }

    /// <summary>
    /// Set the selected inventory item for inventoryLocation to itemCode
    /// </summary>
    public void SetSelectedInventoryItem(InventoryLocation inventoryLocation, int itemCode)
    {
        selectedInventoryItem[(int)inventoryLocation] = itemCode;
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public GameObjectSave ISaveableSave()
    {
        // Create new scene save
        SceneSave sceneSave = new SceneSave();

        // Remove any existing scene save for persistent scene for this gameobject
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        //add inv item dicts
        sceneSave.dictInvItemArray = inventoryDictionaries;

        // Add  inventory list capacity array to persistent scene save
        sceneSave.intArrayDictionary = new Dictionary<string, int[]>();
        sceneSave.intArrayDictionary.Add("inventoryListCapacityArray", inventoryListCapacityIntArray);

        // Add sumItemInInventory to persisten scene save
        sceneSave.intDictionary = new Dictionary<string, int>();
        sceneSave.intDictionary.Add("sumItemInInventory", sumItemInInventory);

        // Add scene save for gameobject
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }


    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Need to find inventory lists - start by trying to locate saveScene for game object
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // list inv items array exists for persistent scene
                if (sceneSave.dictInvItemArray != null)
                {
                    inventoryDictionaries = sceneSave.dictInvItemArray;

                    //  Send events that inventory has been updated
                    for (int i = 0; i < (int)InventoryLocation.count; i++)
                    {
                        EventHandler.CallInventoryUpdatedEvent((InventoryLocation)i, inventoryDictionaries[i]);
                    }

                    // Clear any items player was carrying
                    // Player.Instance.ClearCarriedItem();

                    // Clear any highlights on inventory bar
                    inventoryBar.ClearHighlightOnInventorySlots();
                }

                // int array dictionary exists for scene
                if (sceneSave.intArrayDictionary != null)
                {
                    if (sceneSave.intArrayDictionary.TryGetValue("inventoryListCapacityArray", out int[] inventoryCapacityArray))
                    {
                        inventoryListCapacityIntArray = inventoryCapacityArray;
                    }

                    if (sceneSave.intDictionary.TryGetValue("sumItemInInventory", out int currentSumItemInInventory))
                    {
                        sumItemInInventory = currentSumItemInInventory;
                    }
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required her since the inventory manager is on a persistent scene;
    }

    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since the inventory manager is on a persistent scene;
    }
}
