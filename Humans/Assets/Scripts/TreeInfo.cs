using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeInfo : MonoBehaviour
{

    public string type;



    public static Tuple<float, float, float, float, float> GetPlacementParameters(string name, float wetness, float fw)
    {
        //Debug.Log("TreeInfo: type is: " + type);

        float scale;
        float density;
        float normal;
        float slant;
        float spread;

        switch (name)
        {
            case "Acacia Tree":
                scale = .08f;
                density = .1f;
                normal = .998f;
                slant = 0f;
                spread = .5f;
                break;
            case "Jungle Tree":
                scale = .12f;
                density = 1f;
                normal = .7f;
                slant = .5f;
                spread = .3f;
                break;
            case "Fir Tree":
                scale = .07f;
                density = .75f;
                normal = .7f;
                slant = .18f;
                spread = .5f;
                break;
            case "Snowy Fir Tree":
                scale = .07f;
                density = .7f;
                normal = .7f;
                slant = .18f;
                spread = .5f;
                break;
            case "Palm Tree":
                scale = .09f;
                if(fw > .9f){
                    density = .2f;
                }
                else{
                    density = -1f;
                }
                normal = .98f;
                slant = 0f;
                spread = .1f;
                break;
            case "Oak Tree":
                scale = .08f;
                density = 1f;
                normal = .9f;
                slant = .18f;
                spread = .5f;
                break;
            case "Plains Oak Tree":
                scale = .09f;
                density = .01f;
                normal = .9985f;
                slant = .18f;
                spread = .1f;
                break;
            case string str when name.StartsWith("Grass"):
                scale = .23f;
                density = 7f;
                normal = .99f;
                slant = 1f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Reed"):
                scale = .2f;
                density = 4f;
                normal = .99f;
                slant = .5f;
                spread = .065f;
                break;
            case string str when name.StartsWith("Mushroom"):
                scale = .12f;
                density = .1f;
                normal = .7f;
                slant = 1f;
                spread = .05f;
                break;
            case string str when name.StartsWith("Bush"):
                scale = .04f;
                density = 7f;
                normal = .99f;
                slant = .8f;
                spread = .07f;
                break;
            case string str when name.StartsWith("Dead Bush"):
                scale = .2f;
                density = .02f;
                normal = .6f;
                slant = .8f;
                spread = .5f;
                break;
            case string str when name.StartsWith("Cactus"):
                scale = .09f;
                if(fw > .92f){
                    density = 0f;
                }
                else{
                    density = .01f;
                }
                normal = .996f;
                slant = .18f;
                spread = .5f;
                break;
            default:
                scale = -1f;
                density = -1f;
                normal = -1f;
                slant = -1f;
                spread = -1f;
                break;
        }

        density *= (wetness + .5f);

        return Tuple.Create(scale, density, normal, slant, spread);


    }



   
}
