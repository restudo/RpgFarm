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
    }

    private void OnEnable()
    {
        Lua.RegisterFunction("RebuildHouse", this, SymbolExtensions.GetMethodInfo(() => RebuildHouse((double)0)));
        // Lua.RegisterFunction("RebuildHouse01", this, SymbolExtensions.GetMethodInfo(() => RebuildHouse01()));
        // Lua.RegisterFunction("RebuildHouse02", this, SymbolExtensions.GetMethodInfo(() => RebuildHouse02()));
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

    private void Conversation()
    {
        DialogueManager.StartConversation("HouseRepaired");
    }

    // public void RebuildHouse01()
    // {
    //     DestroyBrokenHouses(1);

    //     Instantiate(repairedHouse01, spawnHouse01.position, Quaternion.identity, parentItem01);

    //     HouseManager.Instance.numOfRebuildHouse = 1;
    //     Debug.Log("num of rebuild house : " + HouseManager.Instance.numOfRebuildHouse);

    //     HouseManager.Instance.isPlayTimeline = true;
    //     Debug.Log("isplayingTimeline ? " + HouseManager.Instance.isPlayTimeline);
    // }

    // public void RebuildHouse02()
    // {
    //     DestroyBrokenHouses(2);


    //     HouseManager.Instance.isPlayTimeline = true;
    //     Debug.Log("isplayingTimeline ? " + HouseManager.Instance.isPlayTimeline);
    // }
}
