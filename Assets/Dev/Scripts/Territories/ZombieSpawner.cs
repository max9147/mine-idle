using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _zombie;

    [SerializeField] private bool _spawnZombies;

    private void SpawnZombie()
    {
        if(ZombieHivemind.Instance.CurZombiesAmount >= General.Instance.GameSettings.ZombieMaxAmount || Vector3.Distance(transform.position, General.Instance.Player.transform.position) >= 30f || !ZombieHivemind.Instance.CanSpawn)
            return;

        Vector3 _randomPos = GetComponent<TerritoryUnlocker>().GetRandomBlockPos();
        _randomPos = new Vector3(_randomPos.x, 2, _randomPos.z);

        if (!Physics.CheckSphere(_randomPos, 0.5f))
        {
            GameObject _curZombie = Instantiate(_zombie, _randomPos, Quaternion.Euler(0, 180, 0));
            _curZombie.transform.localScale = Vector3.zero;
            _curZombie.transform.DOScale(Vector3.one, 1f);
            ZombieHivemind.Instance.CurZombiesAmount++;
            AllyHivemind.Instance.PossibleTargets.Add(_curZombie);
        }
        else
            SpawnZombie();
    }

    public void InitializeZombieSpawning()
    {
        if (_spawnZombies)
            StartCoroutine(SpawnZombieDelay());
    }

    private IEnumerator SpawnZombieDelay()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.ZombieWaveDelays[(int)GetComponent<TerritoryUnlocker>().TerritoryType]);

        for (int i = 0; i < General.Instance.GameSettings.ZombieWaveSize[(int)GetComponent<TerritoryUnlocker>().TerritoryType]; i++)
        {
            SpawnZombie();
        }
        StartCoroutine(SpawnZombieDelay());
    }
}