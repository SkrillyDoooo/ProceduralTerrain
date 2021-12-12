using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NavMapGenerator
{
    public static NavMap GenerateNavMap(float[,] heightMap, float maxHeight, NavMapSettings navMapSettings, Vector2Int chunkCoord)
    {
        int mapDimension = heightMap.GetLength(0);
        int navMapDimension = (mapDimension - 1) / navMapSettings.skipIncrement + 1;

        bool[,] navMapValues = new bool[navMapDimension, navMapDimension];

        for (int x = 0; x < mapDimension; x += navMapSettings.skipIncrement)
        {
            for(int y = 0; y < mapDimension; y+= navMapSettings.skipIncrement)
            {
                navMapValues[x/navMapSettings.skipIncrement, y/navMapSettings.skipIncrement] = (heightMap[x, y] > maxHeight * navMapSettings.passableHeightMin && heightMap[x,y] < maxHeight * navMapSettings.passableHeightMax);
            }
        }

        return new NavMap(navMapValues, navMapDimension, chunkCoord);
    }
}


public struct NavMap
{
    public readonly bool[,] values;
    public readonly int dimension;
    public MinimalAbstractPathFindingTree abstractPathFindingTree;

    public NavMap(bool[,] values, int dimension, Vector2Int chunkcoord)
    {
        this.values = values;
        this.dimension = dimension;
        abstractPathFindingTree = new MinimalAbstractPathFindingTree(values, chunkcoord);
    }
}

