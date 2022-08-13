using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    public Vector3 DefaultPos;
    public Vector3 DefaultScale;

    private void Start()
    {
        DefaultPos = transform.position;
        DefaultScale = transform.localScale;
    }
}