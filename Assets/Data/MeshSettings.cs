using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdateableData
{

    public const int numSupportedLOD = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;

    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 5.0f;
    public bool useFlatShading;

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatShadedChunkSizes - 1)]
    public int chunkSizeFlatShadedIndex;

    // num verts per line of a mesh rendered at the highest resolution. LOD = 0. Includes border vertices which are not a part of the final mesh but used for calculating normals
    public int numberOfVerticiesPerLine
    {
        get
        {
            return supportedChunkSizes[(useFlatShading) ? chunkSizeFlatShadedIndex : chunkSizeIndex] + 1;
        }
    }

    public float meshWorldSize
    {
        get
        {
            return (numberOfVerticiesPerLine - 3) * meshScale;
        }
    }
}
