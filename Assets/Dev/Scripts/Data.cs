using Structs;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Data : MonoBehaviour
{
    public static Data Instance;

    [SerializeField] private DataStruct _data;

    private string _dataPath;

    public int TutorialStep { get { return _data.TutorialStep; } set { _data.TutorialStep = value; } }
    public float TimePlayed { get { return _data.TimePlayed; } set { _data.TimePlayed = value; } }

    public int PlayerPickaxe { get { return _data.PlayerPickaxe; } set { _data.PlayerPickaxe = value; } }
    public float PickaxeHealth { get { return _data.PickaxeHealth; } set { _data.PickaxeHealth = value; } }
    public int PlayerSword { get { return _data.PlayerSword; } set { _data.PlayerSword = value; } }
    public float SwordHealth { get { return _data.SwordHealth; } set { _data.SwordHealth = value; } }

    public int[] PlayerArmors { get { return _data.PlayerArmors; } set { _data.PlayerArmors = value; } }
    public float[] ArmorsHealth { get { return _data.ArmorsHealth; } set { _data.ArmorsHealth = value; } }

    public List<BlockType> CollectedBlocks { get { return _data.CollectedBlocks; } set { _data.CollectedBlocks = value; } }
    public bool[] TerritoriesUnlocked { get { return _data.TerritoriesUnlocked; } set { _data.TerritoriesUnlocked = value; } }
    public bool[] BuildingsUnlocked { get { return _data.BuildingsUnlocked; } set { _data.BuildingsUnlocked = value; } }

    private void Awake()
    {
        Instance = this;

#if UNITY_ANDROID && !UNITY_EDITOR
        _dataPath = Path.Combine(Application.persistentDataPath, "data.json");
#else
        _dataPath = Path.Combine(Application.dataPath, "data.json");
#endif

        LoadData();
    }

    public void SaveData()
    {
        try
        {
            File.WriteAllText(_dataPath, JsonUtility.ToJson(_data));
        }
        catch
        {
            Debug.Log("Save error!");
        }
    }

    public void LoadData()
    {
        if (File.Exists(_dataPath))
            _data = JsonUtility.FromJson<DataStruct>(File.ReadAllText(_dataPath));
        else
        {
            Debug.Log("No saved data found!");
            SaveData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (Application.platform == RuntimePlatform.Android)
            SaveData();
    }
}