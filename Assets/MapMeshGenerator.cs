using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using UnityEngine.Rendering;

public static class MapMeshGenerator
{
    public static MeshData GenerateMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
    {
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;

        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1) / -2.0f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2.0f;


        int verticiesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticiesPerLine, meshSettings.useFlatShading);

        int[,] vertexIndiciesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if(isBorderVertex)
                {
                    vertexIndiciesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndiciesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }


        for (int y = 0 ;  y < borderedSize; y+= meshSimplificationIncrement)
        {
            for(int x = 0; x < borderedSize; x+= meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndiciesMap[x, y];
                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                float height = heightMap[x, y];
                Vector3 vertexPosition = new Vector3((topLeftX + percent.x * meshSizeUnsimplified) * meshSettings.meshScale, height, (topLeftZ - percent.y * meshSizeUnsimplified) * meshSettings.meshScale);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if(x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndiciesMap[x, y];
                    int b = vertexIndiciesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndiciesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndiciesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }
            }
        }
        meshData.ProcessMesh();

        return meshData;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] borderVertices;
    Vector3[] normals;
    int[] borderTriangles;

    int borderTriangleIndex;
    int triangleIndex;
    bool useFlatShading;

    public MeshData(int verticiesPerLine, bool flatShading)
    {
        vertices = new Vector3[(verticiesPerLine) * (verticiesPerLine)];
        triangles = new int[(verticiesPerLine-1) * (verticiesPerLine-1) * 6];
        uvs = new Vector2[(verticiesPerLine) * (verticiesPerLine)];

        borderVertices = new Vector3[verticiesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticiesPerLine];
        this.useFlatShading = flatShading;

    }

    private void BakeNormals()
    {
        normals = CalculateNormals();
    }

    public void ProcessMesh()
    {
        if(useFlatShading)
        {
            FlatShading();
        } else
        {
            BakeNormals();
        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        if(useFlatShading)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            mesh.normals = normals;
        }
        return mesh;
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for(int i = 0; i < triangleCount; i++)
        {
            int normalTrianlgeIndex = i * 3;
            int vertA = triangles[normalTrianlgeIndex];
            int vertB = triangles[normalTrianlgeIndex + 1];
            int vertC = triangles[normalTrianlgeIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndicies(vertA, vertB, vertC);
            vertexNormals[vertA] += triangleNormal;
            vertexNormals[vertB] += triangleNormal;
            vertexNormals[vertC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTrianlgeIndex = i * 3;
            int vertA = borderTriangles[normalTrianlgeIndex];
            int vertB = borderTriangles[normalTrianlgeIndex + 1];
            int vertC = borderTriangles[normalTrianlgeIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndicies(vertA, vertB, vertC);
            if(vertA >= 0)
            {
                vertexNormals[vertA] += triangleNormal;
            }
            if (vertB >= 0)
            {
                vertexNormals[vertB] += triangleNormal;
            }
            if (vertC >= 0)
            {
                vertexNormals[vertC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndicies(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = indexA < 0 ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = indexB < 0 ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = indexC < 0 ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 ab = pointB - pointA;
        Vector3 ac = pointC - pointA;

        return Vector3.Cross(ab, ac).normalized;
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if(vertexIndex < 0)
        {
            borderVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    void FlatShading()
    {
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for(int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uvs[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uvs = flatShadedUvs;

    }
}
