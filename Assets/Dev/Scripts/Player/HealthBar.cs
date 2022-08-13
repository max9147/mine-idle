using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Transform _fill;

    private void Update()
    {
        transform.eulerAngles = new Vector3(30f, -45f, 0);
    }

    public void SetHealth(float _curHealth, float _maxHealth)
    {
        _fill.localScale = new Vector3(_curHealth / _maxHealth, 1, 1);
    }
}