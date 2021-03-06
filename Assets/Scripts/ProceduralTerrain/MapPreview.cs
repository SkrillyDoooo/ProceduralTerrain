﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public bool DisableOnEnterPlaymode;

    public enum DrawMode
    {
        Noise,
        MeshAndColor,
        Falloff,
    }

    public HeightMapSettings heightMapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;


    [Range(0, MeshSettings.numSupportedLOD - 1)]
    public int editorPreviewLevelOfDetail;

    public bool autoUpdate = true;

    public DrawMode drawMode;

    public ComputeShader shader;
    public Material terrainMaterial;

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void Start()
    {
        meshRenderer.gameObject.SetActive(!DisableOnEnterPlaymode);
        textureRenderer.gameObject.SetActive(!DisableOnEnterPlaymode);
    }

    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
        textureData.ApplyToMaterial(terrainMaterial);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticiesPerLine, meshSettings.numberOfVerticiesPerLine, heightMapSettings, Vector2.zero);

        switch (drawMode)
        {
            case DrawMode.Noise:
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
                break;
            case DrawMode.MeshAndColor:
                DrawMesh(MeshGenerator.GenerateMesh(heightMap.values, meshSettings, editorPreviewLevelOfDetail));
                break;
            case DrawMode.Falloff:
                DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numberOfVerticiesPerLine), 0, 1)));
                break;

        }
    }


    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width/10f, 1, texture.height/10f);

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    private void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

}
