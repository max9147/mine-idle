using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ally : MonoBehaviour
{
    [SerializeField] private GameObject _weapon;

    private NavMeshAgent _agent;
    private GameObject _target;

    private bool _isAttacking = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _agent.speed = General.Instance.GameSettings.AllyMoveSpeed;
        StartCoroutine(ChooseTarget());
    }

    private void FixedUpdate()
    {
        _weapon.SetActive(_isAttacking);
        
        if (_target)
        {
            if (_agent.remainingDistance <= General.Instance.GameSettings.AllyAttackRange && _agent.remainingDistance != 0 && !_isAttacking)
                StartCoroutine(TryAttack());

            _agent.SetDestination(_target.transform.position);

            if(!_isAttacking)
                GetComponent<Animator>().SetInteger("state", 1);
        }
        else
            GetComponent<Animator>().SetInteger("state", 0);
            
    }

    public void Die()
    {
        GetComponent<Animator>().SetInteger("state", 3);
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        StopAllCoroutines();
        enabled = false;
    }

    private IEnumerator ChooseTarget()
    {
        List<GameObject> _possibleTargets = AllyHivemind.Instance.PossibleTargets;
        if (_possibleTargets.Count == 0)
            _target = null;
        else
        {
            _possibleTargets.Sort((x, y) => { return (transform.position - x.transform.position).sqrMagnitude.CompareTo((transform.position - y.transform.position).sqrMagnitude); });
            _target = _possibleTargets[0];
        }
        yield return new WaitForSeconds(0.2f);

        StartCoroutine(ChooseTarget());
    }

    private IEnumerator TryAttack()
    {
        _agent.speed = 0;
        _isAttacking = true;
        GetComponent<Animator>().SetInteger("state", 2);
        transform.DOLookAt(_target.transform.position, 0.2f);

        yield return new WaitForSeconds(General.Instance.GameSettings.AllyAttackDelay / 2);

        if (_target && Vector3.Distance(transform.position, _target.transform.position) <= General.Instance.GameSettings.AllyAttackRange * 1.5f)
        {
            if (_target && _target.GetComponent<ZombieHealth>())
                _target.GetComponent<ZombieHealth>().TakeDamage(General.Instance.GameSettings.ZombieAttackDamage);
        }

        yield return new WaitForSeconds(General.Instance.GameSettings.AllyAttackDelay / 2);

        _agent.speed = General.Instance.GameSettings.AllyMoveSpeed;
        _isAttacking = false;
    }
}