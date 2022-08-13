using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workbench : MonoBehaviour
{
    [SerializeField] private GameObject[] _areas;
    [SerializeField] private GameObject[] _craftables;
    [SerializeField] private GameObject[] _blocksVisual;

    private List<List<GameObject>> _craftBlocks = new List<List<GameObject>>();

    private bool _allowChanging = true;
    private bool _craftOpen = false;
    private int _curSelection = 0;

    private void Awake()
    {
        for (int i = 1; i < _areas.Length; i++)
        {
            _craftBlocks.Add(new List<GameObject>());
        }
    }

    private void Start()
    {
        _areas[0].transform.localScale = Vector3.zero;
        _areas[0].transform.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);
    }

    private void Update()
    {
        if (!_allowChanging || !_craftOpen)
            return;

        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out RaycastHit hit) && ((hit.collider.gameObject == gameObject) || (hit.collider.gameObject == General.Instance.Player)))
                StartCoroutine(ChangeRecipe());
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit) && ((hit.collider.gameObject == gameObject) || (hit.collider.gameObject == General.Instance.Player)))
                StartCoroutine(ChangeRecipe());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<PlayerController>() || _craftOpen)
            return;

        _craftOpen = true;
        _areas[0].transform.DOScale(Vector3.one, General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        if (!_craftables[_curSelection].activeInHierarchy)
            StartCoroutine(ChangeRecipe(true));
        else
        {
            _craftables[_curSelection].transform.localScale = Vector3.one;
            _allowChanging = true;
            StartCoroutine(MoveBlocks());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<PlayerController>() || !_craftOpen)
            return;

        _craftOpen = false;
        _areas[0].transform.DOScale(Vector3.zero, General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        StartCoroutine(StopDelay());
    }

    private IEnumerator StopDelay()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime);
        StopAllCoroutines();
        StartCoroutine(ClearWorkbench());
    }

    private IEnumerator ClearWorkbench()
    {
        float _totalBlocks = 1;
        foreach (var item in _craftables[_curSelection].GetComponent<CraftableTool>().BlockAmounts)
        {
            _totalBlocks += item;
        }

        for (int i = 0; i < _craftBlocks.Count; i++)
        {
            for (int j = 0; j < _craftBlocks[i].Count; j++)
            {
                if (_craftables[_curSelection].activeInHierarchy)
                {
                    General.Instance.Player.GetComponent<PlayerInventory>().AddBlock(_craftables[_curSelection].GetComponent<CraftableTool>().RequiredBlocks[i], _craftBlocks[i][j].transform.position);
                    Destroy(_craftBlocks[i][j]);

                    yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2 / _totalBlocks);
                }
                else
                {
                    _craftBlocks[i][j].transform.DOMove(_craftBlocks[i][j].transform.position + Vector3.down * 2, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                    Destroy(_craftBlocks[i][j], General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                }
            }
        }
        foreach (var item in _craftBlocks)
        {
            item.Clear();
        }
    }

    private IEnumerator MoveBlocks()
    {
        bool _enoughBlocks = true;

        float _totalBlocks = 0;
        foreach (var item in _craftables[_curSelection].GetComponent<CraftableTool>().BlockAmounts)
        {
            _totalBlocks += item;
        }

        for (int i = 0; i < _craftables[_curSelection].GetComponent<CraftableTool>().BlockAmounts.Length; i++)
        {
            for (int j = 0; j < _craftables[_curSelection].GetComponent<CraftableTool>().BlockAmounts[i]; j++)
            {
                Vector3 _pos = General.Instance.Player.GetComponent<PlayerInventory>().TryTakeBlock(_craftables[_curSelection].GetComponent<CraftableTool>().RequiredBlocks[i]);
                if (_pos != new Vector3(-1, -1, -1))
                {
                    GameObject _curblock = Instantiate(_blocksVisual[(int)_craftables[_curSelection].GetComponent<CraftableTool>().RequiredBlocks[i]], _areas[i + 1].transform);
                    _curblock.transform.localScale = Vector3.one;
                    _curblock.transform.position = _pos;
                    _curblock.transform.DOLocalMove(Vector3.zero + Vector3.up * j * 0.25f, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                    _craftBlocks[i].Add(_curblock);

                    yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2 / _totalBlocks);
                }
                else
                    _enoughBlocks = false;
            }
        }

        if (_enoughBlocks)
            StartCoroutine(CreateItem());
    }

    private IEnumerator ChangeRecipe(bool _firstChange = false)
    {
        _allowChanging = false;

        StartCoroutine(ClearWorkbench());

        if (_firstChange)
            _curSelection = 0;
        else
        {
            _craftables[_curSelection].transform.DOScale(Vector3.zero, General.Instance.GameSettings.InventoryBlockCollectTime / 2);

            yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime);

            _craftables[_curSelection].SetActive(false);

            _curSelection++;
            if (_curSelection == _craftables.Length)
                _curSelection = 0;
        }

        _craftables[_curSelection].SetActive(true);
        _craftables[_curSelection].transform.localScale = Vector3.zero;
        _craftables[_curSelection].transform.DOScale(Vector3.one, General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        StartCoroutine(MoveBlocks());

        yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        _allowChanging = true;
    }

    private IEnumerator CreateItem()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        _craftables[_curSelection].transform.DOJump(General.Instance.Player.transform.position + Vector3.up, 2f, 1, General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        _craftables[_curSelection].SetActive(false);
        _craftables[_curSelection].transform.localPosition = Vector3.zero;

        switch (_craftables[_curSelection].GetComponent<CraftableTool>().GearType)
        {
            case Structs.GearType.sword:
                General.Instance.Player.GetComponent<PlayerCombat>().ChangeSword((int)_craftables[_curSelection].GetComponent<CraftableTool>().GearQuality);
                break;
            case Structs.GearType.pickaxe:
                General.Instance.Player.GetComponent<PlayerMining>().ChangePickaxe((int)_craftables[_curSelection].GetComponent<CraftableTool>().GearQuality);
                break;
            case Structs.GearType.armor:
                //General.Instance.Player.GetComponent<PlayerHealth>().ChangeArmor((int)_craftables[_curSelection].GetComponent<CraftableTool>().GearQuality);
                break;
            default:
                break;
        }
    }
}