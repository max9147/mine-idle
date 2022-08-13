using DG.Tweening;
using Structs;
using System.Collections;
using UnityEngine;

public class BlockVisual : MonoBehaviour
{
    [SerializeField] private BlockType _blockType;
    private GameSettings _gameSettings;

    private void Awake()
    {
        _gameSettings = General.Instance.GameSettings;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<PlayerController>() && other.gameObject.GetComponent<PlayerHealth>().GetAliveStatus())
        {
            other.gameObject.GetComponent<PlayerInventory>().AddBlock(_blockType, transform.position);
            Destroy(gameObject);
        }
    }

    public void DoMove(Vector3 _targetPos, Transform _inventoryPivot)
    {
        StartCoroutine(MoveBlock(_targetPos, _inventoryPivot));
    }

    private IEnumerator MoveBlock(Vector3 _targetPos, Transform _inventoryPivot)
    {
        transform.DOScale(Vector3.one, _gameSettings.InventoryBlockCollectTime);
        transform.DOLocalJump(_inventoryPivot.position + Vector3.up * 2, _gameSettings.InventoryCollectedJumpHeight, 1, _gameSettings.InventoryBlockCollectTime / 2).SetEase(Ease.Linear);

        yield return new WaitForSeconds(_gameSettings.InventoryBlockCollectTime / 2);

        transform.parent = _inventoryPivot;
        transform.DOLocalRotate(Vector3.zero, _gameSettings.InventoryBlockCollectTime / 2);
        transform.DOLocalMove(_targetPos, _gameSettings.InventoryBlockCollectTime / 2).SetEase(Ease.Linear);
    }
}