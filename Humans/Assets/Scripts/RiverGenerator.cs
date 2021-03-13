using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    static float ElevationMapScale;
    static int Seed;

    static int BoundNorth = 2000;
    static int BoundSouth = 0;
    static int BoundEast = 2000;
    static int BoundWest = 0;

    static Vector2[] Origins;
    static int RiverOriginCount = 90;

    static int MinRiverLength = 100;
    static int MaxRiverLength = 500;

    public static float[,] RiverMap;

    public static void Generate()
    {

        Random.InitState(0);
        ElevationMapScale = ChunkGenerator.ElevationMapScale;
        Seed = ChunkGenerator.Seed;
        RiverMap = new float[BoundEast - BoundWest, BoundNorth - BoundSouth];

        // fill origins with random locations in range of bounds
        Origins = new Vector2[RiverOriginCount];
        int ox, oz;
        for (int i = 0; i < Origins.Length; i++)
        {
            ox = Random.Range(BoundWest, BoundEast);
            oz = Random.Range(BoundSouth, BoundNorth);
            if (Mathf.PerlinNoise((ox - Seed + .01f) / ElevationMapScale, (oz - Seed + .01f) / ElevationMapScale) >= .5f)
            {
                Origins[i] = new Vector2(ox, oz);
            }
            else
            {
                i--;
            }
        }


        float e, eNorth, eSouth, eEast, eWest;
        for (int i = 0; i < Origins.Length; i++)
        {

            // set up initial point to sample
            int x = (int)Origins[i].x;
            int z = (int)Origins[i].y;
            int forceX = 0;
            int forceZ = 0;
            int riverLength = Random.Range(MinRiverLength, MaxRiverLength);
            //Debug.Log(x.ToString() + " " + z.ToString());


            // sample length of the river
            for (int m = 0; m < riverLength; m++)
            {
                if (x >= BoundWest && x < BoundEast && z >= BoundSouth && z < BoundNorth)
                {
                    FillMap(x, z, 1f);
                }

                // get elevation at point and surrounding elevations
                e = Mathf.PerlinNoise((x - Seed + .01f) / ElevationMapScale, (z - Seed + .01f) / ElevationMapScale);
                eNorth = Mathf.PerlinNoise((x - Seed + .01f) / ElevationMapScale, (z - Seed + .01f + 1f) / ElevationMapScale);
                eSouth = Mathf.PerlinNoise((x - Seed + .01f) / ElevationMapScale, (z - Seed + .01f - 1f) / ElevationMapScale);
                eEast = Mathf.PerlinNoise((x - Seed + .01f + 1f) / ElevationMapScale, (z - Seed + .01f) / ElevationMapScale);
                eWest = Mathf.PerlinNoise((x - Seed + .01f - 1f) / ElevationMapScale, (z - Seed + .01f) / ElevationMapScale);
                float[] pts = new float[] { e, eNorth, eSouth, eEast, eWest };

                float lowestPt = Mathf.Min(pts);
                if (lowestPt == e || e < 0f)
                {
                    if (x >= BoundWest && x < BoundEast && z >= BoundSouth && z < BoundNorth)
                    {
                        FillMap(x, z, 10f);
                    }
                    break;
                }

                // calculate direction of river from surrounding elevations
                forceX = (int)(Mathf.Clamp(((eEast / eWest) - 1f) * 10000f, -5f, 5f));
                forceZ = (int)(Mathf.Clamp(((eNorth / eSouth) - 1f) * 10000f, -5f, 5f));
                Vector3 fVec = new Vector2(forceX, forceZ);
                forceX += (int)(Mathf.PerlinNoise((x + Seed) / 20f + .01f, (z + Seed) / 20f + .01f) * 15f);
                forceZ += (int)(Mathf.PerlinNoise((z - Seed) / 20f + .01f, (z - Seed) / 20f + .01f) * 15f);

                //Debug.Log(new Vector2(forceX, forceZ).ToString());
                float inverseSlope = ((float)forceX) / ((float)forceZ);
                float xFloat = (float)x;

                // fill in segment of river from slope of forceZ and forceX
                for (int fz = 0; fz < Mathf.Abs(forceZ); fz++)
                {
                    z += (int)Mathf.Sign(forceZ);
                    xFloat += inverseSlope;
                    x = (int)(xFloat);
                    FillMap(x, z, 1f);
                    //m++;
                }

            }
        }

        Debug.Log("RiverGenerator: finished");
    }

    static void FillMap(int x, int z, float value)
    {
        int radius = (int)(10 * value);
        
        Vector2 baseV = new Vector2(x, z);
        Vector2 sampleV = new Vector2();
        for (int i = z - radius; i < z + radius; i++)
        {
            sampleV.y = i;
            for (int j = x - radius; j < x + radius; j++)
            {

                if (j >= BoundWest && j < BoundEast && i >= BoundSouth && i < BoundNorth)
                {
                    sampleV.x = j;
                    RiverMap[j, i] = Mathf.Max(RiverMap[j, i], value * (1f / Vector2.Distance(baseV, sampleV)));
                }
            }
        }

    }
}
