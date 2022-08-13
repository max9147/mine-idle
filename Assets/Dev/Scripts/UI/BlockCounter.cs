using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlockCounter : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private TextMeshProUGUI _counter;

    private float _capacity;

    private void Awake()
    {
        _playerInventory.OnChange.AddListener(RefreshBlockCounter);
    }

    private void Start()
    {
        _capacity = General.Instance.GameSettings.InventoryCapacity;
    }

    public void RefreshBlockCounter()
    {
        _counter.text = General.Instance.Player.GetComponent<PlayerInventory>().GetBlocksCount() + "/" + _capacity;
        _counter.transform.DOScale(Vector3.one * 3f, 0.15f);
        _counter.transform.DOScale(Vector3.one, 0.15f);
        _fill.fillAmount = General.Instance.Player.GetComponent<PlayerInventory>().GetBlocksCount() / _capacity;
    }
}