using UnityEngine;
using System;
using UnityEngine.AI;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Profiling;

public class TerrainChunk
{
    GameObject terrainObject;
    Vector2 sampleCenter;
    Bounds bounds;

    public event System.Action<TerrainChunk, bool> onVisibilityChanged;

    const float colliderGenerationDistanceThreshold = 5;

    LODInfo[] detailLevels;
    LODMesh[] LODMeshes;
    int colliderLODIndex;
    bool hasSetCollider;
    public Vector2 coordinate;

    HeightMap heightMap;
    bool heightMapReceived;

    TerrainGrid terrainGrid;
    bool terrainGridReceived;

    NavMapChunk navMap;
    bool navMapReceived;
    int previousLODIndex = -1;

    HeightMapSettings heightMapSettings;
    TerrainGridSettings terrainGridSettings;
    Transform viewer;
    float maxViewDst;

    NavigationNodePool navigationNodePool;

    Renderer terrainGridTextureRenderer;

    static readonly ProfilerMarker s_OnTerrainGridReceivedProfilerMarker = new ProfilerMarker("OnTerrainGridReceived");
    static readonly ProfilerMarker s_OnHeightMapReceived = new ProfilerMarker("OnHeightMapReceived");
    static readonly ProfilerMarker s_GetNavMap = new ProfilerMarker("GetNavMap");


    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, TerrainGridSettings gridSettings, LODInfo[] detailLevels, int colliderLODIndex, Material material, Transform viewer, Transform parentObjectEditorOnly, NavigationNodePool navigationNodePool, GameObject terrainPrefab)
    {
        this.coordinate = coord;
        this.detailLevels = detailLevels;
        float terrainWorldSize = gridSettings.terrainWorldSize;
        sampleCenter = coord * gridSettings.dimensions;
        sampleCenter.x *= -1;
        Vector2 position = coordinate * gridSettings.terrainWorldSize;
        bounds = new Bounds(position, Vector2.one * terrainWorldSize);
        this.heightMapSettings = heightMapSettings;
        this.terrainGridSettings = gridSettings;
        this.viewer = viewer;

        terrainObject = GameObject.Instantiate(terrainPrefab, new Vector3(position.x, 0, position.y), terrainPrefab.transform.rotation);
        terrainObject.layer = LayerMask.NameToLayer("Terrain");
        terrainGridTextureRenderer = terrainObject.GetComponent<Renderer>();
        terrainObject.SetActive(true);
        terrainObject.transform.localScale = Vector3.one * terrainWorldSize;

        this.colliderLODIndex = colliderLODIndex;
        hasSetCollider = false;
        this.navigationNodePool = navigationNodePool;

#if UNITY_EDITOR
        terrainObject.transform.parent = parentObjectEditorOnly.transform;
#endif
        SetVisible(false);

        // LODMeshes = new LODMesh[detailLevels.Length];
        //  for (int i = 0; i < detailLevels.Length; i++)
        // {
        //     LODMeshes[i] = new LODMesh(detailLevels[i].lod);
        //     LODMeshes[i].updateCallback += UpdateTerrainChunk;
        // }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public bool TryGetNavMap(out NavMapChunk nav)
    {
        nav = navMapReceived ? navMap : default;
        return navMapReceived   ;
    }


    public bool TryGetHeightMap(out HeightMap height)
    {
        height = heightMapReceived ? heightMap : default;
        return heightMapReceived;
    }

    public Vector2Int GetHeightMapIndexAtPoint(Vector2 point)
    {
        return Vector2Int.zero;
    }

    public float GetHeightAtCoord(Vector2 point)
    {
        Vector2Int t = GetHeightMapIndexAtPoint(point);
        float value = heightMap.values[t.x, t.y];
        return value;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(
            () => HeightMapGenerator.GenerateHeightMap(terrainGridSettings.dimensions, terrainGridSettings.dimensions, heightMapSettings, sampleCenter)
            , OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        s_OnHeightMapReceived.Begin();
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        ThreadedDataRequester.RequestData(() => TerrainGridGenerator.GenerateTerrainGridFromHeightMap(heightMap.values, terrainGridSettings, heightMapSettings.maxHeight), OnTerrainGridReceived);

        UpdateTerrainChunk();
        s_OnHeightMapReceived.End();
    }

    void OnTerrainGridReceived(object terrainGridObject)
    {
        s_OnTerrainGridReceivedProfilerMarker.Begin();
        this.terrainGrid = (TerrainGrid)terrainGridObject;
        terrainGridReceived = true;
        ThreadedDataRequester.RequestDataThreadPool(() => GetNavMapChunk(), OnNavMapReceived);
        terrainGridTextureRenderer.material.mainTexture = TextureGenerator.TextureFromTerrainGrid(terrainGrid);
        UpdateTerrainChunk();
        s_OnTerrainGridReceivedProfilerMarker.End();
    }

    NavMapChunk GetNavMapChunk()
    {
        s_GetNavMap.Auto();
        return new NavMapChunk(terrainGrid.values, terrainGridSettings.dimensions, new Vector2Int((int)coordinate.x,(int) coordinate.y), navigationNodePool);
    }

    void OnNavMapReceived(object navMapObject)
    {
        this.navMap = (NavMapChunk)navMapObject;
        navMapReceived = true;
    }

    // void OnMeshDataReceived(MeshData meshData)
    // {
    //     meshFilter.mesh = meshData.CreateMesh();
    // }

    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewDstFromNearestEdge <= maxViewDst;

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



                //perform lodding

                // if (lodIndex != previousLODIndex)
                // {
                //     LODMesh lodMesh = LODMeshes[lodIndex];
                //     if (lodMesh.hasMesh)
                //     {
                //         previousLODIndex = lodIndex;
                //         meshFilter.mesh = lodMesh.mesh;
                //     }
                //     else if (!lodMesh.hasRequestedMesh)
                //     {
                //         lodMesh.RequestMesh(heightMap, meshSettings);
                //     }
                // }
            }


            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onVisibilityChanged != null)
                    onVisibilityChanged(this, visible);
            }

        }
    }

    public void SetVisible(bool visible)
    {
        terrainGridTextureRenderer.enabled = visible;
    }

    public bool IsVisible()
    {
        return terrainGridTextureRenderer.enabled;
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

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;
        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}