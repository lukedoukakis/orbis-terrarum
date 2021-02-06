using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{

    public enum BiomeType
    {
        Desert,
        Swamp,
        Jungle,
        Plains,
        Forest,
        Taiga,
        SnowyTaiga
    }



    // temp levels
    static int cold = 0;
    static int temperate = 1;
    static int hot = 2;

    // wetness levels
    static int dry = 0;
    static int damp = 1;
    static int wet = 2;

    // height levels
    static int low = 0;
    static int mid = 1;
    static int high = 2;

    // river levels
    static int fresh = 0;
    static int noFresh = 1;


    // --------------------------------------------------------------------


    static int[][] DesertConditions = new int[][]
    {
        // temp
        new int[]{hot},

        // wetness
        new int[]{dry},

        // height
        new int[]{low},

        // fresh water requirement
        new int[]{noFresh}
    };


    static int[][] SwampConditions = new int[][]
    {
        // temp
        new int[]{temperate, hot},

        // wetness
        new int[]{wet},

        // height
        new int[]{low},

        // fresh water requirement
        new int[]{}
    };

    static int[][] JungleConditions = new int[][]
    {
        // temp
        new int[]{hot},

        // wetness
        new int[]{wet},

        // height
        new int[]{low},

        // fresh water requirement
        new int[]{fresh, noFresh}
    };

    

    static int[][] PlainsConditions = new int[][]
    {
        // temp
        new int[]{temperate},

        // wetness
        new int[]{dry},

        // height
        new int[]{low, mid, high},

        // fresh water requirement
        new int[]{fresh, noFresh}
    };

    static int[][] ForestConditions = new int[][]
    {
        // temp
        new int[]{temperate},

        // wetness
        new int[]{damp, wet},

        // height
        new int[]{low, mid},

        // fresh water requirement
        new int[]{fresh, noFresh}
    };

    static int[][] TaigaConditions = new int[][]
    {
        // temp
        new int[]{cold},

        // wetness
        new int[]{damp, wet},

        // height
        new int[]{low, mid},

        // fresh water requirement
        new int[]{fresh, noFresh}
    };

    static int[][] SnowyTaigaConditions = new int[][]
    {
        // temp
        new int[]{cold},

        // wetness
        new int[]{damp, wet},

        // height
        new int[]{high},

        // fresh water requirement
        new int[]{fresh, noFresh}
    };

    static int[][][] BiomeConditions = new int[][][]
    {
        DesertConditions,
        SwampConditions,
        JungleConditions,
        PlainsConditions,
        ForestConditions,
        TaigaConditions,
        SnowyTaigaConditions
    };



    public static int GetBiomeType(float temp, float wetness, float height, float freshWater)
    {

        int t, w, h, f;

        // determine conditions
        if (temp > .6f)
        {
            if (temp > .8f)
            {
                t = hot;
            }
            else
            {
                t = temperate;
            }
        }
        else
        {
            t = cold;
        }

        if (wetness > .5f)
        {
            if (wetness > .8f)
            {
                w = wet;
            }
            else
            {
                w = damp;
            }
        }
        else
        {
            w = dry;
        }

        if (height > .82f)
        {
            if (height > .84f)
            {
                h = high;
            }
            else
            {
                h = mid;
            }
        }
        else
        {
            h = low;
        }

        if (freshWater > .93f)
        {
            f = fresh;
        }
        else
        {
            f = noFresh;
        }


        int[] targetTraits = new int[] { t, w, h, f };
        bool[] blackList = new bool[BiomeConditions.Length];

        for(int i = 0; i < targetTraits.Length; i++)
        {
            int trait = targetTraits[i];
            for (int k = 0; k < BiomeConditions.Length; k++)
            {
                if (!blackList[k])
                {
                    int[][] BiomeCondition = BiomeConditions[k];
                    bool hit = false;
                    for (int j = 0; j < BiomeCondition[i].Length; j++)
                    {
                        if (BiomeCondition[i][j] == trait) { hit = true; break; }
                    }
                    if (!hit)
                    {
                        blackList[k] = true;
                    }
                }
            }
        }

        int biome = -1;
        int n = 0;
        for(int i = 0; i < blackList.Length; i++)
        {
            if(!blackList[i]) {
                n++;
                biome = i;
                break;
            }
        }

        //Debug.Log(n);

        /*
        if(biome == (int)BiomeType.Desert)
        {
            Debug.Log("DESERT");
        }
        */

        return biome;

    }



}
