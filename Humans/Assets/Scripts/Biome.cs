using System;
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
        new int[]{4},

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
        new int[]{4},

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
        new int[]{3},

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] JungleConditions = new int[][]
    {
        // temp
        new int[]{4},

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1, 2, 3, 4}
    };

    static int[][] ForestConditions = new int[][]
    {
        // temp
        new int[]{1, 2, 3 },

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{0, 1}
    };

    static int[][] TaigaConditions = new int[][]
    {
        // temp
        new int[]{1, 2, 3},

        // humidity
        new int[]{3, 4},

        // elevation
        new int[]{0, 1, 2, 3, 4},

        // mountain
        new int[]{2, 3, 4}
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

    // [temperature, humidity]
    static int[][] BiomeTable;


    public static GameObject[][] TreePool;


    public static void Init()
    {


        BiomeTable = new int[][]
        {
        new int[]{ (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga },
        new int[]{ (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga },
        new int[]{ (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.Tundra, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga, (int)BiomeType.SnowyTaiga },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga, (int)BiomeType.Taiga },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest, (int)BiomeType.Forest },
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
        };



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


    public static int GetBiome(float temp, float humid)
    {

        int temperature = (int)((temp * 10f) + 0.5f);
        int humidity = (int)((humid * 10f) + 0.5f);
        //Debug.Log(temp);
        int biome = BiomeTable[temperature][humidity];

        return biome;

    }

    public static Tuple<GameObject, Tuple<float, float, float, float>> GetTree(int biomeType, float wetness)
    {
        //Debug.Log(((BiomeType)biomeType).ToString());
        GameObject[] trees = TreePool[biomeType];
        if(trees.Length > 0)
        {
            GameObject tree = trees[UnityEngine.Random.Range(0, trees.Length)];
            return Tuple.Create(tree, TreeInfo.GetPlacementRequirements(tree.name, wetness));
        }
        else
        {
            //Debug.Log("GetTree(" + biomeType + "): returning null");
            return null;
        }
    }



}
