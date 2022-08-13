using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageVisualizer : MonoBehaviour
{
    private void Start()
    {
        transform.eulerAngles = new Vector3(30f, -45f, 0);
        StartCoroutine(DamageMovement());
    }

    public void SetupDamageText(bool _isFriendly, int _damageAmount)
    {
        if (!_isFriendly)
            GetComponent<TextMeshPro>().color = Color.red;
        GetComponent<TextMeshPro>().text = _damageAmount.ToString();
    }

    private IEnumerator DamageMovement()
    {
        transform.DOMove(transform.position + new Vector3(Random.Range(-1, 1), 2, Random.Range(-1, 1)), 1f);

        yield return new WaitForSeconds(0.8f);

        transform.DOScale(Vector3.zero, 0.2f);

        yield return new WaitForSeconds(0.2f);

        Destroy(gameObject);
    }
}