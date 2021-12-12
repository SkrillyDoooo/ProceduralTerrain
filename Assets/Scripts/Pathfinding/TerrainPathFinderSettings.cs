using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Warpath Unit Data/TerrainPathFindingSettings")]
public class TerrainPathFinderSettings : ScriptableObject
{
    [Range(0,1)]
    public float maxTraversableHeight;

    [Range(0, 1)]
    public float minTraversableHeight;
}
