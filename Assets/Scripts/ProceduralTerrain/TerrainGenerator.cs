using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{

    public GameObject TerrainTexturePrefab;

    const float viewerMoveThresholdForChunkUpdate = 5f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;

    public Transform viewer;
    public Material terrainMaterial;

    public TerrainGridSettings terrainGridSettings;
    public HeightMapSettings heightSettings;
    public NavMapPreview navMapPreview;

    Vector2 viewerPos;
    Vector2 viewerPositionOld;

    float terrainWorldSize;
    int chunksVisibleInViewDist;
    public int colliderLODIndex;
    HashSet<Vector2> updatedChunkCoords;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private static TerrainGenerator m_Instance;

    NavigationNodePool navigationNodePool;


    public float MaxHeight
    {
        get
        {
            return heightSettings.maxHeight;
        }
    }

    public static TerrainGenerator Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = FindObjectOfType<TerrainGenerator>();
            return m_Instance;
        }
    }

    public static GameObject m_ParentObjectEditorOnly;

    void Start()
    {
        navigationNodePool = new NavigationNodePool();
        int dimension = terrainGridSettings.dimensions;
        int maxLevel = (int)Mathf.Log(dimension, 2) + 1;
        Debug.Log("maxLevel: " + maxLevel);
        navMapPreview.SetNavMap(navigationNodePool,terrainGridSettings.terrainWorldSize, maxLevel, dimension);


        terrainWorldSize = terrainGridSettings.terrainWorldSize;
        m_ParentObjectEditorOnly = new GameObject("Map Generator (Editor Only)");
        m_ParentObjectEditorOnly.transform.position = Vector3.zero;

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / terrainWorldSize);

        updatedChunkCoords = new HashSet<Vector2>();
        UpdateVisibleChunks();
    }

    // Update is called once per frame
    void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        if((viewerPositionOld - viewerPos).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPos;
            UpdateVisibleChunks();
        }
    }

    public bool TryGetNavMapAtCoordinate(Vector2 coord, out NavMapChunk nav)
    {
        nav = default;
        return terrainChunkDictionary.TryGetValue(coord, out TerrainChunk chunk) && chunk.TryGetNavMap(out nav);
    }


    bool TryGetTerrainChunkClosestToPoint(Vector2 point, out TerrainChunk terrainChunk)
    {
        int currentChunkCoordX = Mathf.RoundToInt(point.x / terrainWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(point.y / terrainWorldSize);
        return terrainChunkDictionary.TryGetValue(new Vector2(currentChunkCoordX, currentChunkCoordY), out terrainChunk);
    }

    public float GetHeightAtCoord(Vector2 point)
    {
        if(TryGetTerrainChunkClosestToPoint(point, out var chunk))
        {
            return chunk.GetHeightAtCoord(point);
        }
        Debug.Log("Could not get height at point because the corresponding TerrainChunk does not exist");
        return 0.0f;
    }

    void UpdateVisibleChunks()
    {
        updatedChunkCoords.Clear();
        for(int i = visibleTerrainChunks.Count - 1; i >= 0  ; i--)
        {
            updatedChunkCoords.Add(visibleTerrainChunks[i].coordinate);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / terrainWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / terrainWorldSize);

        for(int yOffset = -chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDist; xOffset <= chunksVisibleInViewDist; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (updatedChunkCoords.Contains(viewedChunkCoord))
                    continue;

                if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    TerrainChunk chunk = terrainChunkDictionary[viewedChunkCoord];
                    chunk.UpdateTerrainChunk();
                }
                else
                {
                    TerrainChunk terrainChunk = new TerrainChunk(viewedChunkCoord, heightSettings, terrainGridSettings, detailLevels, colliderLODIndex, terrainMaterial, viewer, m_ParentObjectEditorOnly.transform, navigationNodePool, TerrainTexturePrefab);
                    terrainChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                    terrainChunkDictionary.Add(viewedChunkCoord, terrainChunk);
                    terrainChunk.Load();
                }
            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
            visibleTerrainChunks.Add(chunk);
        else
            visibleTerrainChunks.Remove(chunk);
    }
}

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numSupportedLOD - 1)]
    public int lod;
    public float visibleDistanceThreshold;

    public float sqrVisibleDstThrehold
    {
        get { return visibleDistanceThreshold * visibleDistanceThreshold; }
    }
}
