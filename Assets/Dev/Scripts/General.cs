using UnityEngine;
using UnityEngine.SceneManagement;

public class General : MonoBehaviour
{
    public static General Instance;

    public GameObject Player;
    public GameObject[] Territories;
    public GameObject[] Buildings;
    public GameSettings GameSettings;

    private void Awake()
    {
        Instance = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        for (int i = 0; i < Territories.Length; i++)
        {
            Territories[i].GetComponent<TerritoryUnlocker>().SetupID(i);
        }
    }

    private void Start()
    {
        if(Data.Instance.TerritoriesUnlocked.Length == 0)
            Data.Instance.TerritoriesUnlocked = new bool[Territories.Length];

        if(Data.Instance.BuildingsUnlocked.Length == 0)
            Data.Instance.BuildingsUnlocked = new bool[Buildings.Length];
    }
}