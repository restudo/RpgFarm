
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Camera mainCamera;
    private Canvas parentCanvas;
    private Transform parentItem;
    private GridCursor gridCursor;
    private Cursor cursor;
    public GameObject draggedItem;

    public Image inventorySlotHighlight;
    public Image inventorySlotImage;
    public TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private UIInventoryBar inventoryBar = null;

    // ui inventory slot text box
    // [SerializeField] private GameObject inventoryTextBoxPrefab = null;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public ItemDetails itemDetails;
    [SerializeField] private GameObject itemPrefab = null;
    [HideInInspector] public int itemQuantity;
    [SerializeField] private int slotNumber = 0;
    public int SlotNumber { get { return slotNumber; } }

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= SceneLoaded;
        EventHandler.RemoveSelectedItemFromInventoryEvent -= RemoveSelectedItemFromInventory;
        EventHandler.DropSelectedItemEvent -= DropSelectedItemAtMousePosition;
        EventHandler.InventorySlotSelectedKeyboardEvent -= InventorySelectedKeyboardEvent;
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += SceneLoaded;
        EventHandler.RemoveSelectedItemFromInventoryEvent += RemoveSelectedItemFromInventory;
        EventHandler.DropSelectedItemEvent += DropSelectedItemAtMousePosition;
        EventHandler.InventorySlotSelectedKeyboardEvent += InventorySelectedKeyboardEvent;
    }



    private void Start()
    {
        mainCamera = Camera.main;
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
    }

    private void ClearCursors()
    {
        // Disable cursor
        gridCursor.DisableCursor();
        cursor.DisableCursor();

        // Set item type to none
        gridCursor.SelectedItemType = ItemType.none;
        cursor.SelectedItemType = ItemType.none;
    }


    /// <summary>
    /// Sets this inventory slot item to be selected
    /// </summary>
    private void SetSelectedItem()
    {
        // Clear currently highlighted items
        inventoryBar.ClearHighlightOnInventorySlots();

        // Highlight item on inventory bar
        isSelected = true;

        // Set highlighted inventory slots
        inventoryBar.SetHighlightedInventorySlots();

        // Set use radius for cursors
        gridCursor.ItemUseGridRadius = itemDetails.itemUseGridRadius;
        cursor.ItemUseRadius = itemDetails.itemUseGridRadius;

        // If item requires a grid cursor then enable cursor
        if (itemDetails.itemUseGridRadius > 0)
        {
            gridCursor.EnableCursor();
        }
        else
        {
            gridCursor.DisableCursor();
        }

        if (itemDetails.itemUseRadius > 0)
        {
            cursor.EnableCursor();
        }
        else
        {
            cursor.DisableCursor();
        }

        // Set item type
        gridCursor.SelectedItemType = itemDetails.itemType;
        cursor.SelectedItemType = itemDetails.itemType;

        // // Set item selected in inventory
        InventoryManager.Instance.SetSelectedInventoryItem(InventoryLocation.player, itemDetails.itemCode);

        Player.Instance.itemSelectedCode = itemDetails.itemCode;
        Player.Instance.itemSelectedSlot = SlotNumber;

        // show carried item when those item can be carried
        if (itemDetails.canBeCarried == true)
        {
            Player.Instance.ShowCarriedItem(itemDetails.itemCode);

            if (itemDetails.canBeEaten == true)
            {
                Player.Instance.CanBeEaten = true;
            }
        }
        else
        {
            Player.Instance.ClearCarriedItem();
            Player.Instance.CanBeEaten = false;
        }
    }

    public void ClearSelectedItem()
    {
        ClearCursors();

        // Clear currently highlighted items
        inventoryBar.ClearHighlightOnInventorySlots();

        isSelected = false;

        // set no item selected in inventory
        InventoryManager.Instance.ClearSelectedInventoryItem(InventoryLocation.player);

        // player clear carried item 
        Player.Instance.ClearCarriedItem();
    }


    /// <summary>
    /// Drops the item (if selected) at the current mouse position.  Called by the DropItem event.
    /// </summary>
    private void DropSelectedItemAtMousePosition(Vector3 offset)
    {
        if (itemDetails != null && isSelected)
        {
            // If  a valid cursor position
            if (gridCursor.CursorPositionIsValid)
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCamera.transform.position.z));
                // Create item from prefab at mouse position
                GameObject itemGameObject = Instantiate(itemPrefab, new Vector3(worldPosition.x, worldPosition.y - Settings.gridCellSize / 2f, worldPosition.z) + offset, Quaternion.identity, parentItem);
                Item item = itemGameObject.GetComponent<Item>();
                item.ItemCode = itemDetails.itemCode;

                // Remove item from players inventory
                InventoryManager.Instance.RemoveItem(InventoryLocation.player, item.ItemCode, SlotNumber);

                // If no more of item then clear selected
                if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, item.ItemCode) == -1)
                {
                    ClearSelectedItem();
                }
            }

        }
    }

    private void RemoveSelectedItemFromInventory()
    {
        if (itemDetails != null && isSelected)
        {
            int itemCode = itemDetails.itemCode;

            // Remove item from players inventory
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemCode, SlotNumber);

            // If no more of item then clear selected
            if (InventoryManager.Instance.FindItemInInventory(InventoryLocation.player, itemCode) == -1)
            {
                ClearSelectedItem();
            }
        }
    }

    private void InventorySelectedKeyboardEvent(int slotSelected)
    {
        // if slot selected is this slot
        if (slotSelected == slotNumber)
        {
            // if inventory slot currently selected then deselect
            if (isSelected)
            {
                ClearSelectedItem();
            }
            else
            {
                if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemDetails != null)
        {
            // Disable keyboard input
            Player.Instance.DisablePlayerInputAndResetMovement();

            // Instatiate gameobject as dragged item
            draggedItem = Instantiate(inventoryBar.inventoryBarDraggedItem, inventoryBar.transform);

            // Get image for dragged item
            Image draggedItemImage = draggedItem.GetComponentInChildren<Image>();
            draggedItemImage.sprite = inventorySlotImage.sprite;

            SetSelectedItem();

        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // move game object as dragged item
        if (draggedItem != null)
        {
            draggedItem.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy game object as dragged item
        if (draggedItem != null)
        {
            Destroy(draggedItem);

            // If drag ends over inventory bar, get item drag is over and swap them
            if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>() != null)
            {
                // get the slot number where the drag ended
                int toSlotNumber = eventData.pointerCurrentRaycast.gameObject.GetComponent<UIInventorySlot>().slotNumber;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    int stackSize = Mathf.RoundToInt(itemQuantity / 2);
                    InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber, stackSize);
                }
                else
                {
                    // Swap inventory items in inventory list
                    InventoryManager.Instance.SwapInventoryItems(InventoryLocation.player, slotNumber, toSlotNumber);
                }


                // // Destroy inventory text box
                // DestroyInventoryTextBox();

                // Clear selected item
                ClearSelectedItem();

            }
            // else attempt to drop the item if it can be dropped
            else
            {
                if (itemDetails.canBeDropped)
                {
                    // quantity devided by 2
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        int stackSize = Mathf.RoundToInt(itemQuantity / 2);
                        for (int i = 0; i < stackSize; i++)
                        {
                            Vector3 offset = new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), 0);
                            DropSelectedItemAtMousePosition(offset);
                        }
                    }
                    // Drop a single item
                    else if (Input.GetKey(KeyCode.LeftControl))
                    {

                        Vector3 offset = new Vector3(0, 0, 0);
                        DropSelectedItemAtMousePosition(offset);
                    }
                    else
                    {
                        // Drop a full stack of items
                        int stackSize = itemQuantity;

                        // Store a temporary stackSize variable
                        for (int i = 0; i < stackSize; i++)
                        {
                            Vector3 offset = new Vector3(Random.Range(-2, 2f), Random.Range(-1, 1f), 0);
                            DropSelectedItemAtMousePosition(offset);
                        }
                    }
                }
            }

            // Enable player input
            Player.Instance.EnablePlayerInput();


        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // if left click
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // if inventory slot currently selected then deselect
            if (isSelected == true)
            {
                ClearSelectedItem();
            }
            else
            {
                if (itemQuantity > 0)
                {
                    SetSelectedItem();
                }
            }
        }
    }


    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     // Populate text box with item details
    //     if (itemQuantity != 0)
    //     {
    //         // Instantiate inventory text box
    //         inventoryBar.inventoryTextBoxGameobject = Instantiate(inventoryTextBoxPrefab, transform.position, Quaternion.identity);
    //         inventoryBar.inventoryTextBoxGameobject.transform.SetParent(parentCanvas.transform, false);

    //         UIInventoryTextBox inventoryTextBox = inventoryBar.inventoryTextBoxGameobject.GetComponent<UIInventoryTextBox>();

    //         // Set item type description
    //         string itemTypeDescription = InventoryManager.Instance.GetItemTypeDescription(itemDetails.itemType);

    //         // Populate text box
    //         inventoryTextBox.SetTextboxText(itemDetails.itemDescription, itemTypeDescription, "", itemDetails.itemLongDescription, "", "");

    //         // Set text box position according to inventory bar position
    //         if (inventoryBar.IsInventoryBarPositionBottom)

    //         {
    //             inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
    //             inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y + 50f, transform.position.z);
    //         }
    //         else
    //         {
    //             inventoryBar.inventoryTextBoxGameobject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
    //             inventoryBar.inventoryTextBoxGameobject.transform.position = new Vector3(transform.position.x, transform.position.y - 50f, transform.position.z);
    //         }
    //     }
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     DestroyInventoryTextBox();
    // }

    // public void DestroyInventoryTextBox()
    // {
    //     if (inventoryBar.inventoryTextBoxGameobject != null)
    //     {
    //         Destroy(inventoryBar.inventoryTextBoxGameobject);
    //     }
    // }

    public void SceneLoaded()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
    }

}

