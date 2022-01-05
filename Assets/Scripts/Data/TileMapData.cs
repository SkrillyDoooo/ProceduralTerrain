using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Warpath Terrain Data/Texture")]
public class TileMapData : UpdateableData
{

    public TileMapEntry[] tileMapPallette;
    Dictionary<TerrainGridGenerator.TerrainCellType, TileBase> tileMapDictionary;

    public TileBase[] GenerateChunk(TerrainGrid gridChunk)
    {
        if(tileMapDictionary == null)
        {
            tileMapDictionary = new Dictionary<TerrainGridGenerator.TerrainCellType, TileBase>();
            foreach(var entry in tileMapPallette)
            {
                tileMapDictionary.Add(entry.type, entry.tileBase);
            }
        }

        var values = gridChunk.values;
        int width = values.GetLength(0);
        int height = values.GetLength(1);

        TileBase[] tileMap = new TileBase[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < height; x++)
            {
                tileMap[y * width + x] = tileMapDictionary[values[x,y]];
            }
        }
        return tileMap;
    }

    [System.Serializable]
    public class TileMapEntry
    {
        public TileBase tileBase;
        public TerrainGridGenerator.TerrainCellType type;
    }
}
