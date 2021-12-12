using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;

    public Transform viewer;
    public Material terrainMaterial;

    public MeshSettings meshSettings;
    public HeightMapSettings heightSettings;
    public TextureData textureSettings;
    public NavMapSettings navMapSettings;

    public Transform colliderPOI;

    Vector2 colliderPOIPos;
    Vector2 colliderPOIOld;

    Vector2 viewerPos;
    Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDist;
    public int colliderLODIndex;
    HashSet<Vector2> updatedChunkCoords;

    public RenderTexture navMapDebug;
    public bool m_DebugNavMap = true;
    public DebugUI debugUI;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private static TerrainGenerator m_Instance;

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
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightSettings.minHeight, heightSettings.maxHeight);

        meshWorldSize = meshSettings.meshWorldSize;
        m_ParentObjectEditorOnly = new GameObject("Map Generator (Editor Only)");
        m_ParentObjectEditorOnly.transform.position = Vector3.zero;

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);

        updatedChunkCoords = new HashSet<Vector2>();
        UpdateVisibleChunks();
        int rtDimension = (meshSettings.numberOfVerticiesPerLine - 1) / navMapSettings.skipIncrement + 1;
        navMapDebug = new RenderTexture(rtDimension, rtDimension, 1);

        debugUI.SetRenderTexture(navMapDebug);
    }

    // Update is called once per frame
    void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);
        colliderPOIPos = new Vector2(colliderPOI.position.x, colliderPOI.position.z);
        if(m_DebugNavMap)
            DebugNavMap();

        if (colliderPOIPos != colliderPOIOld)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
            UpdateVisibleChunks();
            colliderPOIOld = colliderPOIPos;
        }

        if((viewerPositionOld - viewerPos).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPos;
            UpdateVisibleChunks();
        }
    }

    void DebugNavMap()
    {
        RenderTexture currentActiveRT = RenderTexture.active;
        RenderTexture.active = navMapDebug;
        if(TryGetTerrainChunkClosestToPoint(colliderPOIPos, out TerrainChunk chunk) && chunk.TryGetHeightMap(out HeightMap height) && chunk.TryGetNavMap(out NavMap nav))
        {
            debugUI.SetCoordinateLabel((int)chunk.coordinate.x, (int)chunk.coordinate.y);
            Graphics.Blit(TextureGenerator.TextureFromNavMapWithBlip(nav, height, chunk.GetNavMapIndexAtPoint(colliderPOIPos)), navMapDebug);
        }

        RenderTexture.active = currentActiveRT;
    }

    public bool TryGetNavMapAtCoordinate(Vector2 coord, out NavMap nav)
    {
        nav = default;
        return terrainChunkDictionary.TryGetValue(coord, out TerrainChunk chunk) && chunk.TryGetNavMap(out nav);
    }

    public bool TryGetNavMapAtWorldPoint(Vector2 point, out NavMap nav, out Vector2Int Index)
    {
        nav = default;
        Index = Vector2Int.zero;
        int currentChunkCoordX = Mathf.RoundToInt(point.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(point.y / meshWorldSize);
        if(terrainChunkDictionary.TryGetValue(new Vector2(currentChunkCoordX, currentChunkCoordY), out TerrainChunk chunk) && chunk.TryGetNavMap(out nav))
        {
            Index = chunk.GetNavMapIndexAtPoint(point);
            return true;
        }
        return false;
    }


    bool TryGetTerrainChunkClosestToPoint(Vector2 point, out TerrainChunk terrainChunk)
    {
        int currentChunkCoordX = Mathf.RoundToInt(point.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(point.y / meshWorldSize);
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

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshWorldSize);

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
                    TerrainChunk terrainChunk = new TerrainChunk(viewedChunkCoord, heightSettings, meshSettings, navMapSettings, detailLevels, colliderLODIndex, terrainMaterial, viewer, colliderPOI, m_ParentObjectEditorOnly.transform);
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
