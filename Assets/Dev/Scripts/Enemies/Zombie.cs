using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class Zombie : MonoBehaviour
{
    [SerializeField] private GameObject[] _zombieModels;

    private NavMeshAgent _agent;
    private GameObject _player;
    private GameObject _target;

    private Vector3 _randomOffset;

    private bool _isAttacking = false;
    private int _zombieModelID;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        _zombieModelID = Random.Range(0, _zombieModels.Length);
        _zombieModels[_zombieModelID].SetActive(true);
    }

    private void Start()
    {
        _agent.speed = General.Instance.GameSettings.ZombieMoveSpeed;
        _player = General.Instance.Player;
        StartCoroutine(ChooseTarget());
    }

    private void FixedUpdate()
    {
        if (!_target)
            return;

        if (_target.GetComponent<BuildingUnlocker>())
            _agent.SetDestination(_target.transform.position + _randomOffset);
        else
            _agent.SetDestination(_target.transform.position);

        if(_agent.remainingDistance <= General.Instance.GameSettings.ZombieAttackRange && _agent.remainingDistance != 0 && !_isAttacking)
                StartCoroutine(TryAttack());
    }

    public void Die()
    {
        _zombieModels[_zombieModelID].GetComponent<Animator>().SetInteger("state", 2);
        foreach(var part in _zombieModels[_zombieModelID].GetComponentsInChildren<Renderer>())
        {
            foreach(var item in part.materials)
            {
                item.DOColor(Color.red, 2.5f);
            }
        }
        GetComponent<CapsuleCollider>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        StopAllCoroutines();
        enabled = false;
    }

    private IEnumerator ChooseTarget()
    {
        if((Vector3.Distance(transform.position, _player.transform.position) <= General.Instance.GameSettings.ZombieVisionRange || ZombieHivemind.Instance.PossibleTargets.Count == 0) && _player.GetComponent<PlayerHealth>().GetAliveStatus())
            _target = _player;
        else
        {
            List<GameObject> _possibleTargets = ZombieHivemind.Instance.PossibleTargets;
            if(_possibleTargets.Count == 0)
                _target = null;
            else
            {
                _possibleTargets.Sort((x, y) => { return (transform.position - x.transform.position).sqrMagnitude.CompareTo((transform.position - y.transform.position).sqrMagnitude); });
                _target = _possibleTargets[0];
            }
        }

        yield return new WaitForSeconds(0.2f);

        StartCoroutine(ChooseTarget());
    }

    private IEnumerator TryAttack()
    {
        _agent.speed = 0;
        _isAttacking = true;
        _zombieModels[_zombieModelID].GetComponent<Animator>().SetInteger("state", 1);
        transform.DOLookAt(_target.transform.position, 0.2f);

        yield return new WaitForSeconds(General.Instance.GameSettings.ZombieAttackDelay / 2);

        if(_target && Vector3.Distance(transform.position, _target.transform.position) <= General.Instance.GameSettings.ZombieAttackRange * 1.5f)
        {
            if(_target.GetComponent<BuildingUnlocker>())
                _target.GetComponent<BuildingUnlocker>().BreakBuilding();

            if(_target.GetComponent<PlayerHealth>())
                _target.GetComponent<PlayerHealth>().TakeDamage(General.Instance.GameSettings.ZombieAttackDamage);

            if(_target.GetComponent<AllyHealth>())
                _target.GetComponent<AllyHealth>().TakeDamage(General.Instance.GameSettings.ZombieAttackDamage);
        }

        yield return new WaitForSeconds(General.Instance.GameSettings.ZombieAttackDelay / 2);

        _agent.speed = General.Instance.GameSettings.ZombieMoveSpeed;
        _isAttacking = false;
        _zombieModels[_zombieModelID].GetComponent<Animator>().SetInteger("state", 0);
    }
}