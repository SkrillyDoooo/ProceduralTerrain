using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainGridGenerator
{

    public enum TerrainCellType
    {
        None = 0,
        Water = 1,
        Land = 2,
        Mountain = 4,
        Woods = 8,
    }


    public static Dictionary<TerrainCellType, Color> colorMap = new Dictionary<TerrainCellType, Color>()
    {
        { TerrainCellType.Land, Color.green },
        { TerrainCellType.Mountain, Color.grey },
        { TerrainCellType.Water, Color.blue },
        { TerrainCellType.Woods, new Color(150, 75, 0) /*brown*/}
    };

    public static TerrainGrid GenerateTerrainGridFromHeightMap(float[,] heightMap, TerrainGridSettings gridSettings, float maxHeight)
    {
        int mapDimension = heightMap.GetLength(0);

        TerrainCellType[,] terrainMapValues = new TerrainCellType[mapDimension, mapDimension];

        for (int x = 0; x < mapDimension; x++)
        {
            for(int y = 0; y < mapDimension; y++)
            {
                terrainMapValues[x, y] = GetCellType(heightMap[x,y], maxHeight, gridSettings);
            }
        }

        return new TerrainGrid(terrainMapValues);
    }

    private static TerrainCellType GetCellType(float noiseValue, float maxHeight, TerrainGridSettings terrainGridSettings)
    {
        if(noiseValue <  maxHeight * terrainGridSettings.passableHeightMin)
        {
            return TerrainCellType.Water;
        }
        else if(noiseValue > maxHeight * terrainGridSettings.passableHeightMax)
        {
            return TerrainCellType.Mountain;
        }
        else
        {
            return TerrainCellType.Land;
        }
    }
}


    public struct TerrainGrid
    {
        public readonly TerrainGridGenerator.TerrainCellType[,] values;
        public TerrainGrid(TerrainGridGenerator.TerrainCellType[,] values)
        {
            this.values = values;
        }
    }