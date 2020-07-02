using UnityEngine;
using System;

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
    int previousLODIndex = -1;

    MeshSettings meshSettings;
    HeightMapSettings heightMapSettings;
    Transform viewer;
    Transform colliderPOI;
    float maxViewDst;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Material material, Transform viewer, Transform colliderPOI, Transform parentObjectEditorOnly)
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

        UpdateTerrainChunk();
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