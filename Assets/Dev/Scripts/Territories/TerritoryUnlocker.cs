using DG.Tweening;
using Structs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class TerritoryUnlocker : MonoBehaviour
{
    public TerritoryType TerritoryType;

    [SerializeField] private bool _unlockedByDefault;
    [SerializeField] private List<GameObject> _baseBlocks;
    [SerializeField] private List<GameObject> _baseStructures;
    [SerializeField] private GameObject _territoryPrice;

    private bool _isOpening = false;
    private int _territoryID;
    private int _curPart;
    private int _partsLeft;
    private GameObject _curPrice;

    private void Start()
    {
        if (!_unlockedByDefault && !Data.Instance.TerritoriesUnlocked[_territoryID])
        {
            GetComponent<BoxCollider>().enabled = true;
            _curPrice = Instantiate(_territoryPrice, transform.position + Vector3.up * 2, Quaternion.Euler(45, -45, 0), transform);
            if(TerritoryType == TerritoryType.desert)
            {
                if(_baseBlocks.Count < 16)
                    _curPrice.transform.localPosition = _curPrice.transform.localPosition + new Vector3(-1.5f, 0, 1.5f);
                else
                    _curPrice.transform.position = _curPrice.transform.position + new Vector3(-1.5f, 0, 1.5f);
            }
            else if(TerritoryType == TerritoryType.mountain)
                _curPrice.transform.position = _curPrice.transform.position + new Vector3(-1.5f, 0, -0.5f);
            _curPrice.GetComponent<TextMeshPro>().text = _baseBlocks.Count.ToString();

            foreach (var item in _baseBlocks)
            {
                item.SetActive(false);
            }
            foreach (var item in _baseStructures)
            {
                item.SetActive(false);
            }
        }
        else
        {
            Data.Instance.TerritoriesUnlocked[_territoryID] = true;
            GetComponent<ZombieSpawner>().InitializeZombieSpawning();
        }
        _partsLeft = _baseBlocks.Count;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.GetComponent<PlayerController>() || _isOpening)
            return;

        StopAllCoroutines();
        StartCoroutine(UnlockingTerritory());
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.gameObject.GetComponent<PlayerController>() || _isOpening)
            return;

        if(_curPart == _baseBlocks.Count)
            StartCoroutine(StopUnlocking());
        else
            StopAllCoroutines();
    }

    public void SetupID(int _id)
    {
        _territoryID = _id;
    }

    public int GetID()
    {
        return _territoryID;
    }

    public Vector3 GetRandomBlockPos()
    {
        return _baseBlocks[Random.Range(0, _baseBlocks.Count)].transform.position;
    }

    private IEnumerator StopUnlocking()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.TerritoryBlockFlyTime);

        StopAllCoroutines();
    }

    private IEnumerator UnlockingTerritory()
    {
        _baseBlocks.Sort((x, y) => { return (General.Instance.Player.transform.position - x.transform.position).sqrMagnitude.CompareTo((General.Instance.Player.transform.position - y.transform.position).sqrMagnitude); });

        _curPart = 0;

        while (_curPart < _baseBlocks.Count)
        {
            while (_baseBlocks[_curPart].activeInHierarchy && _curPart < _baseBlocks.Count - 1)
                _curPart++;

            if((_baseBlocks.Count - _partsLeft + General.Instance.GameSettings.TerritoryBuiltPerBlock) % General.Instance.GameSettings.TerritoryBuiltPerBlock == 0)
            {
                if(General.Instance.Player.GetComponent<PlayerInventory>().TryTakeBlock(BlockType.dirt) == new Vector3(-1, -1, -1))
                    yield break;
            }

            yield return new WaitForSeconds(General.Instance.GameSettings.TerritoryBlockDelay);

            _partsLeft--;
            if(_partsLeft < 0)
                _curPrice.GetComponent<TextMeshPro>().text = "0";
            else
                _curPrice.GetComponent<TextMeshPro>().text = _partsLeft.ToString();

            _baseBlocks[_curPart].SetActive(true);
            _baseBlocks[_curPart].GetComponent<BoxCollider>().enabled = false;

            Vector3 _targetPos = _baseBlocks[_curPart].transform.position;
            Vector3 _targetScale = _baseBlocks[_curPart].transform.localScale;
            _baseBlocks[_curPart].transform.position = General.Instance.Player.transform.position;
            _baseBlocks[_curPart].transform.localScale = Vector3.zero;
            _baseBlocks[_curPart].transform.DOJump(_targetPos, 2f, 1, General.Instance.GameSettings.TerritoryBlockFlyTime);
            _baseBlocks[_curPart].transform.DOScale(_targetScale, General.Instance.GameSettings.TerritoryBlockFlyTime);

            _curPart++;
        }

        _isOpening = true;
        _curPrice.transform.DOScale(Vector3.zero, General.Instance.GameSettings.TerritoryBlockFlyTime);

        yield return new WaitForSeconds(General.Instance.GameSettings.TerritoryBlockFlyTime);

        foreach (var item in _baseBlocks)
        {
            item.GetComponent<BoxCollider>().enabled = true;
        }
        foreach (var item in _baseStructures)
        {
            item.SetActive(true);
            Vector3 _targetScale = item.transform.localScale;
            item.transform.localScale = Vector3.zero;
            DOTween.Sequence().Append(item.transform.DOScale(_targetScale * General.Instance.GameSettings.BlockUnlockBounceScale, General.Instance.GameSettings.BlockUnlockBounceTime * 0.75f)).Append(item.transform.DOScale(_targetScale, General.Instance.GameSettings.BlockUnlockBounceTime * 0.25f));
        }
        DOTween.Sequence().Append(transform.DOLocalMove(transform.localPosition + Vector3.up, General.Instance.GameSettings.BlockUnlockBounceTime * 0.75f)).Append(transform.DOLocalMove(transform.localPosition, General.Instance.GameSettings.BlockUnlockBounceTime * 0.25f));
        Data.Instance.TerritoriesUnlocked[_territoryID] = true;
        NavMeshBaker.instance.RebuildNavMesh();
        TerritoryPriceToggle.instance.CheckTerritories();
        GetComponent<ZombieSpawner>().InitializeZombieSpawning();
        Destroy(_curPrice);

        yield return new WaitForSeconds(General.Instance.GameSettings.BlockUnlockBounceTime);

        GetComponent<BoxCollider>().enabled = false;
    }
}