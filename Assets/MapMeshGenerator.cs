using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEngine.Rendering;

public static class MapMeshGenerator
{
    public static MeshData GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail)
    {
        AnimationCurve curve = new AnimationCurve(meshHeightCurve.keys);
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        float topLeftX = (width - 1) / -2.0f;
        float topLeftZ = (height - 1) / 2.0f;


        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticiesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(width, height);

        for(int y = 0, vi = 0 ;  y < height; y+= meshSimplificationIncrement)
        {
            for(int x = 0; x < width; x+= meshSimplificationIncrement, vi++)
            {
                meshData.vertices[vi] = new Vector3(topLeftX + x, curve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vi] = new Vector2(x / (float)width, y / (float)height);

                if(x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vi, vi + verticiesPerLine + 1, vi + verticiesPerLine);
                    meshData.AddTriangle(vi + verticiesPerLine + 1, vi, vi + 1);
                }
            }
        }

        return meshData;
    }

    public static void GenerateMeshAsync(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail, ComputeShader shader, Action<MeshData> callback)
    {
        int xSize = heightMap.GetLength(0);
        int ySize = heightMap.GetLength(1);

        Texture2D t2d = TextureGenerator.TextureFromHeightMap(heightMap);

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticiesPerLine = (xSize - 1) / meshSimplificationIncrement + 1;

        //component for retrieving starting the coroutine and getting the request
        MeshGeneratorComponent meshGenerator = new GameObject("Mesh Generator").AddComponent<MeshGeneratorComponent>();

        meshGenerator.Vertices = new ComputeBuffer((verticiesPerLine) * (verticiesPerLine), sizeof(float) * 3, ComputeBufferType.Default);
        meshGenerator.UVs = new ComputeBuffer((verticiesPerLine) * (verticiesPerLine), sizeof(float) * 2, ComputeBufferType.Default);
        meshGenerator.Triangles = new ComputeBuffer(verticiesPerLine * verticiesPerLine * 6, sizeof(int), ComputeBufferType.Default);
        meshGenerator.SimplificationIncrement = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Default);
        meshGenerator.VerticesPerLine = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Default);

        meshGenerator.Curve = new ComputeBuffer(256, sizeof(float), ComputeBufferType.Default);

        int kernelHandle = shader.FindKernel("CSGenerateMesh");
        meshGenerator.Curve.SetData(meshHeightCurve.GenerateCurveArray());
        meshGenerator.SimplificationIncrement.SetData(new int[] { meshSimplificationIncrement });
        meshGenerator.VerticesPerLine.SetData(new int[] { verticiesPerLine });

        shader.SetInt("Width", xSize);
        shader.SetInt("Height", ySize);
        shader.SetInt("CurveLength", AnimationCurveExtension.CurveArrayLength);

        shader.SetFloat("Multiplier", heightMultiplier);

        shader.SetBuffer(kernelHandle, "Triangles", meshGenerator.Triangles);
        shader.SetBuffer(kernelHandle, "MeshVerticies", meshGenerator.Vertices);
        shader.SetBuffer(kernelHandle, "UVs", meshGenerator.UVs);
        shader.SetBuffer(kernelHandle, "Curve", meshGenerator.Curve);
        shader.SetTexture(kernelHandle, "HeightMap", t2d);
        shader.SetBuffer(kernelHandle, "SimplificationIncrement", meshGenerator.SimplificationIncrement);
        shader.SetBuffer(kernelHandle, "VerticesPerLine", meshGenerator.VerticesPerLine);

        //TODO: batch creation of all current mesh requests?
        shader.Dispatch(kernelHandle, 1, 1, 1);

        MeshData meshData = new MeshData(verticiesPerLine, verticiesPerLine);
        meshGenerator.AsyncMeshRequest(meshData, callback);
    }

    public class MeshGeneratorComponent : MonoBehaviour
    {
        public ComputeBuffer Vertices;
        public ComputeBuffer UVs;
        public ComputeBuffer Triangles;
        public ComputeBuffer VerticesPerLine;
        public ComputeBuffer SimplificationIncrement;
        public ComputeBuffer Curve;

        public void AsyncMeshRequest(MeshData meshData, Action<MeshData> callback)
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            StartCoroutine(AsyncMeshDataRequest(meshData, callback));
        }

        IEnumerator AsyncMeshDataRequest(MeshData meshData, Action<MeshData> callback)
        {
            var AsyncVertexReadBack = AsyncGPUReadback.Request(Vertices, null);
            var AsyncTriangleReadBack = AsyncGPUReadback.Request(Triangles, null);
            var AsyncUVReadBack = AsyncGPUReadback.Request(UVs, null);
            while (!AsyncVertexReadBack.done || !AsyncTriangleReadBack.done || !AsyncUVReadBack.done)
            {
                yield return null;
            }

            if(AsyncVertexReadBack.hasError || AsyncTriangleReadBack.hasError || AsyncUVReadBack.hasError)
            {
                Debug.LogError("AsyncGPUReadback error when fetching mesh data");
            }
            else
            {
                meshData.vertices = AsyncVertexReadBack.GetData<Vector3>().ToArray();
                meshData.triangles = AsyncTriangleReadBack.GetData<int>().ToArray();
                meshData.uvs = AsyncUVReadBack.GetData<Vector2>().ToArray();
                callback(meshData);
            }

            Vertices.Dispose();
            UVs.Dispose();
            Triangles.Dispose();
            VerticesPerLine.Dispose();
            SimplificationIncrement.Dispose();
            Curve.Dispose();
            Destroy(gameObject);
        }
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[(meshWidth) * (meshHeight)];
        triangles = new int[(meshWidth-1) * (meshHeight-1) * 6];
        uvs = new Vector2[(meshWidth) * (meshHeight)];
        triangleIndex = 0;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }
}
