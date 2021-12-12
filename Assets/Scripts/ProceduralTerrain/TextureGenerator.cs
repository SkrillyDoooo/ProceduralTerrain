using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator 
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < height; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }

    public static Texture2D TextureFromNavMap(NavMap navMap)
    {
        int width = navMap.values.GetLength(0);
        int height = navMap.values.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = Color.white;
                for(int i = 0; i < navMap.abstractPathFindingTree.children.Length; i++)
                {
                    if(navMap.abstractPathFindingTree.children[i].ContainsTraversableTile(new Vector2Int(x, y)))
                    {
                        color = MinimalAbstractPathFindingTree.chunkColor[i % MinimalAbstractPathFindingTree.chunkColor.Length];
                    }
                }
                colorMap[y * width + x] = navMap.values[x, y] ? color : Color.black;
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }

    public static Texture2D TextureFromNavMapWithBlip(NavMap navMap, HeightMap heightMap, Vector2Int index)
    {
        int width = navMap.values.GetLength(0);
        int height = navMap.values.GetLength(1);

        Texture2D texture = new Texture2D(width, height,TextureFormat.RGBA32, 0, true);

        Color[] colorMap = new Color[width * height];
        for (int y = 0, j = height - 1; y < height; y++, j--)
        {
            for (int x = 0; x < width; x++)
            {
                if(x == index.x && y == index.y)
                {
                    colorMap[j * width + x] = Color.red;
                }
                else
                {
                    Color color = Color.white;
                    for (int i = 0; i < navMap.abstractPathFindingTree.children.Length; i++)
                    {
                        if (navMap.abstractPathFindingTree.children[i].ContainsTraversableTile(new Vector2Int(x, y)))
                        {
                            color = MinimalAbstractPathFindingTree.chunkColor[i % MinimalAbstractPathFindingTree.chunkColor.Length];
                        }
                    }
                    colorMap[j * width + x] = navMap.values[x, y] ? color : Color.black;
                }
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
}
