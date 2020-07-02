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
    public Transform colliderPOI;

    Vector2 colliderPOIPos;
    Vector2 colliderPOIOld;

    Vector2 viewerPos;
    Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDist;
    public int colliderLODIndex;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public static GameObject m_ParentObjectEditorOnly;
    // Start is called before the first frame update
    void Start()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightSettings.minHeight, heightSettings.maxHeight);

        meshWorldSize = meshSettings.meshWorldSize;
        m_ParentObjectEditorOnly = new GameObject("Map Generator (Editor Only)");
        m_ParentObjectEditorOnly.transform.position = Vector3.zero;

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);


        UpdateVisibleChunks();
    }

    // Update is called once per frame
    void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);
        colliderPOIPos = new Vector2(colliderPOI.position.x, colliderPOI.position.z);
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

    void UpdateVisibleChunks()
    {

        HashSet<Vector2> updatedChunkCoords = new HashSet<Vector2>();
        for(int i = visibleTerrainChunks.Count - 1; i >= 0  ; i--)
        {
            updatedChunkCoords.Add(visibleTerrainChunks[i].coordinate);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshWorldSize);

        for(int yOffset = - chunksVisibleInViewDist; yOffset <= chunksVisibleInViewDist; yOffset++)
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
                    TerrainChunk terrainChunk = new TerrainChunk(viewedChunkCoord, heightSettings, meshSettings, detailLevels, colliderLODIndex, terrainMaterial, viewer, colliderPOI, m_ParentObjectEditorOnly.transform);
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
