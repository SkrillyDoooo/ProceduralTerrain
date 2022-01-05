using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapPreview : MonoBehaviour
{
    public Renderer[] textureRenderers;
    public bool DisableOnEnterPlaymode;

    public enum DrawMode
    {
        Noise,
        SimpleTerrainColor,
        NavMap,
        TileMap
    }

    public HeightMapSettings heightMapSettings;
    public TerrainGridSettings terrainGridSettings;
    public TileMapData tileMapData;

    public bool autoUpdate = true;
    public DrawMode drawMode;
    private NavigationNodePool navigationNodePool;
    public NavMapPreview navMapPreview;

    public Tilemap tileMap;


    public float scale;
    void Start()
    {
        SetRenderersActive(!DisableOnEnterPlaymode);
        navigationNodePool = new NavigationNodePool();
    }

    public void DrawMapInEditor()
    {
        int index = 0;
        navigationNodePool = new NavigationNodePool();
        HeightMap[] maps = new HeightMap[9];
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++, index++)
            {
                    Vector2 chunkcoord = new Vector2(x,y);
                    maps[index] =  HeightMapGenerator.GenerateHeightMap(terrainGridSettings.dimensions, terrainGridSettings.dimensions, heightMapSettings, chunkcoord * terrainGridSettings.dimensions); 
            }
        }

        index = 0;
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++, index++)
            {
                    Vector2Int chunkcoordInt = new Vector2Int(x,y);
                    HeightMap heightMap = maps[index];
                    TerrainGrid grid = TerrainGridGenerator.GenerateTerrainGridFromHeightMap(heightMap.values, terrainGridSettings, heightMapSettings.maxHeight);
                    switch (drawMode)
                    {
                        case DrawMode.Noise:
                            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap), index, x, y);
                            break;
                        case DrawMode.SimpleTerrainColor:
                            DrawTexture(TextureGenerator.TextureFromTerrainGrid(grid), index, x, y);
                            break;
                        case DrawMode.NavMap:
                            SetRenderersActive(false);
                            NavMapChunk navMap = new NavMapChunk(grid.values, terrainGridSettings.dimensions, chunkcoordInt, navigationNodePool);
                            navMapPreview.SetNavMap(navigationNodePool, scale, navMap.levels, heightMap.values.GetLength(0));
                            DrawTilemap(tileMapData.GenerateChunk(grid), chunkcoordInt);
                            break;
                        case DrawMode.TileMap:
                            SetRenderersActive(false);
                            DrawTilemap(tileMapData.GenerateChunk(grid), chunkcoordInt);
                            break;
                    }
            }
        }
        
    }

    public void DrawTilemap(TileBase[] tileBase, Vector2Int coord)
    {
        Vector2Int topLeft = new Vector2Int(-1, 1) * terrainGridSettings.dimensions + coord * terrainGridSettings.dimensions;
        for (int y = 0; y < terrainGridSettings.dimensions; y++)
        {
            for (int x = 0; x < terrainGridSettings.dimensions; x++)
            {
                tileMap.SetEditorPreviewTile(new Vector3Int(topLeft.x + x, topLeft.y - y, 0) + new Vector3Int(terrainGridSettings.dimensions/2, -terrainGridSettings.dimensions/2, 0), tileBase[y * terrainGridSettings.dimensions + x]);
            }
        }
    }

    public void DrawTexture(Texture2D texture, int index, int x, int y)
    {
        Renderer renderer = textureRenderers[index];
        renderer.sharedMaterial.mainTexture = texture;
        renderer.transform.localScale = Vector3.one * terrainGridSettings.terrainWorldSize;
        renderer.transform.position = new Vector3(x * terrainGridSettings.terrainWorldSize,0,y * terrainGridSettings.terrainWorldSize);

        renderer.gameObject.SetActive(true);
    }

    void SetRenderersActive(bool active)
    {
        foreach(var renderer in textureRenderers)
        {
            renderer.gameObject.SetActive(active);
        }
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            tileMap.ClearAllEditorPreviewTiles();
            DrawMapInEditor();
        }
    }
    private void OnValidate()
    {
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }

        if(terrainGridSettings != null)
        {
            terrainGridSettings.OnValuesUpdated -= OnValuesUpdated;
            terrainGridSettings.OnValuesUpdated += OnValuesUpdated;
        }
    }

}
