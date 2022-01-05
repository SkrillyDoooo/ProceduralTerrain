using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Warpath Terrain Data/Terrain Grid Settings")]
public class TerrainGridSettings : UpdateableData
{
    public int dimensions;

    public float terrainWorldSize = 10;
    
    [Range(0, 1)]
    public float passableHeightMin = 0.1f;

    [Range(0, 1)]
    public float passableHeightMax = 1.0f;

#if UNITY_EDITR
    protected override void OnValidate()
    {
        base.OnValidate();
        noiseSettings.ValidateValues();
    }
#endif
}
