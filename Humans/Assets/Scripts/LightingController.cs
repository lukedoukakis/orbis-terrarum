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


    // Update is called once per frame
    void Update()
    {

        AreaConditions.GetAreaConditions(MainCamera.transform.position);

        //Debug.Log("Area temperature: " + ac.temperature);

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
        ambientColor = Color.Lerp(ambientColor, c, 1f * Time.deltaTime);
        RenderSettings.ambientLight = ambientColor;
    }

    void SetFog(float temp, float wetness)
    {
        float f = wetness * .5f;
        f = Mathf.Clamp(f, 0f, Mathf.Pow(temp, 5f));
        fog = Mathf.Lerp(fog, f, 1f * Time.deltaTime);
        RenderSettings.fogDensity = fog;
    }
}
