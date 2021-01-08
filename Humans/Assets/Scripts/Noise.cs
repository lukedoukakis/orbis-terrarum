using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public static float[,] GenerateNoiseMap(int xSize, int zSize, int seed, float scale, int octaves, float persistance, float lacunarity)
    {

        if(seed == -1){ seed = Random.Range(-100000, 100000); }
        if(scale <= 0){ scale = .0001f;}

        float noiseMax = float.MinValue;
        float noiseMin = float.MaxValue;

        float[,] noiseMap = new float[xSize, zSize];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + seed;
                    float sampleZ = z / scale * frequency + seed;

                    float scale2 = scale + 5f;
                    float scale3 = scale - 5f;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    perlinValue += Mathf.PerlinNoise(sampleX*scale/scale2, sampleZ) * 2 - 1;
                    perlinValue -= Mathf.PerlinNoise(sampleX * scale / scale3, sampleZ) * 2 - 1;
                    
                    noiseHeight += perlinValue * amplitude;
    
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseHeight *= (40f * Mathf.PerlinNoise((x / 120f) + 1000, (z / 120f) + 1000));
                if (noiseHeight > noiseMax)
                {
                    noiseMax = noiseHeight;
                }
                else if(noiseHeight < noiseMin)
                {
                    noiseMin = noiseHeight;
                }

                noiseMap[x, z] = noiseHeight;

            }
        }

        // normalize noiseMap
        Debug.Log(noiseMin);
        Debug.Log(noiseMax);
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                noiseMap[x, z] = Mathf.InverseLerp(noiseMin, noiseMax, noiseMap[x, z]);
            }
        }
        
        return noiseMap;

    }

}
