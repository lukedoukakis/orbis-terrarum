using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChunkGenerator : MonoBehaviour
{

    public static int chunkSize = 30;
    public static int chunkRenderDistance = 2;
    public static float ElevationAmplitude = 120;

    public Transform cameraT;
    public Vector3 cameraPos;
    public Vector2 cameraPos_chunkSpace;
    public Vector2 currentChunkCoord;
   

    public GameObject chunkPrefab;
    GameObject chunk;
    Mesh mesh;

    int xIndex;
    int zIndex;
    int xOffset;
    int zOffset;

    static List<ChunkData> ChunkDataToLoad;
    static List<ChunkData> ChunkDataLoaded;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Color[] colors;

    public float elevationMapScale;
    public float riverMapScale;
    public float wetnessMapScale;
    public float temperatureMapScale;

    float[,] ElevationMap;
    float[,] MountainMap;
    float[,] WetnessMap;
    float[,] TemperatureMap;
    float[,] FreshWaterMap;
    float[,] HeightMap;
    bool[,] TreeMap;

    public int seed;
    public float scale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public float flatLevel;
    public float waterLevel;
    public float treeLevel;


    // feature restrictions
    [Range(0f, 1f)] public float treeDensity;
    [Range(.5f, 1.5f)] public float treeRandomness;


    // feature assets
    GameObject treeParent;
    [SerializeField] GameObject[] treesCactus;
    [SerializeField] GameObject[] treesPalm;
    [SerializeField] GameObject[] treesJungle;
    [SerializeField] GameObject[] treesPlains;
    [SerializeField] GameObject[] treesDeciduous;
    [SerializeField] GameObject[] treesFir;
    [SerializeField] GameObject[] treesSnowyFir;


    [SerializeField] GameObject WaterTilePrefab;


    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    private void Update()
    {
        UpdateChunksToLoad();
        LoadChunks();
        DeloadChunks();
    }

    void init()
    {

        if (seed == -1) { seed = UnityEngine.Random.Range(-100000, 100000); }

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        currentChunkCoord = Vector2.positiveInfinity;
    }


    void UpdateChunksToLoad()
    {

        
        cameraPos = cameraT.position; cameraPos.y = 0f;
        cameraPos_chunkSpace = ToChunkSpace(cameraPos);
        currentChunkCoord = new Vector2(Mathf.Floor(cameraPos_chunkSpace.x), Mathf.Floor(cameraPos_chunkSpace.y));


        // get neighbor chunk coordinates
        Vector2 halfVec = Vector3.one/2f;
        Vector2[] neighborChunkCoords = new Vector2[(int)Mathf.Pow(chunkRenderDistance*2, 2)];
        int i = 0;
        for (int z = (int)currentChunkCoord.y-chunkRenderDistance; z < (int)currentChunkCoord.y+chunkRenderDistance; z++)
        {
            for (int x = (int)currentChunkCoord.x - chunkRenderDistance; x < (int)currentChunkCoord.x + chunkRenderDistance; x++)
            {
                neighborChunkCoords[i] = new Vector2(x, z);
                i++;
            }
        }


        // remove chunks out of rendering range from ChunksToLoad
        foreach (ChunkData cd in ChunkDataToLoad.ToArray())
        {
            if (Vector2.Distance(cameraPos_chunkSpace, cd.coord + halfVec) >= chunkRenderDistance)
            {
                ChunkDataToLoad.Remove(cd);
            }
        }

        // add chunks in rendering range to ChunksToLoad
        foreach (Vector2 chunkCoord in neighborChunkCoords)
        {
            if(Vector2.Distance(cameraPos_chunkSpace, chunkCoord + halfVec) < chunkRenderDistance)
            {

                int index = ChunkDataToLoad.FindIndex(cd => cd.coord == chunkCoord);
                if (index < 0)
                {
                    ChunkDataToLoad.Add(new ChunkData(chunkCoord));
                }
            }
        }
        
    }

    void LoadChunks()
    {
        foreach (ChunkData cd in ChunkDataToLoad.ToArray())
        {
            if (!cd.loaded)
            {
                LoadChunk(cd);
                ChunkDataLoaded.Add(cd);
            }
        }
    }

    void DeloadChunks()
    {
        
        foreach (ChunkData loadedCd in ChunkDataLoaded.ToArray())
        {
            int index = ChunkDataToLoad.FindIndex(cd => cd.coord == loadedCd.coord);
            if (index < 0)
            {
                loadedCd.Deload();
                ChunkDataLoaded.Remove(loadedCd);
            }
        }
        
    }


    void LoadChunk(ChunkData cd)
    {
      
        cd.init(chunkPrefab);
        UnityEngine.Random.InitState(cd.randomState);
        chunk = cd.chunk;
        treeParent = cd.trees;
        mesh = cd.mesh;
        xIndex = (int)(cd.coord.x);
        zIndex = (int)(cd.coord.y);
        xOffset = xIndex * chunkSize;
        zOffset = zIndex * chunkSize;

        
        GenerateTerrain();
        cd.TemperatureMap = TemperatureMap;
        cd.MountainMap = MountainMap;
        cd.ElevationMap = ElevationMap;
        cd.FreshWaterMap = FreshWaterMap;
        cd.WetnessMap = WetnessMap;
        cd.HeightMap = HeightMap;
        cd.TreeMap = TreeMap;

        PlaceTerrain();
        PlaceTrees();
        GenerateWater(cd.sea, WaterTilePrefab, xOffset, zOffset);

    }


    void GenerateTerrain()
    {

        TemperatureMap = new float[chunkSize + 2, chunkSize + 2];
        MountainMap = new float[chunkSize + 2, chunkSize + 2];
        ElevationMap = new float[chunkSize + 2, chunkSize + 2];
        FreshWaterMap = new float[chunkSize + 2, chunkSize + 2];
        WetnessMap = new float[chunkSize + 2, chunkSize + 2];
        HeightMap = new float[chunkSize + 2, chunkSize + 2];
        TreeMap = new bool[chunkSize + 2, chunkSize + 2];

        float elevationValue, mountainValue, temperatureValue, freshWaterValue, wetnessValue, heightValue;
        bool treeValue;

        float tMod, rough;
        if (scale <= 0) { scale = .0001f; }

        // loop start
        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {

                // TemperatureMap
                temperatureValue = Mathf.PerlinNoise((x + xOffset + seed + .01f) / temperatureMapScale, (z + zOffset + seed + .01f) / temperatureMapScale);
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 30f, (z + zOffset + .01f) / 30f) + .5f, .3f) - 1f;
                temperatureValue += rough;

                temperatureValue = Mathf.InverseLerp(0f, .7f, temperatureValue);

                // -------------------------------------------------------

                // MountainMap
                mountainValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / elevationMapScale, (z + zOffset - seed + .01f) / elevationMapScale);

                // -------------------------------------------------------

                // ElevationMap
                elevationValue = Mathf.Pow(Mathf.PerlinNoise((x + xOffset - seed + .01f) / elevationMapScale, (z + zOffset - seed + .01f) / elevationMapScale) + .5f, .5f) - 1f;
                if (elevationValue < 0f)
                {
                    if(mountainValue > .5f)
                    {
                        elevationValue = 0f;
                    }
                    else
                    {
                        rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, .3f) - 1f;
                        //rough *= 1f - MountainMap[x, z];
                        elevationValue += rough;
                    }
                 
                }

                // -------------------------------------------------------

                // FreshWaterMap
                float riverWindingScale = 75f + (25f * (mountainValue * 2f - 1f));
                freshWaterValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / riverWindingScale, (z + zOffset - seed + .01f) / riverWindingScale) * 2f - 1f;
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, .3f) - 1f;
                freshWaterValue += rough;
                freshWaterValue -= Mathf.PerlinNoise((x + xOffset + .01f) / elevationMapScale, (z + zOffset + .01f) / elevationMapScale)/2f;
                freshWaterValue = Mathf.Abs(freshWaterValue);
                freshWaterValue *= -1f;
                freshWaterValue += 1f;
                if (freshWaterValue > .95f) { freshWaterValue = 1f; }
                else if (freshWaterValue > .85f)
                {
                    freshWaterValue = .95f;
                }
                else
                {
                    freshWaterValue = Mathf.InverseLerp(0f, .85f, freshWaterValue);
                    freshWaterValue = Mathf.Pow(freshWaterValue, 7f*temperatureValue);
                }
                // GOOD FOR CREATING LARGE LAKES AND INLETS FOR WETLAND AREA
                /*
                float f = Mathf.PerlinNoise((float)((z + zOffset + seed) / (riverMapScale + 50f) +.01f), (float)((x + xOffset + seed) / (riverMapScale + 50f) + .01f));
                float rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, .3f) - 1f;
                f += rough;
                float dif = Mathf.Abs(f - .7f);
                if (dif < .1f)
                {
                    f = 1f;
                }
                else
                {
                    f = 1f / (1f + dif);
                }
                RiverMap[x, z] = f;
                */

                // -------------------------------------------------------

                // WetnessMap
                //wetnessValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / wetnessMapScale, (z + zOffset - seed + .01f) / wetnessMapScale) * 2f - 1f;
                wetnessValue = freshWaterValue;
                if(freshWaterValue < .9f)
                {
                    if (elevationValue < .02f)
                    {
                        wetnessValue = .9f;
                    }
                }


                // -------------------------------------------------------

                // HeightMap
                heightValue = 0f;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + xOffset) / scale * frequency + seed;
                    float sampleZ = (z + zOffset) / scale * frequency + seed;

                    float scale2 = scale + 5f;
                    float scale3 = scale - 5f;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    perlinValue += Mathf.PerlinNoise(sampleX * scale / scale2, sampleZ) * 2 - 1;
                    perlinValue -= Mathf.PerlinNoise(sampleX * scale / scale3, sampleZ) * 2 - 1;

                    heightValue += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                heightValue *= (40f * Mathf.PerlinNoise(((x + xOffset) / 120f) + 1000, ((z + zOffset) / 120f) + 1000));

                //ABS and INVERT, and normalize value
                heightValue = Mathf.Abs(heightValue);
                heightValue *= -1f;
                heightValue = Mathf.InverseLerp(-75f, .01f, heightValue);

                // flatten land at plains level
                if (heightValue < flatLevel)
                {
                    heightValue = flatLevel;
                }
                else if (heightValue > flatLevel)
                {
                    tMod = 1f + Mathf.InverseLerp(.5f, .75f, temperatureValue);
                    heightValue = Mathf.Lerp(heightValue, flatLevel, (1f - mountainValue) * tMod);
                }

                // apply ElevationMap with respect to TemperatureMap
                if (elevationValue > 0f)
                {
                    tMod = 1f - Mathf.Lerp(0f, 1f, temperatureValue);
                }
                else
                {
                    tMod = 1f;
                }
                heightValue += elevationValue * tMod;

                // flatten high areas with respect to TemperatureMap
                tMod = 1f - Mathf.InverseLerp(temperatureValue, .45f, .55f);
                float heightCutoff = Mathf.Lerp(flatLevel + .01f, 1f, tMod);
                if (heightValue > heightCutoff)
                {
                    heightValue = Mathf.Lerp(heightValue, heightCutoff, tMod);
                }

                // create ocean where height is below flatLevel
                if (heightValue < flatLevel)
                {
                    heightValue = waterLevel - .01f;
                }
                else if (heightValue >= waterLevel)
                {

                    // add fresh water
                    if (freshWaterValue > 0f)
                    {
                        if (freshWaterValue == 1f)
                        {
                            //heightValue = Mathf.Lerp(heightValue, waterLevel - .01f, freshWaterValue);
                            heightValue = waterLevel - .01f;
                        }
                        else
                        {
                            rough = 0f;
                            if (freshWaterValue <= .85f)
                            {
                                rough = (Mathf.PerlinNoise((float)(x + xOffset) / .1f + .01f, (float)((z + zOffset) / .1f + .01f)) * 2f - 1f) / 1f;
                            }
                            heightValue = Mathf.Lerp(heightValue, flatLevel, freshWaterValue + rough);
                        }
                    }
                }

                // create slight roughness in terrain
                if(Biome.GetBiomeType(temperatureValue, wetnessValue, heightValue, freshWaterValue) == (int)Biome.BiomeType.Desert)
                {
                    heightValue += .0016f * (Mathf.Sin((x + xOffset - seed + .01f) / .025f) * Mathf.Sin((z + zOffset - seed + .01f) / 2f));
                }
                else
                {
                    heightValue += .005f * Mathf.PerlinNoise((x + xOffset - seed + .01f) / 2f, (z + zOffset - seed + .01f) / 2f);
                }

                

                // -------------------------------------------------------

                // TreeMap
                if (wetnessValue != -1)
                {
                    treeValue = true;
                }
                else { treeValue = false; }

                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                MountainMap[x, z] = mountainValue;
                ElevationMap[x, z] = elevationValue;
                FreshWaterMap[x, z] = freshWaterValue;
                WetnessMap[x, z] = wetnessValue;
                HeightMap[x, z] = heightValue;
                TreeMap[x, z] = treeValue;

            }
        }

    }



    int checkTempZone(int x, int z)
    {
        float f = TemperatureMap[x, z];

        float[] cutoffs = new float[] { .2f, .4f, .6f, .8f, 1f };

        int i = 0;
        while (f > cutoffs[i] && i < cutoffs.Length)
        {
            i++;
        }
        i--;
        if (i >= cutoffs.Length) { i = cutoffs.Length - 1; }
        if (i < 0) { i = 0; }

        int tempZone = 0;
        return tempZone;

    }

    Color SetVertexColor(float temperature, float wetness)
    {
        Color c = new Color();

        float wet, temp;

        wet = wetness;
        temp = temperature + .3f;
        temp = Mathf.Abs(1f - temp);
        temp = Mathf.Pow(temp, 2f);

        // wetness
        c.g = 255f * wet * Mathf.Pow(temp, .1f);

        // desert yellow (add red)
        c.r = 255f * Mathf.Min(temperature, 1f - wetness);
        return c;
    }



    void PlaceTerrain()
    {
        
        // initialize properties for mesh
        vertices = new Vector3[(chunkSize + 2) * (chunkSize + 2)];
        triangles = new int[(chunkSize+1) * (chunkSize+1) * 6];
        uvs = new Vector2[vertices.Length];
        colors = new Color[vertices.Length];

        // set vertices according to HeightMap
        for (int i = 0, z = 0; z < chunkSize + 2; z++)
        {
            for (int x = 0; x < chunkSize + 2; x++)
            {
                float y = HeightMap[x, z];
                y *= ElevationAmplitude;
                vertices[i] = new Vector3(x + xOffset, y, z + zOffset);
                colors[i] = SetVertexColor(TemperatureMap[x, z], WetnessMap[x, z]);
                i++;
            }
        }
        

        // set up triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < chunkSize+1; z++)
        {
            for (int x = 0; x < chunkSize+1; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + chunkSize + 2;
                triangles[tris + 2] = vert + 1;
              
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + chunkSize + 2;
                triangles[tris + 5] = vert + chunkSize + 3;
        
                vert++;
                tris += 6;
            }
            vert++;
        }

        for (int i = 0, z = 0; z < chunkSize+1; z++)
        {
            for (int x = 0; x < chunkSize+1; x++)
            {
                uvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);
                i++;
            }
        }

        // update mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        chunk.AddComponent<MeshCollider>();


    }


    void PlaceTrees()
    {
       

        GameObject[][] treesSelection = new GameObject[][] { treesPlains, treesCactus, treesPalm, treesJungle, treesDeciduous, treesFir, treesSnowyFir };
        float[] treeScales = new float[] { .06f, .1f, .05f, .1f, .09f, .05f, .05f };
        int[] treePassesMultipliers = new int[] { 1, 1, 2, 10, 10, 10, 10 };
        float[] treeMinYNormals = new float[] { .99f, .99f, .7f, .7f, .7f, .7f, .7f };
        float[] treeAngleMultipliers = new float[] { 0f, 0f, .25f, .5f, .5f, .1f, .1f };

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                int biomeType = Biome.GetBiomeType(TemperatureMap[x, z], WetnessMap[x, z], HeightMap[x, z], FreshWaterMap[x, z]);
                
                // tree placement
                if (TreeMap[x, z])
                {
                    int index;
                    switch (biomeType)
                    {
                        case (int)Biome.BiomeType.Plains: index = 0; break;
                        case (int)Biome.BiomeType.Desert: index = 1; break;
                        case (int)Biome.BiomeType.Swamp: index = 2; break;
                        case (int)Biome.BiomeType.Jungle: index = 3; break;
                        case (int)Biome.BiomeType.Forest: index = 4; break;
                        case (int)Biome.BiomeType.Taiga: index = 5; break;
                        case (int)Biome.BiomeType.SnowyTaiga: index = 6; break;
                        default: index = -1; break;
                    }
                    if (index >= 0)
                    {

                        GameObject[] _trees = treesSelection[index];
                        float treeScale = treeScales[index];
                        int passesMultipler = treePassesMultipliers[index];
                        float treeMinYNormal = treeMinYNormals[index];
                        float treeAngleMultiplier = treeAngleMultipliers[index];
                        Quaternion uprightRot;
                        Quaternion slantedRot;

                        int passes = (int)(WetnessMap[x, z] * passesMultipler * treeDensity);
                        passes = Mathf.Clamp(passes, 1, passes);
                        for (int j = 0; j < treeDensity * passes; j++)
                        {

                            Vector3 castVec = new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10, ElevationAmplitude, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10);
                            if (Physics.Raycast(castVec, Vector3.down, out RaycastHit hit, ElevationAmplitude - (waterLevel * ElevationAmplitude)))
                            {
                                Vector3 point = hit.point;
                                bool xn = hit.normal.x > 0f;
                                if (hit.normal.y > treeMinYNormal)
                                {

                                    uprightRot = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up);
                                    slantedRot = Quaternion.FromToRotation(transform.up, hit.normal);
                                    GameObject t = GameObject.Instantiate(_trees[x % _trees.Length], point, Quaternion.Slerp(uprightRot, slantedRot, treeAngleMultiplier), treeParent.transform);
                                    t.transform.localScale = Vector3.one * treeScale * Mathf.Pow(UnityEngine.Random.value + .5f, .2f);
                                }
                            }

                        }
                    }
                }
            } 
        }
    }


    

    void GenerateWater(GameObject parent, GameObject waterUnit, float xOffset, float zOffset)
    {

        PlaceWaterTiles(parent, waterUnit, xOffset, zOffset);
        //AddWaterCollider(parent);

    }

    void PlaceWaterTiles(GameObject parent, GameObject waterUnit, float xOffset, float zOffset)
    {

        Vector3 placePos;
        for (int x = 0; x < chunkSize / 2f; x++)
        {
            for (int z = 0; z < chunkSize / 2f; z++)
            {
                placePos = new Vector3(x * 5 + xOffset, (waterLevel) * ElevationAmplitude, z * 5 + zOffset);
                GameObject w = GameObject.Instantiate(waterUnit, placePos, Quaternion.identity, parent.transform);
            }
        }
        parent.transform.Rotate(Vector3.up);
    }

    void AddWaterCollider(GameObject parent)
    {
        BoxCollider bc = parent.AddComponent<BoxCollider>();
        bc.size = new Vector3(chunkSize, 4f, chunkSize);
        bc.center = new Vector3(chunkSize / 2f, waterLevel * ElevationAmplitude - bc.size.y / 2f, chunkSize / 2f);
        bc.isTrigger = true;
    }



    // returns given position translated to chunk coordinates, based on chunkSize
    public static Vector2 ToChunkSpace(Vector3 position)
    {
        return new Vector2(position.x / (chunkSize+1), position.z / (chunkSize+1));
    }

    public static ChunkData GetChunk(Vector2 chunkCoord)
    {
        //Debug.Log(chunkCoord);
        foreach (ChunkData cd in ChunkDataLoaded.ToArray())
        {
            if (cd.coord == chunkCoord)
            {
                return cd;
            }
        }
        Debug.Log("ChunkGenerator: chunk from given position is not loaded!");
        return null;
    }

    // retrieves ChunkData in ChunkDataLoaded associated with the raw position given
    public static ChunkData GetChunk(Vector3 position)
    {
        Vector2 position_chunkSpace = ToChunkSpace(position);
        Vector2 chunkCoord = new Vector2((int)position_chunkSpace.x, (int)(position_chunkSpace.y));
        return GetChunk(chunkCoord);
    }

}