using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour
{

    public string type;



    public static Tuple<float, float, float, float> GetPlacementRequirements(string name, float wetness)
    {
        //Debug.Log("TreeInfo: type is: " + type);

        float scale;
        float density;
        float normal;
        float slant;

        switch (name)
        {
            case "Acacia Tree":
                scale = .05f;
                density = .01f;
                normal = .99f;
                slant = 0f;
                break;
            case "Jungle Tree":
                scale = .12f;
                density = 2f;
                normal = .7f;
                slant = .5f;
                break;
            case "Fir Tree":
                scale = .07f;
                density = .75f;
                normal = .7f;
                slant = .18f;
                break;
            case "Snowy Fir Tree":
                scale = .07f;
                density = .7f;
                normal = .7f;
                slant = .18f;
                break;
            case "Palm Tree":
                scale = .05f;
                density = .1f;
                normal = .99f;
                slant = 0f;
                break;
            case "Oak Tree":
                scale = .08f;
                density = 1f;
                normal = .998f;
                slant = .18f;
                break;
            default:
                scale = -1f;
                density = -1f;
                normal = -1f;
                slant = -1f;
                break;
        }

        density *= (wetness + .5f);

        return Tuple.Create(scale, density, normal, slant);


    }



   
}
