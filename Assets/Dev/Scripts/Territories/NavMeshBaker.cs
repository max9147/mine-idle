using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    public static NavMeshBaker instance;

    [SerializeField] private NavMeshSurface surface;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        RebuildNavMesh();
    }

    public void RebuildNavMesh()
    {
        StartCoroutine(InvokeRebuildNavMesh());
    }

    private IEnumerator InvokeRebuildNavMesh()
    {
        yield return new WaitForSeconds(1f);

        surface.BuildNavMesh();
    }
}