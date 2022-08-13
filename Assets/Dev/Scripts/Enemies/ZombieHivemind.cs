using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHivemind : MonoBehaviour
{
    public static ZombieHivemind Instance;

    public List<GameObject> PossibleTargets;

    public bool CanSpawn => _timePlayed >= General.Instance.GameSettings.ZombieNoSpawnTime;
    public int CurZombiesAmount = 0;

    private float _timePlayed = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < General.Instance.Buildings.Length; i++)
        {
            if (Data.Instance.BuildingsUnlocked[i])
                PossibleTargets.Add(General.Instance.Buildings[i]);
        }

        _timePlayed = Data.Instance.TimePlayed;

        if(_timePlayed<=General.Instance.GameSettings.ZombieNoSpawnTime)
            StartCoroutine(CountTimePlayed());
    }

    private IEnumerator CountTimePlayed()
    {
        yield return new WaitForSeconds(1f);

        _timePlayed++;

        Data.Instance.TimePlayed = _timePlayed;

        if(_timePlayed <= General.Instance.GameSettings.ZombieNoSpawnTime)
            StartCoroutine(CountTimePlayed());
    }
}