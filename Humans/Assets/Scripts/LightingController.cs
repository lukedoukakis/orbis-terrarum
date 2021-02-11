using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{

    public Camera MainCamera;
    public Light mainLight;

    public Gradient tempGradient;
    public Gradient wetnessGradient;
    float temperature;
    float wetness;

    Color ambientColor;
    float fog;

    static float fog_base = .3f;
    static float changeSpeed_ambientColor = .5f;
    static float changeSpeed_fog = .5f;

    static float period = .1f;
    static float updateTime;


    // Update is called once per frame
    void FixedUpdate()
    {
        if(Time.fixedTime >= updateTime)
        {
            UpdateLighting();
            updateTime = Time.fixedTime + period;
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
        temperature = AreaConditions.Temperature;
        wetness = AreaConditions.Wetness;
        SetAmbientColor(temperature, wetness);
        SetFog(temperature, wetness);
    }


    Color CalculateAmbientColor(float temp, float wetness)
    {

        Color c = Color.Lerp(tempGradient.Evaluate(temp), wetnessGradient.Evaluate(wetness), .5f);
        return c;
    }

    void SetAmbientColor(float temp, float wetness)
    {
        Color c = CalculateAmbientColor(temp, wetness);
        ambientColor = Color.Lerp(ambientColor, c, changeSpeed_ambientColor * Time.deltaTime);
        RenderSettings.ambientLight = ambientColor;
    }

    void SetFog(float temp, float wetness)
    {
        float f = wetness * fog_base;
        f = Mathf.Clamp(f, 0f, Mathf.Pow(temp, 5f));
        fog = Mathf.Lerp(fog, f, changeSpeed_fog * Time.deltaTime);
        RenderSettings.fogDensity = fog;
    }
}
