using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Structs;

public class WorkbenchWeapon : MonoBehaviour
{
    [SerializeField] private GameObject[] _areas;
    [SerializeField] private GameObject[] _craftableSwords;
    [SerializeField] private GameObject[] _craftablePickaxes;
    [SerializeField] private GameObject[] _blocksVisual;
    [SerializeField] private GameObject _progressBar;
    [SerializeField] private GameObject _progressBarFill;

    [SerializeField] private GameObject _joystick;
    [SerializeField] private GameObject _craftMenu;
    [SerializeField] private Button _closeCraftMenuButton;
    [SerializeField] private Button _weaponBackButton;
    [SerializeField] private Button _weaponForwardButton;
    [SerializeField] private Button _weaponMatBackButton;
    [SerializeField] private Button _weaponMatForwardButton;
    [SerializeField] private Button _craftButton;

    [SerializeField] private Image _resultImage;
    [SerializeField] private Image _res1Image;
    [SerializeField] private Image _res2Image;

    [SerializeField] private Sprite[] _swordImages;
    [SerializeField] private Sprite[] _pickaxeImages;
    [SerializeField] private Sprite[] _materialImages;

    [SerializeField] private TextMeshProUGUI _weaponName;
    [SerializeField] private TextMeshProUGUI _res1Count;
    [SerializeField] private TextMeshProUGUI _res2Count;
    [SerializeField] private TextMeshProUGUI _statCount;

    [SerializeField] private GameObject _inventory;
    [SerializeField] private TextMeshProUGUI[] _resourceAmounts;
    [SerializeField] private Image[] _resourceImages;
    [SerializeField] private TextMeshProUGUI _newItemText;

    private List<List<GameObject>> _craftBlocks = new List<List<GameObject>>();

    private bool _isOpening = false;
    private bool _isCrafting = false;
    private GearType _curGearType;
    private GearQuality _curGearQuality;

    private GameObject _curCraftable;

    private BlockType[] _craftBlockTypes = { BlockType.wood, BlockType.stone, BlockType.ironOre, BlockType.gold, BlockType.diamond };

    private void Awake()
    {
        for(int i = 1; i < _areas.Length; i++)
        {
            _craftBlocks.Add(new List<GameObject>());
        }
    }

    private void Start()
    {
        _areas[0].transform.localScale = Vector3.zero;
        _areas[0].transform.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);

        _curGearType = GearType.pickaxe;
        _curGearQuality = GearQuality.wood;
    }

    private void FixedUpdate()
    {
        if(_isOpening)
            _progressBarFill.GetComponent<Image>().fillAmount += Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<PlayerController>() && !other.GetComponent<PlayerController>().GetMovingStatus() && !_isOpening)
        {
            _isOpening = true;
            _progressBar.SetActive(true);
            StartCoroutine(OpenWait());
        }
        if(General.Instance.Player.GetComponent<PlayerController>().GetMovingStatus())
        {
            _isOpening = false;
            _progressBar.SetActive(false);
            _progressBarFill.GetComponent<Image>().fillAmount = 0;
            StopAllCoroutines();
        }
    }

    public void CloseCraftMenu()
    {
        if(_isCrafting)
            return;

        _areas[0].transform.DOScale(Vector3.zero, 0.5f);
        _joystick.SetActive(true);
        _craftMenu.SetActive(false);
        _inventory.SetActive(false);
        _closeCraftMenuButton.onClick.RemoveAllListeners();
        _weaponBackButton.onClick.RemoveAllListeners();
        _weaponForwardButton.onClick.RemoveAllListeners();
        _weaponMatBackButton.onClick.RemoveAllListeners();
        _weaponMatForwardButton.onClick.RemoveAllListeners();
        _craftButton.onClick.RemoveAllListeners();
        General.Instance.Player.GetComponent<PlayerController>().ResumeAction();
    }

    private void EnterCraftMenu()
    {
        if(Data.Instance.TutorialStep == 5 || Data.Instance.TutorialStep == 8)
            Tutorial.instance.ProgressTutorial();

        _areas[0].transform.DOScale(Vector3.one, 0.5f);
        _progressBar.SetActive(false);
        _progressBarFill.GetComponent<Image>().fillAmount = 0;
        _joystick.SetActive(false);
        _craftMenu.SetActive(true);
        _inventory.SetActive(true);
        _closeCraftMenuButton.onClick.AddListener(CloseCraftMenu);

        if(Data.Instance.TutorialStep != 6 && Data.Instance.TutorialStep != 9)
        {
            _weaponBackButton.onClick.AddListener(ChangeWeapon);
            _weaponForwardButton.onClick.AddListener(ChangeWeapon);
            _weaponMatBackButton.onClick.AddListener(ChangeMatBack);
            _weaponMatForwardButton.onClick.AddListener(ChangeMatForward);
        }
        if(Data.Instance.TutorialStep == 6)
        {
            _curGearType = GearType.pickaxe;
            _curGearQuality = GearQuality.wood;
        }
        if(Data.Instance.TutorialStep == 9)
        {
            _curGearType = GearType.sword;
            _curGearQuality = GearQuality.wood;
        }

        _craftButton.onClick.AddListener(CraftItem);
        General.Instance.Player.GetComponent<PlayerController>().StopAction();

        SetCraftRecipy();
    }

    private IEnumerator OpenWait()
    {
        yield return new WaitForSeconds(1f);

        RefreshInventory();
        EnterCraftMenu();
    }

    public void ChangeWeapon()
    {
        if(_isCrafting)
            return;

        if(_curGearType == GearType.sword)
            _curGearType = GearType.pickaxe;
        else
            _curGearType = GearType.sword;

        SetCraftRecipy();
    }

    public void ChangeMatForward()
    {
        if(_isCrafting)
            return;

        if(_curGearQuality == GearQuality.diamond)
            _curGearQuality = GearQuality.wood;
        else
            _curGearQuality++;

        SetCraftRecipy();
    }

    public void ChangeMatBack()
    {
        if(_isCrafting)
            return;

        if(_curGearQuality == GearQuality.wood)
            _curGearQuality = GearQuality.diamond;
        else
            _curGearQuality--;

        SetCraftRecipy();
    }

    private void RefreshInventory()
    {
        for(int i = 0; i < _resourceAmounts.Length; i++)
        {
            List<BlockType> _curCount = Data.Instance.CollectedBlocks.FindAll(x => x == _craftBlockTypes[i]);
            _resourceAmounts[i].text = $"x{_curCount.Count}";
            if(_curCount.Count == 0)
                _resourceImages[i].color = new Color(0.2f, 0.2f, 0.2f);
            else
                _resourceImages[i].color = Color.white;
        }
    }

    private void SetCraftRecipy()
    {
        if(_curCraftable)
            _curCraftable.SetActive(false);

        _res2Image.sprite = _materialImages[(int)_curGearQuality - 1];
        if(_curGearType == GearType.sword)
        {
            _resultImage.sprite = _swordImages[(int)_curGearQuality - 1];
            _curCraftable = _craftableSwords[(int)_curGearQuality - 1];
        }
        else
        {
            _resultImage.sprite = _pickaxeImages[(int)_curGearQuality - 1];
            _curCraftable = _craftablePickaxes[(int)_curGearQuality - 1];
        }

        _curCraftable.SetActive(true);
        _curCraftable.transform.localScale = Vector3.zero;
        _curCraftable.transform.DOScale(Vector3.one, 0.2f);

        _res1Count.text = _curCraftable.GetComponent<CraftableTool>().BlockAmounts[0].ToString();
        _res2Count.text = _curCraftable.GetComponent<CraftableTool>().BlockAmounts[1].ToString();

        if(_curGearType == GearType.pickaxe)
        {
            _weaponName.text = "Pickaxe";
            _statCount.text = $"+{(int)_curGearQuality}";
        }
        else
        {
            _weaponName.text = "Sword";
            _statCount.text = $"+{5f * (int)_curGearQuality}";
        }

        bool _enoughBlocks = true;
        for(int i = 0; i < _curCraftable.GetComponent<CraftableTool>().BlockAmounts.Length; i++)
        {
            for(int j = 0; j < _curCraftable.GetComponent<CraftableTool>().BlockAmounts[i]; j++)
            {
                Vector3 _pos = General.Instance.Player.GetComponent<PlayerInventory>().TryTakeBlock(_curCraftable.GetComponent<CraftableTool>().RequiredBlocks[i], false);
                if(_pos == new Vector3(-1, -1, -1))
                    _enoughBlocks = false;
            }
        }
        _craftButton.interactable = _enoughBlocks;
    }

    private void CraftItem()
    {
        if(_isCrafting)
            return;

        _isCrafting = true;
        StartCoroutine(CraftCoroutine());

        if(Data.Instance.TutorialStep == 6 || Data.Instance.TutorialStep == 9)
            Tutorial.instance.ProgressTutorial();
    }

    private IEnumerator CraftCoroutine()
    {
        _curCraftable.transform.DOJump(General.Instance.Player.transform.position + Vector3.up, 2f, 1, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
        StartCoroutine(NewItemTextVisual());

        float _totalBlocks = 0;
        foreach(var item in _curCraftable.GetComponent<CraftableTool>().BlockAmounts)
        {
            _totalBlocks += item;
        }

        for(int i = 0; i < _curCraftable.GetComponent<CraftableTool>().BlockAmounts.Length; i++)
        {
            for(int j = 0; j < _curCraftable.GetComponent<CraftableTool>().BlockAmounts[i]; j++)
            {
                Vector3 _pos = General.Instance.Player.GetComponent<PlayerInventory>().TryTakeBlock(_curCraftable.GetComponent<CraftableTool>().RequiredBlocks[i]);

                GameObject _curblock = Instantiate(_blocksVisual[(int)_curCraftable.GetComponent<CraftableTool>().RequiredBlocks[i]], _areas[i + 1].transform);
                _curblock.transform.localScale = Vector3.one;
                _curblock.transform.position = _pos;
                _curblock.transform.DOLocalMove(Vector3.zero + Vector3.up * j * 0.25f, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                _craftBlocks[i].Add(_curblock);

                yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2 / _totalBlocks);
            }
        }

        _curCraftable.transform.localScale = Vector3.zero;
        RefreshInventory();

        yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        for(int i = 0; i < _craftBlocks.Count; i++)
        {
            for(int j = 0; j < _craftBlocks[i].Count; j++)
            {
                _craftBlocks[i][j].transform.DOMove(_craftBlocks[i][j].transform.position + Vector3.down * 2, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                Destroy(_craftBlocks[i][j], General.Instance.GameSettings.InventoryBlockCollectTime / 2);
            }
        }
        foreach(var item in _craftBlocks)
        {
            item.Clear();
        }

        _curCraftable.transform.localPosition = Vector3.zero;
        _curCraftable.transform.DOScale(Vector3.one, 0.2f);

        switch(_curCraftable.GetComponent<CraftableTool>().GearType)
        {
            case GearType.sword:
                General.Instance.Player.GetComponent<PlayerCombat>().ChangeSword((int)_curCraftable.GetComponent<CraftableTool>().GearQuality);
                break;
            case GearType.pickaxe:
                General.Instance.Player.GetComponent<PlayerMining>().ChangePickaxe((int)_curCraftable.GetComponent<CraftableTool>().GearQuality);
                break;
            default:
                break;
        }

        _isCrafting = false;

        SetCraftRecipy();
    }

    private IEnumerator NewItemTextVisual()
    {
        _newItemText.text = $"Got {_curGearQuality} {_curGearType}!";
        _newItemText.gameObject.SetActive(true);
        _newItemText.transform.localScale = Vector3.zero;
        _newItemText.transform.DOScale(Vector3.one, 0.2f);

        yield return new WaitForSeconds(0.8f);

        _newItemText.transform.DOScale(Vector3.zero, 0.2f);

        yield return new WaitForSeconds(0.2f);

        _newItemText.gameObject.SetActive(true);
    }
}