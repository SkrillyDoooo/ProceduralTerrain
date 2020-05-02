using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode
    {
        Noise,
        Color,
        MeshAndColor
    }


    public float noiseScale = 1;
    [Range(1, 10)]
    public int octaves = 1;
    [Range(0, 10)]
    public float persistance = 1;
    [Range(0, 10)]
    public float lacunarity = 1;

    public int seed = 1;
    public Vector2 offset = Vector2.zero;

    const int mapChunkSize = 241;

    [Range(0,6)]
    public int levelOfDetail;

    public bool autoUpdate = true;

    public TerrainType[] regions;

    public DrawMode drawMode;

    public ComputeShader shader;
    public float meshHeightMultiplier;
    public AnimationCurve heightMapCurve;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay mapDisplay = GetComponent<MapDisplay>();
        if (mapDisplay == null)
            Debug.LogError("MapDisplay component not found.");

        switch(drawMode)
        {
            case DrawMode.Noise:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap), true);
                break;
            case DrawMode.Color:

                mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(ConvertHeightMapToColorMap(noiseMap), mapChunkSize, mapChunkSize), true);
                break;
            case DrawMode.MeshAndColor:
                Color[] color = ConvertHeightMapToColorMap(noiseMap);
                mapDisplay.DrawMesh(MapMeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, heightMapCurve, levelOfDetail,  shader), TextureGenerator.TextureFromColorMap(ConvertHeightMapToColorMap(noiseMap), mapChunkSize, mapChunkSize));
                break;

        }
    }

    private Color[] ConvertHeightMapToColorMap(float [,] noiseMap)
    {
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        return colorMap;
    }

    private void OnValidate()
    {
        if(lacunarity < 1)
        {
            lacunarity = 1f;
        }
        if(octaves < 0)
        {
            octaves = 0;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
