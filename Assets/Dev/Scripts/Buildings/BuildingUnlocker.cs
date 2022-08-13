using DG.Tweening;
using Structs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingUnlocker : MonoBehaviour
{
    [SerializeField] private int _buildingID;
    [SerializeField] private GameObject _collider;
    [SerializeField] private List<GameObject> _baseBlocks;
    [SerializeField] private GameObject[] _zonesNeeded;

    private bool _isBuilding = false;
    private int _curPart;
    private int _curHealth = 0;
    private int[] _zoneIDs;

    private void Start()
    {
        _zoneIDs = new int[_zonesNeeded.Length];
        for (int i = 0; i < _zoneIDs.Length; i++)
            _zoneIDs[i] = _zonesNeeded[i].GetComponent<TerritoryUnlocker>().GetID();

        StartCoroutine(CheckForSpace());

        if (!Data.Instance.BuildingsUnlocked[_buildingID])
        {
            foreach (var item in _baseBlocks)
            {
                item.SetActive(false);
            }
        }
        else
        {
            _baseBlocks.Sort((x, y) => { return x.transform.position.y.CompareTo(y.transform.position.y); });
            _curHealth = _baseBlocks.Count;

            if (GetComponent<AllyBuilding>())
                GetComponent<AllyBuilding>().InitializeAllySpawning();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetComponent<PlayerController>())
            return;

        StopAllCoroutines();
        StartCoroutine(UnlockingBuilding());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.GetComponent<PlayerController>())
            return;

        _isBuilding = false;
        StopAllCoroutines();
    }

    public void BreakBuilding()
    {
        if (_curHealth > 0 && !_isBuilding)
        {
            while (!_baseBlocks[_curHealth - 1].activeInHierarchy && _curHealth > 1)
                _curHealth--;

            StartCoroutine(DestroyBlock(_curHealth - 1));
            _curHealth--;
        }

        if ((float)_curHealth / _baseBlocks.Count < General.Instance.GameSettings.AllySpawnHealthTreshold)
            GetComponent<AllyBuilding>().StopAllCoroutines();

        if (_curHealth == 0)
            ZombieHivemind.Instance.PossibleTargets.Remove(gameObject);
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

        if (!Data.Instance.BuildingsUnlocked[_buildingID])
        {
            GetComponent<BoxCollider>().enabled = _allUnlocked;
            _collider.SetActive(_allUnlocked);
        }

        StartCoroutine(CheckForSpace());
    }

    private IEnumerator UnlockingBuilding()
    {
        _isBuilding = true;
        _baseBlocks.Sort((x, y) => { return (General.Instance.Player.transform.position - x.transform.position).sqrMagnitude.CompareTo((General.Instance.Player.transform.position - y.transform.position).sqrMagnitude); });

        _curPart = 0;

        while (_curPart < _baseBlocks.Count)
        {
            while (_baseBlocks[_curPart].activeInHierarchy && _curPart < _baseBlocks.Count - 1)
                _curPart++;

            if((_curHealth - 5) % 5 == 0)
            {
                if(General.Instance.Player.GetComponent<PlayerInventory>().TryTakeBlock(BlockType.stone) == new Vector3(-1, -1, -1))
                    yield break;
            }

            yield return new WaitForSeconds(General.Instance.GameSettings.BuildingBlockDelay);

            _baseBlocks[_curPart].SetActive(true);
            _baseBlocks[_curPart].GetComponent<BoxCollider>().enabled = false;

            _baseBlocks[_curPart].transform.position = General.Instance.Player.transform.position;
            _baseBlocks[_curPart].transform.localScale = Vector3.zero;
            _baseBlocks[_curPart].transform.DOJump(_baseBlocks[_curPart].GetComponent<BuildingBlock>().DefaultPos, 2f, 1, General.Instance.GameSettings.BuildingBlockFlyTime);
            _baseBlocks[_curPart].transform.DOScale(_baseBlocks[_curPart].GetComponent<BuildingBlock>().DefaultScale, General.Instance.GameSettings.BuildingBlockFlyTime);

            _curPart++;
            _curHealth++;
        }

        yield return new WaitForSeconds(General.Instance.GameSettings.BuildingBlockFlyTime);

        _isBuilding = false;
        GetComponent<BoxCollider>().enabled = false;
        foreach (var item in _baseBlocks)
        {
            item.GetComponent<BoxCollider>().enabled = true;
        }
        _baseBlocks.Sort((x, y) => { return x.transform.position.y.CompareTo(y.transform.position.y); });
        _curHealth = _baseBlocks.Count;

        DOTween.Sequence().Append(transform.DOScale(Vector3.one * 4 * General.Instance.GameSettings.BlockUnlockBounceScale, General.Instance.GameSettings.BlockUnlockBounceTime * 0.75f)).Append(transform.DOScale(Vector3.one * 4, General.Instance.GameSettings.BlockUnlockBounceTime * 0.25f));

        Data.Instance.BuildingsUnlocked[_buildingID] = true;
        NavMeshBaker.instance.RebuildNavMesh();
        ZombieHivemind.Instance.PossibleTargets.Add(gameObject);

        if (GetComponent<AllyBuilding>())
            GetComponent<AllyBuilding>().InitializeAllySpawning();
    }

    private IEnumerator DestroyBlock(int _blockID)
    {
        Vector3 _jumpTarget = new Vector3(_baseBlocks[_blockID].transform.position.x - (transform.position.x - _baseBlocks[_blockID].transform.position.x) * 2, 0, _baseBlocks[_blockID].transform.position.z - (transform.position.z - _baseBlocks[_blockID].transform.position.z) * 2);
        _baseBlocks[_blockID].transform.DOJump(_jumpTarget, 4f, 1, 0.5f);
        _baseBlocks[_blockID].transform.DOScale(Vector3.zero, 0.5f);

        yield return new WaitForSeconds(0.5f);

        _baseBlocks[_blockID].SetActive(false);

        GetComponent<BoxCollider>().enabled = true;
    }
}