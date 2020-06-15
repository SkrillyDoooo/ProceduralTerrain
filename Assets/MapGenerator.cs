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
        MeshAndColor,
        Falloff,
    }

    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;


    [Range(0, MeshSettings.numSupportedLOD - 1)]
    public int editorPreviewLevelOfDetail;

    public bool autoUpdate = true;

    public DrawMode drawMode;

    public ComputeShader shader;
    public Material terrainMaterial;

    float[,] falloffMap;

    Queue<MapThreadInfo<HeightMap>> heightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void OnValuesUpdated()
    {
        if(!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }

    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticiesPerLine, meshSettings.numberOfVerticiesPerLine, heightMapSettings, Vector2.zero);

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();

        switch (drawMode)
        {
            case DrawMode.Noise:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values), true);
                break;
            case DrawMode.MeshAndColor:
                mapDisplay.DrawMesh(MapMeshGenerator.GenerateMesh(heightMap.values, meshSettings, editorPreviewLevelOfDetail));
                break;
            case DrawMode.Falloff:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numberOfVerticiesPerLine)), true);
                break;

        }
    }

    public void RequestHeightMap(Vector2 center, Action<HeightMap> callback)
    {
        ThreadStart threadStart = delegate
        {
            HeightMapThread(center, callback);
        };

        new Thread(threadStart).Start();
    }
    
    void HeightMapThread(Vector2 center, Action<HeightMap> callback)
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticiesPerLine, meshSettings.numberOfVerticiesPerLine, heightMapSettings, center);
        lock (heightMapThreadInfoQueue)
        {
            heightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
        }
    }

    // two different approached to threading / async here. above in RequestheightMap we dispatch to another thread. here generate mesh handles the callback 
    // and employs an async gpu read to get data from the compute shader
    public void RequestMeshData(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(heightMap, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callbak)
    {
        MeshData meshData = MapMeshGenerator.GenerateMesh(heightMap.values, meshSettings, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callbak, meshData));
        }

    }

    private void Update()
    {
        if(heightMapThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < heightMapThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<HeightMap> threadInfo = heightMapThreadInfoQueue.Dequeue();
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

    private void OnValidate()
    {
        if(meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if(textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
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
