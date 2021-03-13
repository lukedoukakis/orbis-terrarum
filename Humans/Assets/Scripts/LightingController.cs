using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{

    public Camera MainCamera;
    public Material skyMat;
    public Light mainLight;

    public Gradient tempGradient;
    public Gradient wetnessGradient;
    float height;
    float elevation;
    float temperature;
    float wetness;

    Color ambientColor, fogColor;
    float fog;

    static float fog_base = .2f;
    static float changeSpeed_ambientColor = 5f;
    static float changeSpeed_fog = 5f;

    static float period = .1f;
    static float updateTime;


    // Update is called once per frame
    void FixedUpdate()
    {
        if (Biome.initialized)
        {
            if (Time.fixedTime >= updateTime)
            {
                UpdateLighting();
                updateTime = Time.fixedTime + period;
            }
        }
        
        
    }

    private void Start()
    {
        Init();
    }

    void Init()
    {
        updateTime = Time.fixedTime + period;
    }

    void UpdateLighting()
    {
        AreaConditions.GetAreaConditions(MainCamera.transform.position);
        height = AreaConditions.Height;
        temperature = AreaConditions.Temperature;
        wetness = AreaConditions.Humidity;
        elevation = AreaConditions.Elevation;
        //SetLightingColors(temperature, wetness);
        SetFogDensity(temperature, wetness);
    }


    Color CalculateAmbientColor(float temp, float wetness)
    {

        Color c = Color.Lerp(tempGradient.Evaluate(temp), wetnessGradient.Evaluate(wetness), .2f);
        return c;
    }

    Color CalculateFogColor(float temp, float wetness)
    {

        Color c = Color.Lerp(tempGradient.Evaluate(temp), wetnessGradient.Evaluate(wetness), 1f);
        return c;
    }

    void SetLightingColors(float temp, float wetness)
    {
        //Color a = CalculateAmbientColor(temp, wetness);
        //ambientColor = Color.Lerp(ambientColor, a, changeSpeed_ambientColor * Time.deltaTime);
        //RenderSettings.ambientLight = ambientColor;

        Color f = CalculateFogColor(temp, wetness);
        fogColor = Color.Lerp(fogColor, f, changeSpeed_ambientColor * Time.deltaTime);
        RenderSettings.fogColor = fogColor;
    }

    void SetFogDensity(float temp, float wetness)
    {
        wetness = Mathf.Clamp01(wetness - .6f);
        temp = Mathf.Clamp01(temp - .6f);
        float f = ((wetness + temp) / 2f) * fog_base;
        fog = Mathf.Lerp(fog, f, changeSpeed_fog * Time.deltaTime);
        RenderSettings.fogDensity = fog;
        //skyMat.SetFloat("_FogHeight", Mathf.Clamp01(fog/fog_base - .05f));
    }
}
