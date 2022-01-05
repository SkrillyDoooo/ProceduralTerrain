using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NavMapChunk
{
    public NavigationNode rootNode;
    public int levels;

    public NavMapChunk(TerrainGridGenerator.TerrainCellType[,] values, int dimension, Vector2Int chunkcoord, NavigationNodePool navNodePool)
    {
        int maxLevel = (int)Mathf.Log(dimension, 2) + 1;
        levels = maxLevel;
        rootNode = NavigationNode.BuildTree(values, dimension, chunkcoord, navNodePool, maxLevel);
    }
}

