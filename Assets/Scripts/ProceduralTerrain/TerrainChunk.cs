using UnityEngine;
using System;
using UnityEngine.AI;
using System.Runtime.InteropServices.WindowsRuntime;

public class TerrainChunk
{
    GameObject meshObject;
    Vector2 sampleCenter;
    Bounds bounds;

    public event System.Action<TerrainChunk, bool> onVisibilityChanged;

    const float colliderGenerationDistanceThreshold = 5;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    LODInfo[] detailLevels;
    LODMesh[] LODMeshes;
    int colliderLODIndex;
    bool hasSetCollider;
    public Vector2 coordinate;

    HeightMap heightMap;
    bool heightMapReceived;

    NavMap navMap;
    bool navMapReceived;
    int previousLODIndex = -1;

    MeshSettings meshSettings;
    HeightMapSettings heightMapSettings;
    NavMapSettings navMapSettings;
    Transform viewer;
    Transform colliderPOI;
    float maxViewDst;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, NavMapSettings navMapSettings, LODInfo[] detailLevels, int colliderLODIndex, Material material, Transform viewer, Transform colliderPOI, Transform parentObjectEditorOnly)
    {
        this.coordinate = coord;
        this.detailLevels = detailLevels;
        float meshWorldSize = meshSettings.meshWorldSize;
        sampleCenter = coord * meshWorldSize / meshSettings.meshScale;
        Vector2 position = coordinate * meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshWorldSize);
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        this.colliderPOI = colliderPOI;

        this.navMapSettings = navMapSettings;

        meshObject = new GameObject("Terrain Chunk");
        meshObject.layer = LayerMask.NameToLayer("Terrain");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        if(meshSettings.generateColliderAroundColliderPOI)
            meshCollider = meshObject.AddComponent<MeshCollider>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        this.colliderLODIndex = colliderLODIndex;
        hasSetCollider = false;

        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(position.x, 0, position.y);
#if UNITY_EDITOR
            meshObject.transform.parent = parentObjectEditorOnly.transform;
#endif
        SetVisible(false);

        LODMeshes = new LODMesh[detailLevels.Length];

        for (int i = 0; i < detailLevels.Length; i++)
        {
            LODMeshes[i] = new LODMesh(detailLevels[i].lod);
            LODMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
                LODMeshes[i].updateCallback += UpdateCollisionMesh;
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public bool TryGetNavMap(out NavMap nav)
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
        Vector3 topLeftVector = new Vector3(-1, 0, 1) * (meshSettings.meshWorldSize) / 2f;
        Vector3 worldCenter = new Vector3(coordinate.x * meshSettings.meshWorldSize, 0, coordinate.y * meshSettings.meshWorldSize);
        Vector3 topLeft = worldCenter + topLeftVector;
        Vector3 worldPoint = new Vector3(point.x, 0, point.y);

        Debug.DrawRay(topLeft, Vector3.up * 1000, Color.red);
        Debug.DrawRay(worldCenter, Vector3.up * 1000, Color.green);

        Vector3 worldPointTexCoord = (worldPoint - topLeft);
        Vector3 worldPointDebugRayStart = (Vector3.up * 200) + topLeft;
        Vector3 worldPointDebugRayDir = ((Vector3.up * 200) + worldPoint) - worldPointDebugRayStart;
        Debug.DrawRay(worldPointDebugRayStart, worldPointDebugRayDir, Color.magenta);

        int x = Mathf.RoundToInt((Mathf.Clamp01(worldPointTexCoord.x / meshSettings.meshWorldSize) * (heightMap.values.GetLength(0) - 1)));
        int y = Mathf.RoundToInt((Mathf.Clamp01(-worldPointTexCoord.z / meshSettings.meshWorldSize) * (heightMap.values.GetLength(1) - 1)));

        return new Vector2Int(x, y);
    }

    public Vector2Int GetNavMapIndexAtPoint(Vector2 point)
    {
        Vector3 topLeftVector = new Vector3(-1, 0, 1) * (meshSettings.meshWorldSize) / 2f;
        Vector3 worldCenter = new Vector3(coordinate.x * meshSettings.meshWorldSize, 0,coordinate.y * meshSettings.meshWorldSize);
        Vector3 topLeft = worldCenter + topLeftVector;
        Vector3 worldPoint = new Vector3(point.x, 0, point.y);

        Debug.DrawRay(topLeft, Vector3.up * 1000, Color.red);
        Debug.DrawRay(worldCenter, Vector3.up * 1000, Color.green);

        Vector3 worldPointTexCoord = (worldPoint - topLeft);
        Vector3 worldPointDebugRayStart = (Vector3.up * 200) + topLeft;
        Vector3 worldPointDebugRayDir = ((Vector3.up * 200) + worldPoint) - worldPointDebugRayStart;
        Debug.DrawRay(worldPointDebugRayStart, worldPointDebugRayDir, Color.magenta);

        int x = Mathf.RoundToInt((Mathf.Clamp01(worldPointTexCoord.x / meshSettings.meshWorldSize) * (navMap.values.GetLength(0) - 1)));
        int y = Mathf.RoundToInt((Mathf.Clamp01(-worldPointTexCoord.z / meshSettings.meshWorldSize) * (navMap.values.GetLength(1) - 1)));

        return new Vector2Int(x, y);
    }

    public float GetHeightAtCoord(Vector2 point)
    {
        Vector2Int t = GetHeightMapIndexAtPoint(point);
        float value = heightMap.values[t.x, t.y];
        return value;
    }

    Vector2 colliderPOIPosition
    {
        get
        {
            return new Vector2(colliderPOI.localPosition.x, colliderPOI.localPosition.z);
        }
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(
            () => HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticiesPerLine, meshSettings.numberOfVerticiesPerLine, heightMapSettings, sampleCenter)
            , OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        ThreadedDataRequester.RequestData(() => NavMapGenerator.GenerateNavMap(heightMap.values, heightMapSettings.maxHeight , navMapSettings, new Vector2Int((int)coordinate.x, (int)coordinate.y)), OnNavMapReceived);

        UpdateTerrainChunk();
    }

    void OnNavMapReceived(object navMapObject)
    {
        this.navMap = (NavMap)navMapObject;
        navMapReceived = true;
    }

    void OnMeshDataReceived(MeshData meshData)
    {
        meshFilter.mesh = meshData.CreateMesh();
    }

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
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }


            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onVisibilityChanged != null)
                    onVisibilityChanged(this, visible);
            }

        }
    }

    public void UpdateCollisionMesh()
    {
        if (!meshSettings.generateColliderAroundColliderPOI || hasSetCollider)
            return;

        float sqrDstFromColliderPOIToEdge = bounds.SqrDistance(colliderPOIPosition);

        if (sqrDstFromColliderPOIToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThrehold)
        {
            if (!LODMeshes[colliderLODIndex].hasRequestedMesh)
            {
                LODMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
            }
        }

        if (sqrDstFromColliderPOIToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
        {
            if (LODMeshes[colliderLODIndex].hasMesh)
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