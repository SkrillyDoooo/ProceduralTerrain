using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        if (scale <= 0)
            scale = 0.0001f;


        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }



        float maxHeght = float.MinValue;
        float minHeight = float.MaxValue;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1f;
                float freq = 1.0f;
                float noiseHeight = 0f;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * freq + octaveOffsets[i].x;
                    float sampleY = y / scale * freq + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    freq *= lacunarity;
                }

                if(noiseHeight > maxHeght)
                {
                    maxHeght = noiseHeight;
                }
                else if(noiseHeight < minHeight)
                {
                    minHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeght, noiseMap[x, y]);
            }
        }

         return noiseMap;
    }
}