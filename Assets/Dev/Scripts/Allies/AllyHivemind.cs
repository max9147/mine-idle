using System.Collections.Generic;
using UnityEngine;

public class AllyHivemind : MonoBehaviour
{
    public static AllyHivemind Instance;

    public List<GameObject> PossibleTargets;

    public int CurAlliesAmount = 0;

    private void Awake()
    {
        Instance = this;
    }
}