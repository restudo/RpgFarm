using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

[RequireComponent(typeof(GenerateGUID))]
public class SceneItemsManager : SingletonMonobehaviour<SceneItemsManager>, ISaveable
{
    private Transform parentItem;
    private Transform parentHouse01;
    private Transform parentHouse02;
    [SerializeField] private GameObject itemPrefab = null;

    [Header("House01")]
    [SerializeField] private GameObject brokenHouse01 = null;
    [SerializeField] private GameObject repairedHouse01 = null;

    [Header("House02")]
    [SerializeField] private GameObject brokenHouse02 = null;
    [SerializeField] private GameObject repairedHouse02 = null;

    private string _iSaveableUniqueID;
    public string ISaveableUniqueID { get { return _iSaveableUniqueID; } set { _iSaveableUniqueID = value; } }

    private GameObjectSave _gameObjectSave;
    public GameObjectSave GameObjectSave { get { return _gameObjectSave; } set { _gameObjectSave = value; } }

    private void AfterSceneLoad()
    {
        parentItem = GameObject.FindGameObjectWithTag(Tags.ItemsParentTransform).transform;
        if (GameObject.FindGameObjectWithTag(Tags.House01) != null)
            parentHouse01 = GameObject.FindGameObjectWithTag(Tags.House01).transform;

        if (GameObject.FindGameObjectWithTag(Tags.House02) != null)
            parentHouse02 = GameObject.FindGameObjectWithTag(Tags.House02).transform;

    }

    protected override void Awake()
    {
        base.Awake();

        ISaveableUniqueID = GetComponent<GenerateGUID>().GUID;
        GameObjectSave = new GameObjectSave();
    }

    /// <summary>
    /// Destroy items currently in the scene
    /// </summary>
    private void DestroySceneItems()
    {
        // Get all items in the scene
        Item[] itemsInScene = GameObject.FindObjectsOfType<Item>();

        // Loop through all scene items and destroy them
        for (int i = itemsInScene.Length - 1; i > -1; i--)
        {
            Destroy(itemsInScene[i].gameObject);
        }
    }

    private void DestroySceneHouses()
    {
        // Get all items in the scene
        House[] housesInScene = GameObject.FindObjectsOfType<House>();

        // Loop through all scene Houses and destroy them
        for (int i = housesInScene.Length - 1; i > -1; i--)
        {
            Destroy(housesInScene[i].gameObject);
        }
    }

    public void InstantiateSceneItem(int itemCode, Vector3 itemPosition)
    {
        GameObject itemGameObject = Instantiate(itemPrefab, itemPosition, Quaternion.identity, parentItem);
        Item item = itemGameObject.GetComponent<Item>();
        item.Init(itemCode);
    }

    private void InstantiateSceneItems(List<SceneItem> sceneItemList)
    {
        GameObject itemGameObject;

        foreach (SceneItem sceneItem in sceneItemList)
        {
            itemGameObject = Instantiate(itemPrefab, new Vector3(sceneItem.position.x, sceneItem.position.y, sceneItem.position.z), Quaternion.identity, parentItem);

            Item item = itemGameObject.GetComponent<Item>();
            item.ItemCode = sceneItem.itemCode;
            item.name = sceneItem.itemName;
        }
    }

    private void InstantiateSceneHouse(List<SceneSaveHouse> sceneHousesList)
    {
        foreach (SceneSaveHouse sceneHouse in sceneHousesList)
        {
            if (sceneHouse.houseCode == 1)
            {
                if (sceneHouse.isHouseRepaired01)
                {
                    GameObject houseGameObject = Instantiate(repairedHouse01, new Vector3(sceneHouse.position.x, sceneHouse.position.y, sceneHouse.position.z), Quaternion.identity, parentHouse01);
                    House house = houseGameObject.GetComponent<House>();
                    house.HouseCode = sceneHouse.houseCode;
                    house.IsHouseRepaired01 = sceneHouse.isHouseRepaired01;
                }
                else
                {
                    GameObject houseGameObject = Instantiate(brokenHouse01, new Vector3(sceneHouse.position.x, sceneHouse.position.y, sceneHouse.position.z), Quaternion.identity, parentHouse01);
                    House house = houseGameObject.GetComponent<House>();
                    house.HouseCode = sceneHouse.houseCode;
                    // house.IsHouseRepaired01 = sceneHouse.isHouseRepaired01;
                }
            }

            if (sceneHouse.houseCode == 2)
            {
                if (sceneHouse.isHouseRepaired02)
                {
                    GameObject houseGameObject = Instantiate(repairedHouse02, new Vector3(sceneHouse.position.x, sceneHouse.position.y, sceneHouse.position.z), Quaternion.identity, parentHouse02);
                    House house = houseGameObject.GetComponent<House>();
                    house.HouseCode = sceneHouse.houseCode;
                    house.IsHouseRepaired02 = sceneHouse.isHouseRepaired02;
                }
                else
                {
                    GameObject houseGameObject = Instantiate(brokenHouse02, new Vector3(sceneHouse.position.x, sceneHouse.position.y, sceneHouse.position.z), Quaternion.identity, parentHouse02);
                    House house = houseGameObject.GetComponent<House>();
                    house.HouseCode = sceneHouse.houseCode;
                    // house.IsHouseRepaired01 = sceneHouse.isHouseRepaired01;
                }

            }
        }
    }

    private void OnDisable()
    {
        ISaveableDeregister();
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void OnEnable()
    {
        ISaveableRegister();
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    public void ISaveableDeregister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Remove(this);
    }

    public void ISaveableLoad(GameSave gameSave)
    {
        if (gameSave.gameObjectData.TryGetValue(ISaveableUniqueID, out GameObjectSave gameObjectSave))
        {
            GameObjectSave = gameObjectSave;

            // Restore data for current scene
            ISaveableRestoreScene(SceneManager.GetActiveScene().name);
        }
    }
    public void ISaveableRestoreScene(string sceneName)
    {
        if (GameObjectSave.sceneData.TryGetValue(sceneName, out SceneSave sceneSave))
        {
            if (sceneSave.listSceneItem != null)
            {
                // scene list items found - destroy existing items in scene
                DestroySceneItems();

                // now instantiate the list of scene items
                InstantiateSceneItems(sceneSave.listSceneItem);
            }

            if (sceneSave.listSceneHouse != null)
            {
                DestroySceneHouses();
                InstantiateSceneHouse(sceneSave.listSceneHouse);
            }
        }
    }

    public void ISaveableRegister()
    {
        SaveLoadManager.Instance.iSaveableObjectList.Add(this);
    }

    public GameObjectSave ISaveableSave()
    {
        // Store current scene data
        ISaveableStoreScene(SceneManager.GetActiveScene().name);

        return GameObjectSave;
    }
    public void ISaveableStoreScene(string sceneName)
    {
        // Remove old scene save for gameObject if exists
        GameObjectSave.sceneData.Remove(sceneName);

        // Get all items in the scene
        List<SceneItem> sceneItemList = new List<SceneItem>();
        Item[] itemsInScene = FindObjectsOfType<Item>();

        List<SceneSaveHouse> sceneHouseList = new List<SceneSaveHouse>();
        House[] housesInScene = FindObjectsOfType<House>();

        // Loop through all scene items
        foreach (Item item in itemsInScene)
        {
            SceneItem sceneItem = new SceneItem();
            sceneItem.itemCode = item.ItemCode;
            sceneItem.position = new Vector3Serializable(item.transform.position.x, item.transform.position.y, item.transform.position.z);
            sceneItem.itemName = item.name;

            // Add scene item to list
            sceneItemList.Add(sceneItem);
        }

        foreach (House house in housesInScene)
        {
            SceneSaveHouse sceneHouse = new SceneSaveHouse();
            sceneHouse.houseCode = house.HouseCode;
            sceneHouse.position = new Vector3Serializable(house.transform.position.x, house.transform.position.y, house.transform.position.z);

            sceneHouse.isHouseRepaired01 = house.IsHouseRepaired01;

            sceneHouse.isHouseRepaired02 = house.IsHouseRepaired02;

            // Add scene House to list
            sceneHouseList.Add(sceneHouse);
            Debug.Log(" ada " + sceneHouse.houseCode + " " + sceneHouse.position);
        }

        // Create list scene items in scene save and set to scene item list
        SceneSave sceneSave = new SceneSave();
        sceneSave.listSceneItem = sceneItemList;
        sceneSave.listSceneHouse = sceneHouseList;

        // Add scene save to gameobject
        GameObjectSave.sceneData.Add(sceneName, sceneSave);
    }
}
