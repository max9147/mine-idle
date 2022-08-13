using DG.Tweening;
using Structs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningMachine : MonoBehaviour
{
    [SerializeField] private GameObject _woodBlock;
    [SerializeField] private GameObject _goldBlock;
    [SerializeField] private GameObject _diamondBlock;

    [SerializeField] private Transform _woodPos;

    [SerializeField] private Transform _spawnPos;
    [SerializeField] private Transform _targetPos;

    private List<GameObject> _spawnedOres;
    private List<BlockType> _spawnedOresTypes;
    private GameObject _currentWoodBlock;
    private GameObject _currentOreBlock;
    private int _currentWoodAmount = 0;

    private void Awake()
    {
        _spawnedOres = new List<GameObject>();
        _spawnedOresTypes = new List<BlockType>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<PlayerInventory>())
            return;

        if (_currentWoodAmount == 0)
        {
            Vector3 _pos = other.GetComponent<PlayerInventory>().TryTakeBlock(BlockType.wood);
            if (_pos != new Vector3(-1, -1, -1))
            {
                _currentWoodBlock = Instantiate(_woodBlock, _woodPos);
                _currentWoodBlock.transform.localScale = Vector3.one;
                _currentWoodBlock.transform.position = _pos;
                _currentWoodBlock.transform.DOLocalMove(Vector3.zero, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                _currentWoodAmount = General.Instance.GameSettings.BlocksMinedPerWood;
                StartMachine();
            }
        }

        if (_spawnedOres.Count > 0)
        {
            for (int i = 0; i < _spawnedOres.Count; i++)
                other.GetComponent<PlayerInventory>().AddBlock(_spawnedOresTypes[i], _spawnedOres[i].transform.position);
            foreach (var item in _spawnedOres)
                Destroy(item);
            _spawnedOres.Clear();
            _spawnedOresTypes.Clear();
        }
    }

    public void StartMachine()
    {
        if (_currentWoodAmount > 0)
        {
            StopAllCoroutines();
            StartCoroutine(Mining());
            GetComponent<Animator>().SetTrigger("start");
        }
    }

    private IEnumerator Mining()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.MachineMineTime);

        if (Random.Range(0f, 1f) < 0.2f)
        {
            _currentOreBlock = Instantiate(_diamondBlock, _spawnPos.position, Quaternion.identity, transform);
            _spawnedOresTypes.Add(BlockType.diamond);
        }
        else
        {
            _currentOreBlock = Instantiate(_goldBlock, _spawnPos.position, Quaternion.identity, transform);
            _spawnedOresTypes.Add(BlockType.gold);
        }

        _currentOreBlock.transform.localScale = Vector3.one * 0.5f;
        _currentOreBlock.transform.DOJump(_targetPos.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(0, 0.05f), Random.Range(-0.4f, 0.4f)), 1f, 1, 0.5f);

        _spawnedOres.Add(_currentOreBlock);

        _currentWoodAmount--;

        if (_currentWoodAmount == 0)
        {
            Destroy(_currentWoodBlock);
            GetComponent<Animator>().SetTrigger("end");
        }
        else
            StartCoroutine(Mining());
    }
}