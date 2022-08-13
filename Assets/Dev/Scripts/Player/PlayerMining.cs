using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMining : MonoBehaviour
{
    [SerializeField] private GameObject _pickaxe;
    [SerializeField] private GameObject[] _pickaxes;

    private Animator _animator;
    private List<Block> _blocksToMine;

    private bool _isMining = false;
    private int _curPickaxe;
    private float _pickaxeHealth;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _blocksToMine = new List<Block>();
    }

    private void Start()
    {
        GetComponent<SphereCollider>().radius = General.Instance.GameSettings.PlayerMiningAttackingRange;

        _curPickaxe = Data.Instance.PlayerPickaxe;
        _pickaxes[_curPickaxe].SetActive(true);
        _pickaxeHealth = Data.Instance.PickaxeHealth;
    }

    private void FixedUpdate()
    {
        _pickaxe.SetActive(_isMining);

        if (!GetComponent<PlayerCombat>().GetAttackingStatus())
        {
            if (_blocksToMine.Count > 0 && !_blocksToMine[0].GetMiningStatus())
            {
                _blocksToMine.Sort((x, y) => { return (General.Instance.Player.transform.position - x.transform.position).sqrMagnitude.CompareTo((General.Instance.Player.transform.position - y.transform.position).sqrMagnitude); });
                _blocksToMine[0].StartMining(General.Instance.GameSettings.PlayerMiningStrength + (General.Instance.GameSettings.PlayerMiningStrengthBonusPerPickaxe * _curPickaxe));

                _isMining = true;
                _animator.SetInteger("state", 2);
                if(!GetComponent<PlayerController>().GetMovingStatus())
                    transform.DOLookAt(new Vector3(_blocksToMine[0].transform.position.x, 1, _blocksToMine[0].transform.position.z), 0.1f);
            }
        }
        else
        {
            foreach (var item in _blocksToMine)
                item.StopMining();

            _isMining = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Block>())
            _blocksToMine.Add(other.GetComponent<Block>());
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<Block>())
        {
            _blocksToMine.Remove(other.GetComponent<Block>());
            other.GetComponent<Block>().StopMining();
            if(_blocksToMine.Count == 0)
                _isMining = false;
        }
    }

    public void ChangePickaxe(int _id)
    {
        _pickaxes[_curPickaxe].SetActive(false);
        _curPickaxe = _id;
        _pickaxes[_curPickaxe].SetActive(true);
        _pickaxeHealth = General.Instance.GameSettings.PickaxesHealth[_curPickaxe];
        Data.Instance.PlayerPickaxe = _curPickaxe;
        Data.Instance.PickaxeHealth = _pickaxeHealth;
    }

    public void RemoveBlock(Block _blockToRemove)
    {
        _blocksToMine.Remove(_blockToRemove);

        if (_blocksToMine.Count == 0)
            _isMining = false;
    }

    public void DamagePickaxe()
    {
        if(_curPickaxe != 0)
        {
            _pickaxeHealth--;
            Data.Instance.PickaxeHealth = _pickaxeHealth;

            if(_pickaxeHealth == 0)
                ChangePickaxe(0);
        }
    }

    public bool GetMiningStatus()
    {
        if (!GetComponent<PlayerController>().GetMovingStatus() && _blocksToMine.Count > 0 && !_blocksToMine[0].GetMiningStatus())
            _isMining = true;

        return _isMining;
    }
}