using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcInstantiate : MonoBehaviour
{
    public GameObject npcPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (HouseManager.Instance.isFirstTimeSceneLoadToInstantiateNpc)
        {
            Instantiate(npcPrefab, transform.position, Quaternion.identity);
            HouseManager.Instance.isFirstTimeSceneLoadToInstantiateNpc = false;
            Debug.Log("instantiated");

        }
    }
}
