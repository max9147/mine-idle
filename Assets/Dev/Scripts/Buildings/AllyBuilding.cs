using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AllyBuilding : MonoBehaviour
{
    [SerializeField] private GameObject _ally;

    private void SpawnAlly()
    {
        if (AllyHivemind.Instance.CurAlliesAmount >= General.Instance.GameSettings.AllyMaxAmount)
            return;

        GameObject _curAlly = Instantiate(_ally, transform.position + Vector3.back * 3, Quaternion.Euler(0, 180, 0));
        _curAlly.transform.localScale = Vector3.zero;
        _curAlly.transform.DOScale(Vector3.one, 1f);
        AllyHivemind.Instance.CurAlliesAmount++;
        ZombieHivemind.Instance.PossibleTargets.Add(_curAlly);
    }

    public void InitializeAllySpawning()
    {
        StartCoroutine(SpawnAllyDelay());
    }

    private IEnumerator SpawnAllyDelay()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.AllySpawnDelay);

        SpawnAlly();

        StartCoroutine(SpawnAllyDelay());
    }
}