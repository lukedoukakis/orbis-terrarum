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


    // [temperature, humidity]
    static int[][] BiomeTable;
    public static GameObject[][] TreePool;



    public static float MaxTemp_Snow = 4f / 11f;

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
        new int[]{ (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Plains, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
        new int[]{ (int)BiomeType.Desert, (int)BiomeType.Desert, (int)BiomeType.Chaparral, (int)BiomeType.Chaparral, (int)BiomeType.Savannah, (int)BiomeType.Savannah, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle, (int)BiomeType.Jungle },
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
