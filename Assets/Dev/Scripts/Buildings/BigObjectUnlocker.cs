using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BigObjectUnlocker : MonoBehaviour
{
    [SerializeField] private GameObject[] _zonesNeeded;
    [SerializeField] private GameObject _unlockableObject;

    private int[] _zoneIDs;

    private void Start()
    {
        _zoneIDs = new int[_zonesNeeded.Length];
        for (int i = 0; i < _zoneIDs.Length; i++)
            _zoneIDs[i] = _zonesNeeded[i].GetComponent<TerritoryUnlocker>().GetID();

        _unlockableObject.SetActive(false);

        StartCoroutine(CheckForSpace());
    }

    private IEnumerator CheckForSpace()
    {
        yield return new WaitForSeconds(1f);

        bool _allUnlocked = true;

        for (int i = 0; i < _zoneIDs.Length; i++)
        {
            if (!Data.Instance.TerritoriesUnlocked[_zoneIDs[i]])
                _allUnlocked = false;
        }

        if (_allUnlocked)
        {
            _unlockableObject.SetActive(true);

            Vector3 _targetScale = _unlockableObject.transform.localScale;
            _unlockableObject.transform.localScale = Vector3.zero;
            DOTween.Sequence().Append(_unlockableObject.transform.DOScale(_targetScale * General.Instance.GameSettings.BlockUnlockBounceScale, General.Instance.GameSettings.BlockUnlockBounceTime * 0.75f)).Append(_unlockableObject.transform.DOScale(_targetScale, General.Instance.GameSettings.BlockUnlockBounceTime * 0.25f));

            if (_unlockableObject.GetComponent<MiningMachine>())
                _unlockableObject.GetComponent<MiningMachine>().StartMachine();
        }
        else
            StartCoroutine(CheckForSpace());
    }
}