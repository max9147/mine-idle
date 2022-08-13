using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [SerializeField] private HealthBar _healthBar;
    [SerializeField] private GameObject _damageCanvas;

    private float _curHealth;
    private float _maxHealth;

    private void Start()
    {
        _maxHealth = General.Instance.GameSettings.ZombieMaxHealth;
        _curHealth = _maxHealth;
    }

    public void TakeDamage(float _damage)
    {
        Instantiate(_damageCanvas, transform.position + Vector3.up * 3, Quaternion.identity).GetComponent<DamageVisualizer>().SetupDamageText(false, (int)_damage);

        if (_damage > _curHealth)
            _damage = _curHealth;

        _curHealth -= _damage;
        _healthBar.SetHealth(_curHealth, _maxHealth);

        if (_curHealth <= 0)
        {
            ZombieHivemind.Instance.CurZombiesAmount--;
            AllyHivemind.Instance.PossibleTargets.Remove(gameObject);
            General.Instance.Player.GetComponent<PlayerCombat>().EnemiesNearby.Remove(gameObject);
            StartCoroutine(Dying());
        }
    }

    private IEnumerator Dying()
    {
        GetComponent<Zombie>().Die();

        yield return new WaitForSeconds(2.3f);

        transform.DOScale(Vector3.zero, 0.2f);

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }
}