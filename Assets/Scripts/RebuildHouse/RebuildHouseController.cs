using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class RebuildHouseController : MonoBehaviour
{
    private GameObject fader;
    private Animator faderAnim;
    private int isFade;

    [Header("House01")]
    [SerializeField] private GameObject repairedHouse01;
    [SerializeField] private Transform spawnHouse01;
    private Transform parentItem01;

    [Header("House01")]
    [SerializeField] private GameObject repairedHouse02;
    [SerializeField] private Transform spawnHouse02;
    private Transform parentItem02;

    private void Start()
    {
        parentItem01 = GameObject.FindGameObjectWithTag(Tags.House01).transform;
        parentItem02 = GameObject.FindGameObjectWithTag(Tags.House02).transform;

        fader = GameObject.FindGameObjectWithTag(Tags.Fader);
        faderAnim = fader.GetComponent<Animator>();
    }

    private void OnDisable()
    {
        // Lua.UnregisterFunction("RebuildHouse01");
        // Lua.UnregisterFunction("RebuildHouse02");
        Lua.UnregisterFunction("RebuildHouse");
        Lua.UnregisterFunction("StaminaQtyCheck");
        Lua.UnregisterFunction("MaterialWoodStone");
        Lua.UnregisterFunction("DecreaseMaterial");
    }

    private void OnEnable()
    {
        Lua.RegisterFunction("RebuildHouse", this, SymbolExtensions.GetMethodInfo(() => RebuildHouse((double)0)));
        Lua.RegisterFunction("StaminaQtyCheck", this, SymbolExtensions.GetMethodInfo(() => StaminaQtyCheck((double)0)));
        Lua.RegisterFunction("MaterialWoodStone", this, SymbolExtensions.GetMethodInfo(() => MaterialWoodStone((double)0, (double)0)));
        Lua.RegisterFunction("DecreaseMaterial", this, SymbolExtensions.GetMethodInfo(() => DecreaseMaterial((double)0, (double)0, (double)0)));
        // Lua.RegisterFunction("RebuildHouse01", this, SymbolExtensions.GetMethodInfo(() => RebuildHouse01()));
        // Lua.RegisterFunction("RebuildHouse02", this, SymbolExtensions.GetMethodInfo(() => RebuildHouse02()));
    }

    public void RebuildHouse(double houseIndex)
    {
        faderAnim.SetBool("isFade", true);

        StartCoroutine(Rebuild(houseIndex, 2f));
    }

    IEnumerator Rebuild(double houseIndex, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        DestroyBrokenHouses((int)houseIndex);

        switch (houseIndex)
        {
            case 1:
                Instantiate(repairedHouse01, spawnHouse01.position, Quaternion.identity, parentItem01);

                HouseManager.Instance.numOfRebuildHouse = 1;
                break;

            case 2:
                Instantiate(repairedHouse02, spawnHouse02.position, Quaternion.identity, parentItem02);

                HouseManager.Instance.numOfRebuildHouse = 2;
                break;

            default:
                break;
        }

        HouseManager.Instance.isPlayTimeline = true;
        faderAnim.SetBool("isFade", false);

        Invoke("Conversation", 1f);
    }

    public void DestroyBrokenHouses(int houseNumber)
    {
        House[] house = FindObjectsOfType<House>();

        for (int i = 0; i < house.Length; i++)
        {
            if (houseNumber == house[i].HouseCode && house[i].IsHouseRepaired01 == false)
            {
                Destroy(house[i].gameObject);
            }
            if (houseNumber == house[i].HouseCode && house[i].IsHouseRepaired02 == false)
            {
                Destroy(house[i].gameObject);
            }
        }
    }
    private void Conversation()
    {
        DialogueManager.StartConversation("HouseRepaired");
    }

    public bool MaterialWoodStone(double woodQty, double stoneQty)
    {
        // ItemDetails wood = InventoryManager.Instance.GetItemDetails(10008);
        // ItemDetails stone = InventoryManager.Instance.GetItemDetails(10015);

        int wood = InventoryManager.Instance.FindWoodInInventory(InventoryLocation.player, 10008);
        int stone = InventoryManager.Instance.FindStoneInInventory(InventoryLocation.player, 10015);

        if (stone >= stoneQty && wood >= woodQty)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool StaminaQtyCheck(double staminaQty)
    {
        if (Player.Instance.Stamina >= staminaQty)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DecreaseMaterial(double woodQty, double stoneQty, double staminaQty)
    {
        for (int i = (int)woodQty; i > 0; i--)
        {
            int itemWoodPosition = InventoryManager.Instance.FindMaterialInInventory(InventoryLocation.player, 10008);
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, 10008, itemWoodPosition);
        }

        for (int i = (int)stoneQty; i > 0; i--)
        {
            int itemStonePosition = InventoryManager.Instance.FindMaterialInInventory(InventoryLocation.player, 10015);
            InventoryManager.Instance.RemoveItem(InventoryLocation.player, 10015, itemStonePosition);
        }

        //  Send event that inventory has been updated
        EventHandler.CallInventoryUpdatedEvent(InventoryLocation.player, InventoryManager.Instance.inventoryDictionaries[(int)InventoryLocation.player]);

        Player.Instance.Stamina -= (int)staminaQty;
        StaminaController.Instance.UpdateStamina(Player.Instance.Stamina);
    }
}
