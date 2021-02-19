using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour
{

    public string type;



    public static Tuple<float, float, float, float> GetPlacementRequirements(string type)
    {
        //Debug.Log("TreeInfo: type is: " + type);

        float scale;
        float density;
        float normal;
        float slant;


        /*
        float[] treeScales = new float[] { -1f, .1f, .05f, .1f, .06f, .09f, .05f, .05f };
        int[] treePassesMultipliers = new int[] { -1, 1, 2, 10, 1, 10, 10, 10 };
        float[] treeMinYNormals = new float[] { -1f, .99f, .7f, .7f, .99f, .7f, .7f, .7f };
        float[] treeAngleMultipliers = new float[] { -1f, 0f, .25f, .5f, .0f, .5f, .1f, .1f };
        */
        
        switch (type)
        {
            case "Acacia":
                scale = .05f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                break;
            case "Jungle":
                scale = .05f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                break;
            case "Fir":
                scale = .05f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                break;
            case "Snowy Fir":
                scale = .05f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                break;
            case "Palm":
                scale = .05f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                break;
            case "Oak":
                scale = .05f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                break;
            default:
                scale = -1f;
                density = -1f;
                normal = -1f;
                slant = -1f;
                break;
        }


        return Tuple.Create(scale, density, normal, slant);


    }



   
}
