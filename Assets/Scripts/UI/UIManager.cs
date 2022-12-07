using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonobehaviour<UIManager>
{
    private bool _pauseMenuOn = false;
    private bool _chestMenuOn = false;
    [SerializeField] private UIInventoryBar uiInventoryBar = null;
    [SerializeField] private PauseMenuInventoryManagement pauseMenuInventoryManagement = null;
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private GameObject[] menuTabs = null;
    [SerializeField] private Button[] menuButtons = null;

    [SerializeField] private GameObject chestMenu = null;


    public bool PauseMenuOn { get => _pauseMenuOn; set => _pauseMenuOn = value; }
    public bool ChestMenuOn { get => _chestMenuOn; set => _chestMenuOn = value; }

    protected override void Awake()
    {
        base.Awake();

        pauseMenu.SetActive(false);
        chestMenu.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        PauseMenu();

        ChestMenu();
    }

    private void PauseMenu()
    {
        // Toggle pause menu if escape is pressed

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!ChestMenuOn)
            {
                if (PauseMenuOn)
                {
                    DisablePauseMenu();
                }
                else
                {
                    EnablePauseMenu();
                }
            }
        }
    }

    private void ChestMenu()
    {
        // Open Chest
        if (Player.Instance.IsTriggerWithChest && Input.GetKeyDown(KeyCode.E))
        {
            if (!PauseMenuOn)
            {
                if (ChestMenuOn)
                {
                    CLoseChest();
                }
                else
                {
                    OpenChest();
                }
            }
        }
    }

    private void EnablePauseMenu()
    {
        // Destroy any currently dragged items
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        uiInventoryBar.ClearCurrentlySelectedItems();

        PauseMenuOn = true;
        Player.Instance.playerInputIsDisabled = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);

        // Trigger garbage collector
        System.GC.Collect();

        // Highlight selected button
        HighlightButtonForSelectedTab();
    }

    public void DisablePauseMenu()
    {
        // Destroy any currently dragged items
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        PauseMenuOn = false;
        Player.Instance.playerInputIsDisabled = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);

    }

    private void OpenChest()
    {
        // Destroy any currently dragged items
        uiInventoryBar.DestroyCurrentlyDraggedItems();

        // Clear currently selected items
        uiInventoryBar.ClearCurrentlySelectedItems();

        ChestMenuOn = true;
        Player.Instance.playerInputIsDisabled = true;
        chestMenu.SetActive(true);

    }

    private void CLoseChest()
    {
        // TODO: change with chest inventory management
        // Destroy any currently dragged items
        pauseMenuInventoryManagement.DestroyCurrentlyDraggedItems();

        ChestMenuOn = false;
        Player.Instance.playerInputIsDisabled = false;
        chestMenu.SetActive(false);

    }

    private void HighlightButtonForSelectedTab()
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (menuTabs[i].activeSelf)
            {
                SetButtonColorToActive(menuButtons[i]);
            }

            else
            {
                SetButtonColorToInactive(menuButtons[i]);
            }
        }
    }

    private void SetButtonColorToActive(Button button)
    {
        ColorBlock colors = button.colors;

        colors.normalColor = colors.pressedColor;

        button.colors = colors;

    }

    private void SetButtonColorToInactive(Button button)
    {
        ColorBlock colors = button.colors;

        colors.normalColor = colors.disabledColor;

        button.colors = colors;

    }

    public void SwitchPauseMenuTab(int tabNum)
    {
        for (int i = 0; i < menuTabs.Length; i++)
        {
            if (i != tabNum)
            {
                menuTabs[i].SetActive(false);
            }
            else
            {
                menuTabs[i].SetActive(true);

            }
        }

        HighlightButtonForSelectedTab();

    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
