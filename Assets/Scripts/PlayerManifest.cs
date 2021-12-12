using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManifest 
{
    private static PlayerManifest m_Instance;

    public static PlayerManifest Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new PlayerManifest();
            return m_Instance;
        }
    }

    private List<Transform> units;
    private List<Transform> buildings;


    public PlayerManifest()
    {
        units = new List<Transform>();
        buildings = new List<Transform>();
    }

    public void AddBuilding(Transform t)
    {
        buildings.Add(t);
    }

    public void RemoveBuilding(Transform t)
    {
        buildings.Remove(t);
    }

    public void AddUnit(Transform t)
    {
        units.Add(t);
    }

    public void RemoveUnit(Transform t)
    {
        units.Remove(t);
    }

    public List<Transform> GetUnitManifest()
    {
        return units;
    }

    public List<Transform> GetBuildingManifest()
    {
        return buildings;
    }
}
