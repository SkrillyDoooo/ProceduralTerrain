using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

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

    public const int mapChunkSize = 241;

    [Range(0,6)]
    public int editorPreviewLevelOfDetail;

    public bool autoUpdate = true;

    public TerrainType[] regions;

    public DrawMode drawMode;

    public ComputeShader shader;
    public float meshHeightMultiplier;
    public AnimationCurve heightMapCurve;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
 
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        MapDisplay mapDisplay = GetComponent<MapDisplay>();
        if (mapDisplay == null)
            Debug.LogError("MapDisplay component not found.");

        switch (drawMode)
        {
            case DrawMode.Noise:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap), true);
                break;
            case DrawMode.Color:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize), true);
                break;
            case DrawMode.MeshAndColor:
                mapDisplay.DrawMesh(MapMeshGenerator.GenerateMesh(mapData.heightMap, meshHeightMultiplier, heightMapCurve, editorPreviewLevelOfDetail, shader), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                break;

        }
    }

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }
    
    void MapDataThread(Action<MapData> callback)
    {
        MapData mapData = GenerateMapData();
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    // two different approached to threading / async here. above in RequestMapData we dispatch to another thread. here generate mesh handles the callback 
    // and employs an async gpu read to get data from the compute shader
    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        MapMeshGenerator.GenerateMeshAsync(mapData.heightMap, meshHeightMultiplier, heightMapCurve, lod, shader, callback);
    }

    private void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

    }

    private MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        Color[] colorMap = ConvertHeightMapToColorMap(noiseMap);
        return new MapData(noiseMap, colorMap);
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

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
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

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMaps, Color[] colorMap)
    {
        this.heightMap = heightMaps;
        this.colorMap = colorMap;
    }
}
