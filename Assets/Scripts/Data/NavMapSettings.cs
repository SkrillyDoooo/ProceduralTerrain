using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Warpath Terrain Data/Navigation")]
public class NavMapSettings : UpdateableData
{
    [Range(0, 16)]
    public int LOD;

    [Range(0, 1)]
    public float passableHeightMin = 0.1f;

    [Range(0, 1)]
    public float passableHeightMax = 1.0f;

    public int skipIncrement
    {
        get
        {
            return (LOD == 0) ? 1 : LOD * 2;
        }
    }
}
