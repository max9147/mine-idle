using Cinemachine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Structs;

public class WorkbenchArmor : MonoBehaviour
{
    [SerializeField] private GameObject[] _areas;
    [SerializeField] private GameObject[] _craftableHelmets;
    [SerializeField] private GameObject[] _craftableArmors;
    [SerializeField] private GameObject[] _craftableBracers;
    [SerializeField] private GameObject[] _craftableLegs;
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
    [SerializeField] private Image _resImage;

    [SerializeField] private Sprite[] _helmetImages;
    [SerializeField] private Sprite[] _armorImages;
    [SerializeField] private Sprite[] _bracersImages;
    [SerializeField] private Sprite[] _legsImages;
    [SerializeField] private Sprite[] _materialImages;

    [SerializeField] private TextMeshProUGUI _partName;
    [SerializeField] private TextMeshProUGUI _resCount;
    [SerializeField] private TextMeshProUGUI _statCount;

    [SerializeField] private GameObject _inventory;
    [SerializeField] private TextMeshProUGUI[] _resourceAmounts;
    [SerializeField] private Image[] _resourceImages;
    [SerializeField] private TextMeshProUGUI _newItemText;

    private List<GameObject> _craftBlocks = new List<GameObject>();

    private bool _isOpening = false;
    private bool _isCrafting = false;
    private GearType _curGearType;
    private GearQuality _curGearQuality;

    private GameObject _curCraftable;

    private BlockType[] _craftBlockTypes = { BlockType.wood, BlockType.stone, BlockType.ironOre, BlockType.gold, BlockType.diamond };

    private void Start()
    {
        _areas[0].transform.localScale = Vector3.zero;
        _areas[0].transform.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear).SetLoops(-1);

        _curGearType = GearType.helmet;
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
        if(Data.Instance.TutorialStep == 10)
            Tutorial.instance.ProgressTutorial();

        _areas[0].transform.DOScale(Vector3.one, 0.5f);
        _progressBar.SetActive(false);
        _progressBarFill.GetComponent<Image>().fillAmount = 0;
        _joystick.SetActive(false);
        _craftMenu.SetActive(true);
        _inventory.SetActive(true);
        _closeCraftMenuButton.onClick.AddListener(CloseCraftMenu);
        _weaponBackButton.onClick.AddListener(ChangeArmorBack);
        _weaponForwardButton.onClick.AddListener(ChangeArmorForward);
        _weaponMatBackButton.onClick.AddListener(ChangeMatBack);
        _weaponMatForwardButton.onClick.AddListener(ChangeMatForward);
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

    public void ChangeArmorForward()
    {
        if(_isCrafting)
            return;

        if(_curGearType == GearType.legs)
            _curGearType = GearType.helmet;
        else
            _curGearType++;

        SetCraftRecipy();
    }

    public void ChangeArmorBack()
    {
        if(_isCrafting)
            return;

        if(_curGearType == GearType.helmet)
            _curGearType = GearType.legs;
        else
            _curGearType--;

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

        if(_curGearType == GearType.helmet)
        {
            _resultImage.sprite = _helmetImages[(int)_curGearQuality - 1];
            _curCraftable = _craftableHelmets[(int)_curGearQuality - 1];
        }
        else if (_curGearType== GearType.armor)
        {
            _resultImage.sprite = _armorImages[(int)_curGearQuality - 1];
            _curCraftable = _craftableArmors[(int)_curGearQuality - 1];
        }
        else if(_curGearType==GearType.bracers)
        {
            _resultImage.sprite = _bracersImages[(int)_curGearQuality - 1];
            _curCraftable = _craftableBracers[(int)_curGearQuality - 1];
        }
        else
        {
            _resultImage.sprite = _legsImages[(int)_curGearQuality - 1];
            _curCraftable = _craftableLegs[(int)_curGearQuality - 1];
        }

        _resImage.sprite = _materialImages[(int)_curGearQuality - 1];

        _curCraftable.SetActive(true);
        _curCraftable.transform.localScale = Vector3.zero;
        _curCraftable.transform.DOScale(Vector3.one, 0.2f);

        _resCount.text = _curCraftable.GetComponent<CraftableTool>().BlockAmounts[0].ToString();

        _statCount.text = $"+{(int)_curGearQuality}";

        switch(_curGearType)
        {
            case GearType.helmet:
                _partName.text = "Helmet";
                break;
            case GearType.armor:
                _partName.text = "Armor";
                break;
            case GearType.bracers:
                _partName.text = "Bracers";
                break;
            case GearType.legs:
                _partName.text = "Legs";
                break;
            default:
                break;
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

        if(Data.Instance.TutorialStep == 11)
            Tutorial.instance.ProgressTutorial();
    }

    private IEnumerator CraftCoroutine()
    {
        Vector3 _startPos = _curCraftable.transform.localPosition;
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
                _craftBlocks.Add(_curblock);

                yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2 / _totalBlocks);
            }
        }

        _curCraftable.transform.localScale = Vector3.zero;
        RefreshInventory();

        yield return new WaitForSeconds(General.Instance.GameSettings.InventoryBlockCollectTime / 2);

        for(int i = 0; i < _craftBlocks.Count; i++)
        {
                _craftBlocks[i].transform.DOMove(_craftBlocks[i].transform.position + Vector3.down * 2, General.Instance.GameSettings.InventoryBlockCollectTime / 2);
                Destroy(_craftBlocks[i], General.Instance.GameSettings.InventoryBlockCollectTime / 2);
        }
        _craftBlocks.Clear();

        _curCraftable.transform.localPosition = _startPos;
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
        switch(_curCraftable.GetComponent<CraftableTool>().GearType)
        {
            case GearType.helmet:
                General.Instance.Player.GetComponent<PlayerHealth>().ChangeArmor(0, (int)_curCraftable.GetComponent<CraftableTool>().GearQuality);
                break;
            case GearType.armor:
                General.Instance.Player.GetComponent<PlayerHealth>().ChangeArmor(1, (int)_curCraftable.GetComponent<CraftableTool>().GearQuality);
                break;
            case GearType.bracers:
                General.Instance.Player.GetComponent<PlayerHealth>().ChangeArmor(2, (int)_curCraftable.GetComponent<CraftableTool>().GearQuality);
                break;
            case GearType.legs:
                General.Instance.Player.GetComponent<PlayerHealth>().ChangeArmor(3, (int)_curCraftable.GetComponent<CraftableTool>().GearQuality);
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