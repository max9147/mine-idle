using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryPriceToggle : MonoBehaviour
{
    [SerializeField] private GameObject[] _islands;

    private List<GameObject> _territoriesLocked;
    private List<GameObject> _territoriesUnlocked;

    public static TerritoryPriceToggle instance;

    private void Awake()
    {
        _territoriesLocked = new List<GameObject>();
        _territoriesUnlocked = new List<GameObject>();
        instance = this;
    }

    private void Start()
    {
        Invoke(nameof(CheckTerritories), 0.05f);
    }

    public void CheckTerritories()
    {
        _territoriesLocked.Clear();
        _territoriesUnlocked.Clear();

        foreach(var item in _islands)
        {
            if(Data.Instance.TerritoriesUnlocked[item.GetComponent<TerritoryUnlocker>().GetID()])
            {
                _territoriesUnlocked.Add(item);
            }
            else
            {
                _territoriesLocked.Add(item);
            }
        }

        foreach(var itemLocked in _territoriesLocked)
        {
            bool _hide = true;
            foreach(var itemUnlocked in _territoriesUnlocked)
            {
                if(Vector3.Distance(itemLocked.transform.position, itemUnlocked.transform.position) < 7f || itemLocked.name=="Grass_Plane_1_Bridge")
                {
                    _hide = false;
                }
            }
            itemLocked.SetActive(!_hide);
        }
    }
}