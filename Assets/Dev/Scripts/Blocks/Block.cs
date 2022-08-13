using DG.Tweening;
using Structs;
using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private BlockType _blockType;

    [SerializeField] private GameObject[] _crackStates;
    [SerializeField] private GameObject _destroyFX;
    [SerializeField] private GameObject _blockMark;

    private bool _beingMined = false;
    private float _startHealth;
    private float _curHealth;
    private float _miningDamage;
    private float _crackThreshold;
    private int _crackStep = 0;
    private GameObject _curMark;
    private Vector3 _startPos;
    private Vector3 _startScale;

    private void Start()
    {
        switch(_blockType)
        {
            case BlockType.dirt:
                _startHealth = General.Instance.GameSettings.DirtHealth;
                break;
            case BlockType.grass:
                _startHealth = General.Instance.GameSettings.GrassHealth;
                break;
            case BlockType.wood:
                _startHealth = General.Instance.GameSettings.WoodHealth;
                break;
            case BlockType.tree:
                _startHealth = General.Instance.GameSettings.TreeHealth;
                break;
            case BlockType.stone:
                _startHealth = General.Instance.GameSettings.StoneHealth;
                break;
            case BlockType.ironOre:
                _startHealth = General.Instance.GameSettings.IronOreHealth;
                break;
            default:
                break;
        }

        _curHealth = _startHealth;
        _crackThreshold = _startHealth / _crackStates.Length;
    }

    private void FixedUpdate()
    {
        if(_beingMined)
        {
            _curHealth -= _miningDamage * Time.fixedDeltaTime;

            _crackThreshold -= _miningDamage * Time.fixedDeltaTime;

            if(_curHealth <= 0)
            {
                DestroyBlock();
                return;
            }

            if(_crackThreshold <= 0)
            {
                _crackThreshold = _startHealth / _crackStates.Length;
                _crackStates[_crackStep].SetActive(false);
                _crackStep++;
                _crackStates[_crackStep].SetActive(true);

                Instantiate(_destroyFX, transform.position, Quaternion.identity);

                if(_blockType == BlockType.tree)
                {
                    General.Instance.Player.GetComponent<PlayerInventory>().AddBlock(_blockType, transform.position);
                    if(Data.Instance.TutorialStep == 4 || Data.Instance.TutorialStep == 7)
                        Tutorial.instance.LowerArrow();
                }
            }
        }
    }

    private void DestroyBlock()
    {
        _beingMined = false;
        Destroy(_curMark);
        GetComponent<BoxCollider>().enabled = false;

        _startPos = transform.position;
        _startScale = transform.localScale;

        General.Instance.Player.GetComponent<PlayerMining>().RemoveBlock(this);
        General.Instance.Player.GetComponent<PlayerMining>().DamagePickaxe();
        General.Instance.Player.GetComponent<PlayerInventory>().AddBlock(_blockType, transform.position);

        transform.DOScale(Vector3.zero, General.Instance.GameSettings.BlockFadeOutTime);
        StartCoroutine(RespawnBlock());

        Instantiate(_destroyFX, transform.position, Quaternion.identity);

        if(Data.Instance.TutorialStep <= 3 && (_blockType == BlockType.grass || _blockType == BlockType.dirt))
            Tutorial.instance.ProgressTutorial();
        if((Data.Instance.TutorialStep == 4 || Data.Instance.TutorialStep == 7) && _blockType == BlockType.tree)
            Tutorial.instance.ProgressTutorial();
    }

    public void StartMining(float _damage)
    {
        _beingMined = true;
        _curMark = Instantiate(_blockMark, transform.position, transform.rotation, transform);
        _miningDamage = _damage;
    }

    public void StopMining()
    {
        _beingMined = false;
        Destroy(_curMark);

        if(_curHealth < _startHealth / _crackStates.Length)
        {
            _curHealth = _startHealth / _crackStates.Length;
            _crackThreshold = _curHealth;
        }
    }

    public bool GetMiningStatus()
    {
        return _beingMined;
    }

    private IEnumerator RespawnBlock()
    {
        yield return new WaitForSeconds(General.Instance.GameSettings.BlockRespawnTime);

        foreach(GameObject item in _crackStates)
        {
            item.SetActive(false);
        }

        _crackStates[0].SetActive(true);
        _curHealth = _startHealth;
        _crackThreshold = _startHealth / _crackStates.Length;
        _crackStep = 0;
        GetComponent<BoxCollider>().enabled = true;
        transform.position = _startPos;
        transform.DOScale(_startScale, 0.2f);
    }
}