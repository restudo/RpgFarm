using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class Player : SingletonMonobehaviour<Player>, ISaveable
{
    // animation tools
    private bool playerToolUseDisabled = false;
    private WaitForSeconds useToolAnimationPause;
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds pickAnimationPause;
    private WaitForSeconds afterPickAnimationPause;

    private UIInventorySlot uIInventorySlot;
    private GridCursor gridCursor;
    private Cursor cursor;
    private Vector3 offset = new Vector3(0, 0, 0);

    // picking condition for harvest with basket
    private bool isUsingToolRight = false;
    private bool isPickingRight = false;

    [HideInInspector] public bool instantiateCrop = true;
    [HideInInspector] public bool playerIsOnTheBed = false;
    private bool facingRight;
    private Animator anim;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float xInput, yInput;

    private bool _canBeEaten = false;
    public bool CanBeEaten { get => _canBeEaten; set => _canBeEaten = value; }

    private bool _playerInputIsDisabled = false;
    public bool playerInputIsDisabled { get => _playerInputIsDisabled; set => _playerInputIsDisabled = value; }

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    // animation
    private int isWalking;
    private int isUsingHoe;
    private int isUsingAxe;
    private int isUsingPickaxe;
    private int isUsingScythe;
    private int isUsingWateringCan;
    private int isHarvesting;
    private int isStaminaZero;

    public int itemSelectedCode;
    public int itemSelectedSlot;


    // camera
    private Camera mainCamera;


    [Header("Item Selected"), Tooltip("Should be populated in the prefab with the equipped item sprite renderer")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null;

    [Header("MoveController")]
    [SerializeField] private float moveSpeed;

    [Header("Stamina & Sleep")]
    public int _stamina = 100;
    [SerializeField] private Vector3 scenePositionGoto = new Vector3();
    private SceneName sceneNameGoto = SceneName.Scene3_Cabin;
    public int Stamina { get => _stamina; set => _stamina = value; }
    private int defaultStamina;
    public int DefaultStamina { get => defaultStamina; set => defaultStamina = value; }

    [Header("Water Quantity")]
    [SerializeField] private int _waterQuantity = 50;
    public int WaterQuantity { get => _waterQuantity; set => _waterQuantity = value; }
    private bool isFillWater = false;

    protected override void Awake()
    {
        base.Awake();

        // Get unique ID for gameobject and create save data object
        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;

        GameObjectSave = new GameObjectSave();
    }

    private void OnDisable()
    {
        ISaveableDeregister();

        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;
        Lua.UnregisterFunction("DisablePlayerInput");
        Lua.UnregisterFunction("EnablePlayerInput");
        Lua.UnregisterFunction("PlayerSleep");
    }


    private void OnEnable()
    {
        ISaveableRegister();

        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
        Lua.RegisterFunction("DisablePlayerInput", this, SymbolExtensions.GetMethodInfo(() => DisablePlayerInput()));
        Lua.RegisterFunction("EnablePlayerInput", this, SymbolExtensions.GetMethodInfo(() => EnablePlayerInput()));
        Lua.RegisterFunction("PlayerSleep", this, SymbolExtensions.GetMethodInfo(() => PlayerSleep()));
    }

    void Start()
    {
        facingRight = true;
        defaultStamina = Stamina;
        mainCamera = Camera.main;

        isWalking = Animator.StringToHash("isWalking");
        isUsingHoe = Animator.StringToHash("isUsingHoe");
        isUsingAxe = Animator.StringToHash("isUsingAxe");
        isUsingPickaxe = Animator.StringToHash("isUsingPickaxe");
        isHarvesting = Animator.StringToHash("isHarvesting");
        isUsingScythe = Animator.StringToHash("isUsingScythe");
        isUsingWateringCan = Animator.StringToHash("isUsingWateringCan");
        isStaminaZero = Animator.StringToHash("isStaminaZero");

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
        uIInventorySlot = FindObjectOfType<UIInventorySlot>();

        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        pickAnimationPause = new WaitForSeconds(Settings.pickAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    void Update()
    {
        if (!playerInputIsDisabled)
        {
            PlayerMovementInput();

            PlayerTestInput();

            PlayerClickInput();

            PlayerInventorySlotKeyboardSelection();
        }
    }

    void FixedUpdate() // physics
    {
        if (!playerInputIsDisabled)
        {

            PlayerMovement();
        }
    }

    void PlayerMovementInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(xInput, yInput).normalized;

        if (xInput != 0 || yInput != 0)
        {
            anim.SetBool(isWalking, true);
        }
        else
        {
            anim.SetBool(isWalking, false);
        }

        if (xInput < 0 && facingRight)
        {
            Flip();
        }
        else if (xInput > 0 && !facingRight)
        {
            Flip();
        }
    }

    void PlayerMovement()
    {
        rb.MovePosition(rb.position + (moveDirection * moveSpeed * Time.fixedDeltaTime));
    }

    private void PlayerClickInput()
    {
        if (!playerToolUseDisabled)
        {
            if (Input.GetMouseButton(0))
            {

                if (gridCursor.CursorIsEnabled || cursor.CursorIsEnabled)
                {
                    // Get Cursor Grid Position
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();

                    // Get Player Grid Position
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();

                    ProcessPlayerClickInput(cursorGridPosition, playerGridPosition);
                }
            }

            // Trigger sleep
            if (playerIsOnTheBed && Input.GetKeyDown(KeyCode.E))
            {
                DisablePlayerInputAndResetMovement();
                UIManager.Instance.OpenSleepUI();
            }

            // fill watering can in well
            if (isFillWater && Input.GetKeyDown(KeyCode.E))
            {
                // Get Selected item details
                ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

                if (itemDetails != null && itemDetails.itemType == ItemType.Watering_tool)
                {
                    // TODO: set animation

                    WaterQuantity = 50;
                }
            }

            if (Input.GetKeyDown(KeyCode.E) && CanBeEaten == true)
            {
                // TODO: set animation

                if (itemSelectedCode == 10001) // pumpkin
                {
                    #region StartCoroutine Eating Animation
                    Stamina += 2;
                    if (Stamina >= defaultStamina)
                    {
                        Stamina = defaultStamina;
                    }
                    StaminaController.Instance.UpdateStamina(Stamina);
                    Debug.Log("Makan kentang, singkong, tomat, labu " + Stamina);

                    InventoryManager.Instance.RemoveItem(InventoryLocation.player, itemSelectedCode, itemSelectedSlot);
                    #endregion
                }
                // else if (itemSelectedCode == BlaBlaBla....)
            }
        }
    }

    /// <summary>
    /// Detect Player Keyboard Input to select inventory Slot 
    /// </summary>
    private void PlayerInventorySlotKeyboardSelection()
    {
        string numSelected;
        switch (Input.inputString)
        {
            case "1":
                numSelected = "0";
                break;

            case "2":
                numSelected = "1";
                break;

            case "3":
                numSelected = "2";
                break;

            case "4":
                numSelected = "3";
                break;

            case "5":
                numSelected = "4";
                break;

            case "6":
                numSelected = "5";
                break;

            case "7":
                numSelected = "6";
                break;

            case "8":
                numSelected = "7";
                break;

            default:
                numSelected = "";
                break;
        }

        if (numSelected != "")
        {
            EventHandler.CallInventorySlotSelectedKeyboardEvent(int.Parse(numSelected));
        }
    }

    public void PlayerSleep()
    {
        playerInputIsDisabled = true;
        instantiateCrop = false;

        //  Calculate players new position
        float xPosition = Mathf.Approximately(scenePositionGoto.x, 0f) ? gameObject.transform.position.x : scenePositionGoto.x;
        float yPosition = Mathf.Approximately(scenePositionGoto.y, 0f) ? gameObject.transform.position.y : scenePositionGoto.y;
        float zPosition = 0f;

        // Teleport to new scene
        SceneControllerManager.Instance.FadeAndLoadScene(sceneNameGoto.ToString(), new Vector3(xPosition, yPosition, zPosition));

        if (TimeManager.Instance.GameHour >= 0 && TimeManager.Instance.GameHour <= 4 && Stamina <= 10)
        {
            // set Default stamina to 70 because of penalty
            DefaultStamina = Settings.playerMaxPenaltyStamina;
            // set stamina to 50 because of penalty
            Stamina = Settings.playerInitialPenaltyStamina;

            TimeManager.Instance.TestAdvancePenaltyGameDay();
            StaminaController.Instance.UpdateStamina(Stamina);
        }
        else if (TimeManager.Instance.GameHour >= 0 && TimeManager.Instance.GameHour <= 4 && Stamina > 10)
        {
            // set stamina back to default
            DefaultStamina = Settings.playerInitialDefaultStamina;
            Stamina = DefaultStamina;

            // set time to penalty at 8 oclock
            TimeManager.Instance.TestAdvanceNormalPenaltyGameDay();
            StaminaController.Instance.UpdateStamina(Stamina);
        }
        else
        {
            // set stamina back to default
            DefaultStamina = Settings.playerInitialDefaultStamina;
            Stamina = DefaultStamina;

            TimeManager.Instance.TestAdvanceNormalGameDay();
            StaminaController.Instance.UpdateStamina(Stamina);
        }

        EventHandler.CallAdvanceGameDayEvent(TimeManager.Instance.GameYear, TimeManager.Instance.GameSeason, TimeManager.Instance.GameDay, TimeManager.Instance.GameDayOfWeek, TimeManager.Instance.GameHour, TimeManager.Instance.GameMinute, TimeManager.Instance.GameSecond);

        playerIsOnTheBed = false;

        if (TimeManager.Instance.GameDay == 10 || TimeManager.Instance.GameDay == 20 || TimeManager.Instance.GameDay == 30)
        {
            instantiateCrop = true;
        }
        // playerInputIsDisabled = false;
    }

    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        ResetMovement();
        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition, playerGridPosition);

        // Get Grid property details at cursor position (the GridCursor validation routine ensures that grid property details are not null)
        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x, cursorGridPosition.y);

        // Get Selected item details
        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if (itemDetails != null)
        {
            switch (itemDetails.itemType)
            {
                case ItemType.Seed:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(gridPropertyDetails, itemDetails, playerDirection);
                    }
                    break;

                case ItemType.Commodity:
                    if (Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;

                case ItemType.Hoeing_tool:
                case ItemType.Watering_tool:
                case ItemType.Chopping_tool:
                case ItemType.Collecting_tool:
                case ItemType.Breaking_tool:
                case ItemType.Reaping_tool:
                    if (Stamina == 50 && gridCursor.CursorPositionIsValid || cursor.CursorPositionIsValid)
                    {
                        // TODO: Change animation
                        // Stamina 50 animation
                        StartCoroutine(StaminaFifty(playerDirection, gridPropertyDetails));
                        Debug.Log("Stamina Fifty");
                    }
                    else if (Stamina <= 0 && gridCursor.CursorPositionIsValid || cursor.CursorPositionIsValid)
                    {
                        // Stamina Zero animation
                        StartCoroutine(StaminaZero(playerDirection, gridPropertyDetails));
                        Debug.Log("Stamina abis");
                    }

                    if (Stamina > 0)
                    {
                        ProcessPlayerClickInputTool(gridPropertyDetails, itemDetails, playerDirection);
                    }
                    break;

                case ItemType.none:
                    break;

                case ItemType.count:
                    break;

                default:
                    break;
            }

        }
    }

    private IEnumerator StaminaZero(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        // TODO: set animation

        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        if (playerDirection == Vector3Int.right)
        {
            if (!facingRight)
            {
                Flip();
            }
            // anim.SetBool(isStaminaZero, true);
        }
        else if (playerDirection == Vector3Int.left)
        {
            if (facingRight)
            {
                Flip();
            }
            // anim.SetBool(isStaminaZero, true);
        }

        yield return useToolAnimationPause;

        // After animation pause
        yield return afterUseToolAnimationPause;

        // anim.SetBool(isStaminaZero, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private IEnumerator StaminaFifty(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        // TODO: set animation

        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        if (playerDirection == Vector3Int.right)
        {
            if (!facingRight)
            {
                Flip();
            }
            // anim.SetBool(isStaminaZero, true);
        }
        else if (playerDirection == Vector3Int.left)
        {
            if (facingRight)
            {
                Flip();
            }
            // anim.SetBool(isStaminaZero, true);
        }

        yield return useToolAnimationPause;

        // After animation pause
        yield return afterUseToolAnimationPause;

        // anim.SetBool(isStaminaZero, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition, Vector3Int playerGridPosition)
    {
        if (cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }
        else
        {
            return Vector3Int.left;
        }
    }

    private Vector3Int GetPlayerDirection(Vector3 cursorPosition, Vector3 playerPosition)
    {
        if (
            cursorPosition.x > playerPosition.x
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.right;
        }
        else if (
            cursorPosition.x < playerPosition.x
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.left;
        }
        else if (
            cursorPosition.y > playerPosition.y
            &&
            cursorPosition.x > playerPosition.x
            )
        {
            return Vector3Int.right;
        }
        else if (
            cursorPosition.y > playerPosition.y
            &&
            cursorPosition.x < playerPosition.x
            )
        {
            return Vector3Int.left;
        }
        else if (
            cursorPosition.y < playerPosition.y
            &&
            cursorPosition.x > playerPosition.x
            )
        {
            return Vector3Int.right;
        }
        else
        {
            return Vector3Int.left;
        }
    }

    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            if (Stamina == 50)
            {
                // TODO: Change animation
                // Stamina 50 animation
                StartCoroutine(StaminaFifty(playerDirection, gridPropertyDetails));
                Debug.Log("Stamina Fifty");
            }
            else if (Stamina <= 0)
            {
                // Stamina Zero animation
                StartCoroutine(StaminaZero(playerDirection, gridPropertyDetails));
                Debug.Log("Stamina abis");
            }
            else if (Stamina > 0)
            {
                PlantSeedAtCursor(gridPropertyDetails, itemDetails);
            }
        }
        else if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent(offset);
        }
    }

    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        // Process if we have cropdetails for the seed
        if (GridPropertiesManager.Instance.GetCropDetails(itemDetails.itemCode) != null)
        {
            // Decrease the stamina
            Stamina -= 2;
            StaminaController.Instance.UpdateStamina(Stamina);

            // Update grid properties with seed details
            gridPropertyDetails.seedItemCode = itemDetails.itemCode;
            gridPropertyDetails.growthDays = 0;

            // Display planted crop at grid property details
            GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

            // Remove item from inventory
            EventHandler.CallRemoveSelectedItemFromInventoryEvent();

        }

    }

    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
        if (itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent(offset);
        }
    }

    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails, Vector3Int playerDirection)
    {
        // Switch on tool
        switch (itemDetails.itemType)
        {
            case ItemType.Hoeing_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    // Decrease the stamina
                    Stamina -= 2;
                    StaminaController.Instance.UpdateStamina(Stamina);

                    HoeGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Watering_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    // Decrease the stamina
                    Stamina -= 1;
                    StaminaController.Instance.UpdateStamina(Stamina);

                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Chopping_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    // Decrease the stamina
                    Stamina -= 2;
                    StaminaController.Instance.UpdateStamina(Stamina);

                    ChopInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Collecting_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    Stamina -= 1;
                    StaminaController.Instance.UpdateStamina(Stamina);

                    CollectInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Breaking_tool:
                if (gridCursor.CursorPositionIsValid)
                {
                    // Decrease the stamina
                    Stamina -= 2;
                    StaminaController.Instance.UpdateStamina(Stamina);

                    BreakInPlayerDirection(gridPropertyDetails, itemDetails, playerDirection);
                }
                break;

            case ItemType.Reaping_tool:
                if (cursor.CursorPositionIsValid)
                {
                    // Decrease the stamina
                    Stamina -= 1;
                    StaminaController.Instance.UpdateStamina(Stamina);

                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(), GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails, playerDirection);
                }
                break;

            default:
                break;
        }
    }

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation
        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        if (playerDirection == Vector3Int.right)
        {
            if (!facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingHoe, true);
        }
        else if (playerDirection == Vector3Int.left)
        {
            if (facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingHoe, true);
        }

        yield return useToolAnimationPause;

        // Set Grid property details for dug ground
        if (gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        // Set grid property to dug
        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

        // Display dug grid tiles
        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);

        // After animation pause
        yield return afterUseToolAnimationPause;

        anim.SetBool(isUsingHoe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        // Trigger animation
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection, gridPropertyDetails));
    }

    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection, GridPropertyDetails gridPropertyDetails)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // TODO: Change the animation to watering animation
        if (playerDirection == Vector3Int.right)
        {
            if (!facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingWateringCan, true);
        }
        else if (playerDirection == Vector3Int.left)
        {
            if (facingRight)
            {
                Flip();
            }
            anim.SetBool(isUsingWateringCan, true);
        }

        yield return liftToolAnimationPause;

        if (WaterQuantity > 0)
        {
            // TODO: If there is water in the watering can
            // toolEffect = ToolEffect.watering;

            // Set Grid property details for watered ground
            if (gridPropertyDetails.daysSinceWatered == -1)
            {
                WaterQuantity -= 1;

                gridPropertyDetails.daysSinceWatered = 0;
            }

            // Set grid property to watered
            GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);

            // Display watered grid tiles
            GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);
        }

        // After animation pause
        yield return afterLiftToolAnimationPause;

        // TODO: Change the animation to Watering animation
        anim.SetBool(isUsingWateringCan, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ChopInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        // Trigger animation
        StartCoroutine(ChopInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator ChopInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // TODO: Change the animation to Chopping animation
        anim.SetBool(isUsingAxe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {

        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }


    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return pickAnimationPause;

        // TODO: Change the animation to Collecting animation
        anim.SetBool(isHarvesting, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void BreakInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {

        StartCoroutine(BreakInPlayerDirectionRoutine(gridPropertyDetails, equippedItemDetails, playerDirection));
    }

    private IEnumerator BreakInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        ProcessCropWithEquippedItemInPlayerDirection(playerDirection, equippedItemDetails, gridPropertyDetails);

        yield return useToolAnimationPause;

        // TODO: Change the animation to Breaking animation
        anim.SetBool(isUsingPickaxe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails, playerDirection));
    }

    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        playerInputIsDisabled = true;
        playerToolUseDisabled = true;

        // Reap in player direction
        UseToolInPlayerDirection(itemDetails, playerDirection);

        yield return useToolAnimationPause;

        yield return afterPickAnimationPause;

        // TODO: Change the animation to Reaping animation
        anim.SetBool(isUsingScythe, false);

        playerInputIsDisabled = false;
        playerToolUseDisabled = false;
    }

    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails, Vector3Int playerDirection)
    {
        if (Input.GetMouseButton(0))
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Reaping_tool:
                    // TODO: Change the animation to Reaping animation
                    if (playerDirection == Vector3Int.right)
                    {
                        if (!facingRight)
                        {
                            Flip();
                        }
                        anim.SetBool(isUsingScythe, true);
                    }
                    else if (playerDirection == Vector3Int.left)
                    {
                        if (facingRight)
                        {
                            Flip();
                        }
                        anim.SetBool(isUsingScythe, true);
                    }
                    break;
            }

            // Define centre point of square which will be used for collision testing
            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f)), GetPlayerCentrePosition().y + playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));

            // Define size of the square which will be used for collision testing
            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);

            // Get Item components with 2D collider located in the square at the centre point defined (2d colliders tested limited to maxCollidersToTestPerReapSwing)
            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing, point, size, 0f);

            int reapableItemCount = 0;

            // Loop through all items retrieved
            for (int i = itemArray.Length - 1; i >= 0; i--)
            {
                if (itemArray[i] != null)
                {
                    // Destroy item game object if reapable
                    if (InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenary)
                    {
                        // Effect position
                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x, itemArray[i].transform.position.y + Settings.gridCellSize / 2f, itemArray[i].transform.position.z);

                        // Trigger reaping effect
                        EventHandler.CallHarvestActionEffectEvent(effectPosition, HarvestActionEffect.reaping);

                        Destroy(itemArray[i].gameObject);

                        reapableItemCount++;
                        if (reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Method processes crop with equipped item in player direction
    /// </summary>
    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection, ItemDetails equippedItemDetails, GridPropertyDetails gridPropertyDetails)
    {
        switch (equippedItemDetails.itemType)
        {
            case ItemType.Breaking_tool:
                // TODO: Change the animation to Breaking animation
                if (playerDirection == Vector3Int.right)
                {
                    isUsingToolRight = true;

                    if (!facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isUsingPickaxe, true);
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isUsingToolRight = false;

                    if (facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isUsingPickaxe, true);
                }
                break;

            case ItemType.Chopping_tool:
                // TODO: Change the animation to Chopping animation
                if (playerDirection == Vector3Int.right)
                {
                    isUsingToolRight = true;

                    if (!facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isUsingAxe, true);
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isUsingToolRight = false;

                    if (facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isUsingAxe, true);
                }
                break;

            case ItemType.Collecting_tool:
                // TODO: Change the animation to Collection animation
                if (playerDirection == Vector3Int.right)
                {
                    isPickingRight = true;

                    if (!facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isHarvesting, true);
                }
                else if (playerDirection == Vector3Int.left)
                {
                    isPickingRight = false;

                    if (facingRight)
                    {
                        Flip();
                    }
                    anim.SetBool(isHarvesting, true);
                }
                break;

            case ItemType.none:
                break;
        }

        // Get crop at cursor grid location
        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        // Execute Process Tool Action For crop
        if (crop != null)
        {
            switch (equippedItemDetails.itemType)
            {
                case ItemType.Chopping_tool:
                case ItemType.Breaking_tool:
                    crop.ProcessToolAction(equippedItemDetails, isUsingToolRight);
                    break;

                case ItemType.Collecting_tool:
                    crop.ProcessToolAction(equippedItemDetails, isPickingRight);
                    break;
            }
        }
    }

    // TODO: Remove
    /// <summary>
    /// Temp routine for test input
    /// </summary>
    private void PlayerTestInput()
    {
        // Trigger Advance Time
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        // Trigger Advance Day
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (TimeManager.Instance.GameHour >= 0 && TimeManager.Instance.GameHour <= 4)
            {
                // set Default stamina to 70 because of penalty
                DefaultStamina = Settings.playerMaxPenaltyStamina;
                // set stamina to 50 because of penalty
                Stamina = Settings.playerInitialPenaltyStamina;

                TimeManager.Instance.TestAdvancePenaltyGameDay();
            }
            else
            {
                // set stamina back to default
                DefaultStamina = Settings.playerInitialDefaultStamina;
                Stamina = DefaultStamina;

                TimeManager.Instance.TestAdvanceNormalGameDay();
            }

        }

        // TODO: Add Eating Mechanics
        // Debug test for stamina
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Stamina += 20;
            if (Stamina >= defaultStamina)
            {
                Stamina = defaultStamina;
            }
            StaminaController.Instance.UpdateStamina(Stamina);
            Debug.Log("Makan kentang, singkong, tomat, labu " + Stamina);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            Stamina += 40;
            if (Stamina >= defaultStamina)
            {
                Stamina = defaultStamina;
            }
            StaminaController.Instance.UpdateStamina(Stamina);
            Debug.Log("Makan Tanaman Herbal " + Stamina);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            Stamina += 25;
            if (Stamina >= defaultStamina)
            {
                Stamina = defaultStamina;
            }
            StaminaController.Instance.UpdateStamina(Stamina);
            Debug.Log("Makan Jahe " + Stamina);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (defaultStamina < Settings.playerMaxDefaultStamina)
            {
                DefaultStamina += 2;
            }
            StaminaController.Instance.IncraseMaxStamina(DefaultStamina);
            Debug.Log("Makan Tanaman Herbal, default stamina bertambah" + defaultStamina + ", namun stamina tidak" + Stamina);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(Stamina);
            Debug.Log("Default Stamina " + defaultStamina);
        }
        if (Input.GetKeyDown(KeyCode.L)) // if the weather is raining
        {
            Stamina -= 2 + (2 * 50 / 100);
            StaminaController.Instance.UpdateStamina(Stamina);
            Debug.Log(Stamina);
        }
        if (Input.GetKeyDown(KeyCode.K)) // if the weather is raining
        {
            Stamina -= 1 + (1 * 50 / 100);
            StaminaController.Instance.UpdateStamina(Stamina);
            Debug.Log(Stamina);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Stamina -= 50;
            StaminaController.Instance.UpdateStamina(Stamina);
        }
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();
    }

    void ResetMovement()
    {
        rb.MovePosition(rb.position);
        anim.SetBool(isWalking, false);
    }

    public void DisablePlayerInput()
    {
        playerInputIsDisabled = true;
    }
    public void EnablePlayerInput()
    {
        playerInputIsDisabled = false;
    }

    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
    }

    public void ShowCarriedItem(int itemCode)
    {
        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        if (itemDetails != null)
        {
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public Vector3 GetPlayerViewPortPosition()
    {
        // Vector3 viewport position for player (0,0) viewport bottom left, (1,1) viewport top right
        return mainCamera.WorldToViewportPoint(transform.position);
    }

    public Vector3 GetPlayerCentrePosition()
    {
        return new Vector3(transform.position.x, transform.position.y + Settings.playerCentreYOffset, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case Tags.Bed:
                playerIsOnTheBed = true;
                break;

            case Tags.Well:
                isFillWater = true;
                break;

            default:
                break;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case Tags.Bed:
                playerIsOnTheBed = false;
                break;

            case Tags.Well:
                isFillWater = false;
                break;

            default:
                break;
        }
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
        // Delete saveScene for game object if it already exists
        GameObjectSave.sceneData.Remove(Settings.PersistentScene);

        // Create saveScene for game object
        SceneSave sceneSave = new SceneSave();

        // Create Vector3 Dictionary
        sceneSave.vector3Dictionary = new Dictionary<string, Vector3Serializable>();

        //  Create String Dictionary
        sceneSave.stringDictionary = new Dictionary<string, string>();

        // Create int Dictionary
        sceneSave.intDictionary = new Dictionary<string, int>();

        // Add Player position to Vector3 dictionary
        Vector3Serializable vector3Serializable = new Vector3Serializable(transform.position.x, transform.position.y, transform.position.z);
        sceneSave.vector3Dictionary.Add("playerPosition", vector3Serializable);

        // Add Current Scene Name to string dictionary
        sceneSave.stringDictionary.Add("currentScene", SceneManager.GetActiveScene().name);

        // Add stamina to int Dictionary
        sceneSave.intDictionary.Add("stamina", Stamina);

        // Add water quantity to int dictionary
        sceneSave.intDictionary.Add("waterQuantity", WaterQuantity);

        // Add Player Direction to string dictionary
        // sceneSave.stringDictionary.Add("playerDirection", playerDirection.ToString());

        // Add sceneSave data for player game object
        GameObjectSave.sceneData.Add(Settings.PersistentScene, sceneSave);

        return GameObjectSave;
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            // Get save data dictionary for scene
            if (gameObjectSave.sceneData.TryGetValue(Settings.PersistentScene, out SceneSave sceneSave))
            {
                // Get player position
                if (sceneSave.vector3Dictionary != null && sceneSave.vector3Dictionary.TryGetValue("playerPosition", out Vector3Serializable playerPosition))
                {
                    transform.position = new Vector3(playerPosition.x, playerPosition.y, playerPosition.z);
                }

                // Get String dictionary
                if (sceneSave.stringDictionary != null)
                {
                    // Get player scene
                    if (sceneSave.stringDictionary.TryGetValue("currentScene", out string currentScene))
                    {
                        SceneControllerManager.Instance.FadeAndLoadScene(currentScene, transform.position);
                    }
                }

                // Get int Dictionary
                if (sceneSave.intDictionary != null)
                {
                    // Get Player Stamina
                    if (sceneSave.intDictionary.TryGetValue("stamina", out int currentStamina))
                    {
                        Stamina = currentStamina;
                    }

                    // Get Player Water Quantity
                    if (sceneSave.intDictionary.TryGetValue("waterQuantity", out int currentWaterQuantity))
                    {
                        WaterQuantity = currentWaterQuantity;
                    }
                }
            }
        }
    }

    public void ISaveableStoreScene(string sceneName)
    {
        // Nothing required here since the player is on a persistent scene;
    }


    public void ISaveableRestoreScene(string sceneName)
    {
        // Nothing required here since the player is on a persistent scene;
    }
}
