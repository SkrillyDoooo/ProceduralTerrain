using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileManager : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float colliderGenerationDistanceThreshold = 5;

    public LODInfo[] detailLevels;
    public static float maxViewDist = 450;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPos;
    public static Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;

    float meshWorldSize;
    int chunksVisibleInViewDist;
    public int colliderLODIndex;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public static GameObject m_ParentObjectEditorOnly;
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>(); // TODO:: ew
        meshWorldSize = mapGenerator.meshSettings.meshWorldSize;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / meshWorldSize);
        m_ParentObjectEditorOnly = new GameObject("Map Generator (Editor Only)");
        m_ParentObjectEditorOnly.transform.position = Vector3.zero;
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        
        UpdateVisibleChunks();
    }

    // Update is called once per frame
    void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPos != viewerPositionOld)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
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
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, meshWorldSize, detailLevels, colliderLODIndex, mapMaterial));
                }

            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 sampleCenter;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        LODInfo[] detailLevels;
        LODMesh[] LODMeshes;
        int colliderLODIndex;
        bool hasSetCollider;
        public Vector2 coordinate;

        HeightMap mapData;
        bool mapDataRecieved;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, float meshWorldSize, LODInfo[] detailLevels, int colliderLODIndex, Material material)
        {
            this.coordinate = coord;
            this.detailLevels = detailLevels;
            sampleCenter = coord * meshWorldSize / mapGenerator.meshSettings.meshScale;
            Vector2 position = coordinate * meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshWorldSize);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            this.colliderLODIndex = colliderLODIndex;
            hasSetCollider = false;

            meshRenderer.material = material;
            meshObject.transform.position = new Vector3(position.x, 0, position.y);
#if UNITY_EDITOR
            meshObject.transform.parent = m_ParentObjectEditorOnly.transform;
#endif
            SetVisible(false);
            mapGenerator.RequestHeightMap(sampleCenter, OnMapDataReceived);

            LODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                LODMeshes[i] = new LODMesh(detailLevels[i].lod);
                LODMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                    LODMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        void OnMapDataReceived(HeightMap mapData)
        {
            this.mapData = mapData;
            mapDataRecieved = true;

            UpdateTerrainChunk();
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            if(mapDataRecieved)
            {
                float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
                bool wasVisible = IsVisible();
                bool visible = viewDstFromNearestEdge <= maxViewDist;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                            break;
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = LODMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if(wasVisible != visible)
                    {
                        if(visible)
                        {
                            visibleTerrainChunks.Add(this);
                        }
                        else
                        {
                            visibleTerrainChunks.Remove(this);
                        }
                        SetVisible(visible);
                    }

                }

            }
        }

        public void UpdateCollisionMesh()
        {
            if (hasSetCollider)
                return;

            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPos);

            if(sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThrehold)
            {
                if(!LODMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    LODMeshes[colliderLODIndex].RequestMesh(mapData);
                }
            }

            if(sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if(LODMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = LODMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                } 
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        public int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback ();
        }
        public void RequestMesh(HeightMap mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshSettings.numSupportedLOD-1)]
        public int lod;
        public float visibleDistanceThreshold;

        public float sqrVisibleDstThrehold
        {
            get { return visibleDistanceThreshold * visibleDistanceThreshold;  } 
        }
    }
}
