[System.Serializable]
public class SceneSaveHouse
{
    public Vector3Serializable position;
    public int houseCode;
    public bool isHouseRepaired01;
    public bool isHouseRepaired02;

    public SceneSaveHouse()
    {
        position = new Vector3Serializable();
    }
}
