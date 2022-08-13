using Structs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerInventory : MonoBehaviour
{
    public UnityEvent OnChange;

    [SerializeField] private Transform _inventoryPivot;
    [SerializeField] private GameObject[] _blocksVisual;

    private List<BlockType> _collectedBlocks;
    private List<GameObject> _collectedVisuals;
    private GameSettings _gameSettings;

    private float _capacity;
    private int _collectedBlocksCount;

    private void Awake()
    {
        _collectedBlocks = new List<BlockType>();
        _collectedVisuals = new List<GameObject>();
    }

    private void Start()
    {
        _gameSettings = General.Instance.GameSettings;
        _capacity = _gameSettings.InventoryCapacity;

        _collectedBlocks = Data.Instance.CollectedBlocks;
        if (_collectedBlocks.Count > 0)
            InitializeInventory();

        _collectedBlocksCount = _collectedBlocks.Count;

        OnChange.Invoke();
    }

    private void InitializeInventory()
    {
        for (int i = 0; i < _collectedBlocks.Count; i++)
        {
            int y = i / (_gameSettings.InventoryX * _gameSettings.InventoryZ);
            int z = i % (_gameSettings.InventoryX * _gameSettings.InventoryZ) / _gameSettings.InventoryX;
            int x = i % (_gameSettings.InventoryX * _gameSettings.InventoryZ) % _gameSettings.InventoryX;
            Vector3 _targetPos = new Vector3(x - (3 / 2f) + 0.5f, y, -z) * _gameSettings.InventoryMargin;

            GameObject _curBlock = Instantiate(_blocksVisual[(int)_collectedBlocks[i]], _inventoryPivot);
            _collectedVisuals.Add(_curBlock);
            _curBlock.transform.localPosition = _targetPos;
            _curBlock.transform.localEulerAngles = Vector3.zero;
            _curBlock.transform.localScale = Vector3.one;
        }
    }

    public void AddBlock(BlockType _blockType, Vector3 _startPos)
    {
        if (_collectedBlocks.Count >= _capacity)
            return;

        if (_blockType == BlockType.grass)
            _blockType = BlockType.dirt;

        if (_blockType == BlockType.tree)
            _blockType = BlockType.wood;

        GameObject _curBlock = Instantiate(_blocksVisual[(int)_blockType], _startPos, Quaternion.identity);
        _collectedVisuals.Add(_curBlock);

        int y = _collectedBlocksCount / (_gameSettings.InventoryX * _gameSettings.InventoryZ);
        int z = _collectedBlocksCount % (_gameSettings.InventoryX * _gameSettings.InventoryZ) / _gameSettings.InventoryX;
        int x = _collectedBlocksCount % (_gameSettings.InventoryX * _gameSettings.InventoryZ) % _gameSettings.InventoryX;
        Vector3 _targetPos = new Vector3(x - (3 / 2f) + 0.5f, y, -z) * _gameSettings.InventoryMargin;

        _curBlock.GetComponent<BlockVisual>().DoMove(_targetPos, _inventoryPivot);

        _collectedBlocksCount++;

        StartCoroutine(InvokeChange(_blockType));
    }

    public Vector3 TryTakeBlock(BlockType _blockType, bool _collect=true)
    {
        if (_collectedBlocks.Contains(_blockType))
        {
            Vector3 _pos = _collectedVisuals[_collectedBlocks.IndexOf(_blockType)].transform.position;
            if(_collect)
            {
                Destroy(_collectedVisuals[_collectedBlocks.IndexOf(_blockType)]);
                _collectedVisuals.RemoveAt(_collectedBlocks.IndexOf(_blockType));
                _collectedBlocksCount--;
                _collectedBlocks.Remove(_blockType);
                Data.Instance.CollectedBlocks = _collectedBlocks;
                OnChange.Invoke();

                for(int i = 0; i < _collectedBlocksCount; i++)
                {
                    int y = i / (_gameSettings.InventoryX * _gameSettings.InventoryZ);
                    int z = i % (_gameSettings.InventoryX * _gameSettings.InventoryZ) / _gameSettings.InventoryX;
                    int x = i % (_gameSettings.InventoryX * _gameSettings.InventoryZ) % _gameSettings.InventoryX;
                    Vector3 _targetPos = new Vector3(x - (3 / 2f) + 0.5f, y, -z) * _gameSettings.InventoryMargin;

                    _collectedVisuals[i].GetComponent<BlockVisual>().transform.DOLocalMove(_targetPos, 0.1f);
                }
            }
            else
            {
                _collectedBlocks.Remove(_blockType);
                StartCoroutine(ReturnBlockDelay(_blockType));
            }
            return _pos;
        }
        else
            return new Vector3(-1, -1, -1);
    }

    public void DropInventory()
    {
        _collectedBlocks.Clear();
        _collectedBlocksCount = 0;

        foreach(var item in _collectedVisuals)
        {
            item.transform.parent = null;
            item.AddComponent<BoxCollider>();
            item.AddComponent<Rigidbody>().drag = 1;
            item.GetComponent<Rigidbody>().angularDrag = 1;
            item.AddComponent<SphereCollider>().isTrigger = true;
            item.GetComponent<SphereCollider>().radius = 1;
        }

        _collectedVisuals.Clear();

        OnChange.Invoke();
    }

    public int GetBlocksCount()
    {
        return _collectedBlocks.Count;
    }

    private IEnumerator ReturnBlockDelay(BlockType _blockType)
    {
        yield return new WaitForSeconds(0.05f);

        _collectedBlocks.Add(_blockType);
    }

    private IEnumerator InvokeChange(BlockType _blockType)
    {
        yield return new WaitForSeconds(_gameSettings.InventoryBlockCollectTime);

        _collectedBlocks.Add(_blockType);
        Data.Instance.CollectedBlocks = _collectedBlocks;
        OnChange.Invoke();
    }
}