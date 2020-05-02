using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class MapMeshGenerator
{
    public static MeshData GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve meshHeightCurve, int levelOfDetail, ComputeShader shader)
    {
        int xSize = heightMap.GetLength(0);
        int ySize = heightMap.GetLength(1);

        Texture2D t2d = TextureGenerator.TextureFromHeightMap(heightMap);

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticiesPerLine = (xSize - 1) / meshSimplificationIncrement + 1;

        ComputeBuffer Vertices = new ComputeBuffer((verticiesPerLine) * (verticiesPerLine), sizeof(float) * 3, ComputeBufferType.Default);
        ComputeBuffer UVs = new ComputeBuffer((verticiesPerLine) * (verticiesPerLine), sizeof(float) * 2, ComputeBufferType.Default);
        ComputeBuffer Triangles = new ComputeBuffer(verticiesPerLine * verticiesPerLine * 6, sizeof(int), ComputeBufferType.Default);
        ComputeBuffer Curve = new ComputeBuffer(256, sizeof(float), ComputeBufferType.Default);
        Curve.SetData(meshHeightCurve.GenerateCurveArray());

        int kernelHandle = shader.FindKernel("CSGenerateMesh");
        shader.SetInt("Width", xSize);
        shader.SetInt("Height", ySize);
        shader.SetInt("CurveLength", AnimationCurveExtension.CurveArrayLength);
        shader.SetInt("SimplificationIncrement", meshSimplificationIncrement);
        shader.SetInt("VerticesPerLine", verticiesPerLine);
        shader.SetFloat("Multiplier", heightMultiplier);

        shader.SetBuffer(kernelHandle, "Triangles", Triangles);
        shader.SetBuffer(kernelHandle, "MeshVerticies", Vertices);
        shader.SetBuffer(kernelHandle, "UVs", UVs);
        shader.SetBuffer(kernelHandle, "Curve", Curve);
        shader.SetTexture(kernelHandle, "HeightMap", t2d);

        shader.Dispatch(kernelHandle, 10, 1, 1);

        MeshData meshData = new MeshData(verticiesPerLine, verticiesPerLine);
        Vertices.GetData(meshData.vertices);
        Triangles.GetData(meshData.triangles);
        UVs.GetData(meshData.uvs);

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[(meshWidth) * (meshHeight)];
        triangles = new int[(meshWidth-1) * (meshHeight-1) * 6];
        uvs = new Vector2[(meshWidth) * (meshHeight)];
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
}
