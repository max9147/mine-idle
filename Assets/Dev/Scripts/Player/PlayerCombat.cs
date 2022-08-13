using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameObject _weapon;
    [SerializeField] private GameObject[] _swords;

    public List<GameObject> EnemiesNearby;

    private Animator _animator;

    private bool _isAttacking = false;
    private bool _canAttack = true;
    private int _curSword;
    private float _swordHealth;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _curSword = Data.Instance.PlayerSword;
        _swords[_curSword].SetActive(true);
        _swordHealth = Data.Instance.SwordHealth;
    }

    private void FixedUpdate()
    {
        _weapon.SetActive(!_canAttack);

        if (EnemiesNearby.Count == 0)
            return;
        else if (_canAttack)
            StartCoroutine(TryAttack());
        else if (!GetComponent<PlayerController>().GetMovingStatus())
            transform.DOLookAt(new Vector3(EnemiesNearby[0].transform.position.x, 1, EnemiesNearby[0].transform.position.z), 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Zombie>())
            EnemiesNearby.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Zombie>())
            EnemiesNearby.Remove(other.gameObject);
    }

    public void ChangeSword(int _id)
    {
        _swords[_curSword].SetActive(false);
        _curSword = _id;
        _swords[_curSword].SetActive(true);
        _swordHealth = General.Instance.GameSettings.SwordsHealth[_curSword];
        Data.Instance.PlayerSword = _curSword;
        Data.Instance.SwordHealth = _swordHealth;
    }

    public void DamageSword()
    {
        if(_curSword != 0)
        {
            _swordHealth--;
            Data.Instance.SwordHealth = _swordHealth;

            if(_swordHealth == 0)
                ChangeSword(0);
        }
    }

    public bool GetAttackingStatus()
    {
        return _isAttacking;
    }

    private IEnumerator TryAttack()
    {
        _isAttacking = true;
        _canAttack = false;
        _animator.SetInteger("state", 3);

        yield return new WaitForSeconds(General.Instance.GameSettings.PlayerAttackDelay / 3);

        for (int i = 0; i < EnemiesNearby.Count; i++)
        {
            if (EnemiesNearby[i] != null)
            {
                EnemiesNearby[i].GetComponent<Rigidbody>().AddForce((EnemiesNearby[i].transform.position - transform.position) * General.Instance.GameSettings.PlayerAttackKnockbackStrength, ForceMode.Impulse);
                EnemiesNearby[i].GetComponent<ZombieHealth>().TakeDamage(General.Instance.GameSettings.PlayerAttackDamage + (General.Instance.GameSettings.PlayerAttackDamageBonusPerSword * _curSword));
                DamageSword();
            }
        }

        _isAttacking = false;

        yield return new WaitForSeconds(General.Instance.GameSettings.PlayerAttackDelay / 3 * 2);

        _canAttack = true;
    }
}