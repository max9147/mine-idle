using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private HealthBar _healthBar;
    [SerializeField] private GameObject _gameScreen;
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private GameObject _damageCanvas;
    [SerializeField] private GameObject[] _helmetSets;
    [SerializeField] private GameObject[] _armorSets;
    [SerializeField] private GameObject[] _bracersSets;
    [SerializeField] private GameObject[] _legsSets;

    private bool _isAlive = true;
    private float _curHealth;
    private float _maxHealth;
    private int[] _curArmors;
    private float[] _armorHealths;

    private List<GameObject[]> _allArmors;
    private Vector3 _startPos;

    private void Awake()
    {
        _allArmors = new List<GameObject[]>();
        _allArmors.Add(_helmetSets);
        _allArmors.Add(_armorSets);
        _allArmors.Add(_bracersSets);
        _allArmors.Add(_legsSets);
        _startPos = transform.position;
    }

    private void Start()
    {
        _maxHealth = General.Instance.GameSettings.PlayerMaxHealth;
        _curHealth = _maxHealth;

        _curArmors = Data.Instance.PlayerArmors;
        _armorHealths = Data.Instance.ArmorsHealth;
        for(int i = 0; i < _allArmors.Count; i++)
        {
            _allArmors[i][_curArmors[i]].SetActive(true);
        }
    }

    public void ChangeArmor(int _piece, int _quality)
    {
        _allArmors[_piece][_curArmors[_piece]].SetActive(false);
        _curArmors[_piece] = _quality;
        _allArmors[_piece][_curArmors[_piece]].SetActive(true);
        for(int i = 0; i < _armorHealths.Length; i++)
        {
            _armorHealths[i] = General.Instance.GameSettings.ArmorsHealth[_curArmors[i]];
        }
        Data.Instance.PlayerArmors = _curArmors;
        Data.Instance.ArmorsHealth = _armorHealths;
    }

    public void DamageArmor()
    {
        for(int i = 0; i < _curArmors.Length; i++)
        {
            if(_curArmors[i]!=0)
            {
                _armorHealths[i]--;

                if(_armorHealths[i] == 0)
                    ChangeArmor(i, 0);
            }
        }
        Data.Instance.ArmorsHealth = _armorHealths;
    }

    public void TakeDamage(float _damage)
    {
        foreach(var item in _curArmors)
        {
            _damage -= item;
        }

        Instantiate(_damageCanvas, transform.position + Vector3.up * 3, Quaternion.identity).GetComponent<DamageVisualizer>().SetupDamageText(true, (int)_damage);

        if (_damage > _curHealth)
            _damage = _curHealth;

        _curHealth -= _damage;
        _healthBar.SetHealth(_curHealth, _maxHealth);
        DamageArmor();

        if (_curHealth <= 0 && _isAlive)
        {
            _isAlive = false;

            _gameScreen.SetActive(false);
            _deathScreen.SetActive(true);

            GetComponent<PlayerInventory>().DropInventory();
            GetComponent<PlayerController>().StopAction();
        }
    }

    public void RevivePlayer()
    {
        _isAlive = true;

        _curHealth = _maxHealth;
        _healthBar.SetHealth(_curHealth, _maxHealth);

        _gameScreen.SetActive(true);
        _deathScreen.SetActive(false);

        transform.position = _startPos;
        GetComponent<PlayerCombat>().EnemiesNearby.Clear();
        GetComponent<PlayerController>().ResumeAction();
    }

    public void HealPlayer()
    {
        _curHealth = _maxHealth;
        _healthBar.SetHealth(_curHealth, _maxHealth);
    }

    public bool GetAliveStatus()
    {
        return _isAlive;
    }
}