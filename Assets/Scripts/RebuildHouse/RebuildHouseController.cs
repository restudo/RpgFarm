using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebuildHouseController : MonoBehaviour
{
    [SerializeField] private GameObject[] repairedHouse;
    [SerializeField] private Transform[] brokenHouse;

    public void RebuildHouse(int repairedHouseIndex, int brokenHouseIndex)
    {
        if (repairedHouse.Length > repairedHouseIndex && repairedHouseIndex >= 0 && brokenHouseIndex >= 0 && repairedHouse.Length > brokenHouseIndex)
        {
            Instantiate(repairedHouse[repairedHouseIndex], brokenHouse[brokenHouseIndex].position, Quaternion.identity);

            Destroy(brokenHouse[brokenHouseIndex].gameObject);
        }
        else
        {
            Debug.Log("repairedHouseIndex and brokenHouseIndex are wrong!!!");
        }

    }
}
