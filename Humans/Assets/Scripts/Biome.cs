using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{

    public static bool initialized;
    public enum BiomeType
    {
        Desert,
        Chaparral,
        Jungle,
        Savannah,
        Plains,
        Tundra,
        Forest,
        Taiga,
        SnowyTaiga,
        Ocean
    }


    // --------------------------------------------------------------------

    // condition definitions
    static int[][] DesertConditions = new int[][]
    {
        // temp
        new int[]{3, 4},

        // humidity
        new int[]{0},

        // elevation
        new int[]{0, 1},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] ChaparralConditions = new int[][]
    {
        // temp
        new int[]{3, 4},

        // humidity
        new int[]{0},

        // elevation
        new int[]{2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] PlainsConditions = new int[][]
    {
        // temp
        new int[]{1, 2, 3, 4},

        // humidity
        new int[]{0, 1, 2},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] TundraConditions = new int[][]
    {
        // temp
        new int[]{0},

        // humidity
        new int[]{0, 1, 2},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] SavannahConditions = new int[][]
    {
        // temp
        new int[]{3, 4},

        // humidity
        new int[]{3},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] JungleConditions = new int[][]
    {
        // temp
        new int[]{3, 4},

        // humidity
        new int[]{4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] ForestConditions = new int[][]
    {
        // temp
        new int[]{3},

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] TaigaConditions = new int[][]
    {
        // temp
        new int[]{1, 2},

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] SnowyTaigaConditions = new int[][]
    {
        // temp
        new int[]{0},

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] OceanConditions = new int[][]
    {
        // temp
        new int[]{0, 1, 2, 3, 4},

        // humidity
        new int[]{0, 1, 2, 3, 4},

        // elevation
        new int[]{-1},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][][] BiomeConditions = new int[][][]
    {
        DesertConditions,
        ChaparralConditions,
        JungleConditions,
        SavannahConditions,
        PlainsConditions,
        TundraConditions,
        ForestConditions,
        TaigaConditions,
        SnowyTaigaConditions,
        OceanConditions
    };


    public static GameObject[][] TreePool;


    static void Start()
    {
        //Init();
    }

    public static void Init()
    {
        string biomeName;
        string path;
        TreePool = new GameObject[10][];
        for (int i = 0; i < TreePool.Length; i++)
        {
            biomeName = ((BiomeType)i).ToString();
            path = "Terrain/" + biomeName + "/Trees";
            TreePool[i] = Resources.LoadAll<GameObject>(path);
            //Debug.Log(TreePool[i].Length);
        }

        //Debug.Log("initialized");
        initialized = true;
    }


    public static int GetBiome(float temp, float humid, float elev, float mountain)
    {

        // transform parameters to integer [0, 4] (or -1) scale
        float[] parameters = new float[] { temp, humid, elev, mountain };
        int[] sampledTraits = new int[parameters.Length];
        float p;
        for(int i = 0; i < parameters.Length; i++)
        {
            p = parameters[i];
            if (p == -1f)
            {
                sampledTraits[i] = -1;
            }
            else
            {
                float f = Mathf.Clamp(parameters[i], 0, .999f);
                sampledTraits[i] = (int)(f * 5f);
            }
        }

        // match the parameters to biome conditions find the biome
        bool[] blackList = new bool[BiomeConditions.Length];
        for(int i = 0; i < sampledTraits.Length; i++)
        {
            int trait = sampledTraits[i];
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

        /*
        //Debug.Log(n);
        //Debug.Log(biome); 
        */

        if(biome == -1)
        {
            Debug.Log("NO BIOME MATCHED\n" + sampledTraits[0] + " " + sampledTraits[1] + " " + sampledTraits[2] + " " + sampledTraits[3]);
        }
        

        return biome;

    }

    public static GameObject GetTree(int biomeType)
    {
        //Debug.Log(((BiomeType)biomeType).ToString());
        GameObject[] trees = TreePool[biomeType];
        if(trees.Length > 0)
        {
            return trees[Random.Range(0, trees.Length)];
        }
        else
        {
            return null;
        }
    }



}
