using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseManager : SingletonMonobehaviour<HouseManager>
{
    [HideInInspector] public bool isPlayTimeline = false;
    [HideInInspector] public int numOfRebuildHouse = 0;
    [HideInInspector] public int numOfMemory = 0;

    [HideInInspector] public bool isFirstTimeSceneLoadToInstantiateNpc = false;

    public void UpdateMemory()
    {
        if (isPlayTimeline)
        {
            // house 01
            if (numOfRebuildHouse == 1 && SceneManager.GetActiveScene().name == "Scene1_Farm")
            {
                // TODO: Play timeline
                Debug.Log(SceneManager.GetActiveScene().name + " play timeline & trigger memory 01");

                // TODO: set this to timeline event
                PlayTimelineMemory();

                isPlayTimeline = false;
            }
            //house 02...

            isFirstTimeSceneLoadToInstantiateNpc = true;
        }
    }

    public void PlayTimelineMemory()
    {
        numOfMemory++;
        switch (numOfMemory)
        {
            case 1:
                // play timeline;
                Debug.Log("got memory 01");
                break;

            default:
                break;
        }

        // trigger memory ui in ui manager
    }
}
