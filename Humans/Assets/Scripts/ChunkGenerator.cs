using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChunkGenerator : MonoBehaviour
{

    public static int ChunkSize = 30;
    public static int ChunkRenderDistance = 4;
    public static float ElevationAmplitude = 120;
    static GameObject Chunk;
    static GameObject Terrain;
    static GameObject Water;

    public Transform cameraT;
    public Vector3 cameraPos;
    public Vector2 cameraPos_chunkSpace;
    public Vector2 currentChunkCoord;


    [SerializeField] MeshFilter TerrainMeshFilter;
    [SerializeField] MeshFilter WaterMeshFilter;
    Mesh TerrainMesh;
    Mesh WaterMesh;

    public GameObject chunkPrefab;

    int xIndex;
    int zIndex;
    int xOffset;
    int zOffset;

    static List<ChunkData> ChunkDataToLoad;
    static List<ChunkData> ChunkDataLoaded;

    static Vector3[] TerrainVertices;
    static int[] TerrainTriangles;
    static Vector2[] TerrainUvs;
    static Color[] TerrainColors;
    static Vector3[] WaterVertices;
    static int[] WaterTriangles;
    static Vector2[] WaterUvs;
    static Color[] WaterColors;

    [SerializeField] float waterAlpha;

    public float mountainMapScale;
    public float elevationMapScale;
    public float riverMapScale;
    public float wetnessMapScale;
    public float temperatureMapScale;

    float[,] TemperatureMap;
    float[,] HumidityMap;
    float[,] ElevationMap;
    float[,] MountainMap;
    int[,] BiomeMap;
    float[,] FreshWaterMap;
    float[,] WetnessMap;
    float[,] HeightMap;
    float[,] WaterHeightMap;
    bool[,] TreeMap;

    public int seed;
    public float scale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public float flatLevel;
    public float seaLevel;
    public float treeLevel;


    // feature
    GameObject Trees;
    [Range(0f, 1f)] public float treeDensity;
    [Range(.5f, 1.5f)] public float treeRandomness;


    // Start is called before the first frame update
    void Start()
    {
        Init();
        Biome.Init();
    }

    private void Update()
    {
        if (Biome.initialized)
        {
            UpdateChunksToLoad();
            LoadChunks();
            DeloadChunks();
        }
       
    }

    void Init()
    {

        if (seed == -1) { seed = UnityEngine.Random.Range(-100000, 100000); }

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();

        TerrainMesh = new Mesh();
        WaterMesh = new Mesh();
        TerrainMeshFilter.mesh = TerrainMesh;
        WaterMeshFilter.mesh = WaterMesh;
        currentChunkCoord = Vector2.positiveInfinity;
    }


    void UpdateChunksToLoad()
    {

        
        cameraPos = cameraT.position; cameraPos.y = 0f;
        cameraPos_chunkSpace = ToChunkSpace(cameraPos);
        currentChunkCoord = new Vector2(Mathf.Floor(cameraPos_chunkSpace.x), Mathf.Floor(cameraPos_chunkSpace.y));


        // get neighbor chunk coordinates
        Vector2 halfVec = Vector3.one/2f;
        Vector2[] neighborChunkCoords = new Vector2[(int)Mathf.Pow(ChunkRenderDistance*2, 2)];
        int i = 0;
        for (int z = (int)currentChunkCoord.y-ChunkRenderDistance; z < (int)currentChunkCoord.y+ChunkRenderDistance; z++)
        {
            for (int x = (int)currentChunkCoord.x - ChunkRenderDistance; x < (int)currentChunkCoord.x + ChunkRenderDistance; x++)
            {
                neighborChunkCoords[i] = new Vector2(x, z);
                i++;
            }
        }


        // remove chunks out of rendering range from ChunksToLoad
        foreach (ChunkData cd in ChunkDataToLoad.ToArray())
        {
            if (Vector2.Distance(cameraPos_chunkSpace, cd.coord + halfVec) >= ChunkRenderDistance)
            {
                ChunkDataToLoad.Remove(cd);
            }
        }

        // add chunks in rendering range to ChunksToLoad
        foreach (Vector2 chunkCoord in neighborChunkCoords)
        {
            if(Vector2.Distance(cameraPos_chunkSpace, chunkCoord + halfVec) < ChunkRenderDistance)
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
      
        cd.Init(chunkPrefab);
        UnityEngine.Random.InitState(cd.randomState);
        Chunk = cd.chunk;
        Terrain = cd.terrain;
        Water = cd.water;
        TerrainMesh = cd.terrainMesh;
        WaterMesh = cd.waterMesh;
        Trees = cd.trees;
        xIndex = (int)(cd.coord.x);
        zIndex = (int)(cd.coord.y);
        xOffset = xIndex * ChunkSize;
        zOffset = zIndex * ChunkSize;

        
        GenerateTerrain();
        cd.TemperatureMap = TemperatureMap;
        cd.HumidityMap = HumidityMap;
        cd.ElevationMap = ElevationMap;
        cd.MountainMap = MountainMap;
        cd.BiomeMap = BiomeMap;
        cd.FreshWaterMap = FreshWaterMap;
        cd.WetnessMap = WetnessMap;
        cd.HeightMap = HeightMap;
        cd.WaterHeightMap = WaterHeightMap;
        cd.TreeMap = TreeMap;

        PlaceTerrainAndWater();
        PlaceTrees();

    }


    void GenerateTerrain()
    {

        TemperatureMap = new float[ChunkSize + 2, ChunkSize + 2];
        HumidityMap = new float[ChunkSize + 2, ChunkSize + 2];
        ElevationMap = new float[ChunkSize + 2, ChunkSize + 2];
        MountainMap = new float[ChunkSize + 2, ChunkSize + 2];
        BiomeMap = new int[ChunkSize + 2, ChunkSize + 2];
        FreshWaterMap = new float[ChunkSize + 2, ChunkSize + 2];
        WetnessMap = new float[ChunkSize + 2, ChunkSize + 2];
        HeightMap = new float[ChunkSize + 2, ChunkSize + 2];
        WaterHeightMap = new float[ChunkSize + 2, ChunkSize + 2];
        TreeMap = new bool[ChunkSize + 2, ChunkSize + 2];

        float temperatureValue, humidityValue, elevationValue, mountainValue, freshWaterValue, wetnessValue, heightValue, waterHeightValue;
        int biomeValue;
        bool treeValue;

        float tMod, rough;
        if (scale <= 0) { scale = .0001f; }

        // loop start
        for (int z = 0; z < ChunkSize + 1; z++)
        {
            for (int x = 0; x < ChunkSize + 1; x++)
            {

                // TemperatureMap
                temperatureValue = Mathf.PerlinNoise((x + xOffset + seed + .01f) / temperatureMapScale, (z + zOffset + seed + .01f) / temperatureMapScale);
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 30f, (z + zOffset + .01f) / 30f) + .5f, .3f) - 1f;
                temperatureValue += rough;
                temperatureValue = Mathf.Clamp(temperatureValue, 0f, 1f);

                // -------------------------------------------------------

                // HumidityMap
                humidityValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / temperatureMapScale, (z + zOffset - seed + .01f) / temperatureMapScale);
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 30f, (z + zOffset + .01f) / 30f) + .5f, .3f) - 1f;
                humidityValue += rough;
                humidityValue = Mathf.Clamp(humidityValue, 0f, 1f);

                // -------------------------------------------------------

                // MountainMap
                mountainValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / mountainMapScale, (z + zOffset - seed + .01f) / mountainMapScale);
                //rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 30f, (z + zOffset + .01f) / 30f) + .5f, .3f) - 1f;
                //mountainValue += rough;

                // -------------------------------------------------------

                // ElevationMap
                elevationValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / elevationMapScale, (z + zOffset - seed + .01f) / elevationMapScale);

                float oceanCutoff = .5f + Mathf.Pow((1f-mountainValue), 2f);
                if (elevationValue < oceanCutoff)
                {
                    elevationValue = -1f;
                }
                else
                {
                    elevationValue = Mathf.InverseLerp(oceanCutoff, 1f, elevationValue);
                }

                // -------------------------------------------------------

                // BiomeMap

                biomeValue = Biome.GetBiome(temperatureValue, humidityValue, elevationValue, mountainValue);

                // -------------------------------------------------------

                // FreshWaterMap
                if (elevationValue != -1f)
                {
                    float riverWindingScale = 75f + (25f * (mountainValue * 2f - 1f));
                    freshWaterValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / riverWindingScale, (z + zOffset - seed + .01f) / riverWindingScale) * 2f - 1f;
                    rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, .3f) - 1f;
                    freshWaterValue += rough;
                    freshWaterValue -= Mathf.PerlinNoise((x + xOffset + .01f) / elevationMapScale, (z + zOffset + .01f) / elevationMapScale) / 2f;
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
                        freshWaterValue = Mathf.Pow(freshWaterValue, 7f * temperatureValue);
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
                }
                else
                {
                    freshWaterValue = 0f;
                }

                // -------------------------------------------------------

                // WetnessMap
                
                wetnessValue = freshWaterValue;
                float fwThreshhold = .9f + Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, 1f) - 1f;
                if (freshWaterValue < fwThreshhold)
                {
                    if (elevationValue < .02f)
                    {
                        wetnessValue = fwThreshhold;
                    }
                }
                wetnessValue = Mathf.Clamp(wetnessValue, .3f, 1f);
                float humidityValue_mod = Mathf.Clamp(humidityValue, .75f, 1f);
                wetnessValue *= humidityValue_mod;


                // -------------------------------------------------------

                // HeightMap

                float flat;
                if (elevationValue != -1f)
                {
                    flat = flatLevel + Mathf.Pow(elevationValue + .5f, .05f) - 1f;
                    flat = Mathf.Clamp(flat, flatLevel, 1f);

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

                    // flatten land at flat level
                    if (heightValue < flat)
                    {
                        heightValue = flat;
                    }
                    else if (heightValue > flat)
                    {
                        //tMod = 1f + Mathf.InverseLerp(.5f, .75f, temperatureValue);
                        float t = Mathf.InverseLerp(.3f, .7f, 1f - mountainValue);
                        heightValue = Mathf.Lerp(heightValue, flat, t);
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
                    float heightCutoff = Mathf.Lerp(flat + .01f, 1f, tMod);
                    if (heightValue > heightCutoff)
                    {
                        heightValue = Mathf.Lerp(heightValue, heightCutoff, tMod);
                    }

                    // dip land under seaLevel where height is below flatLevel
                    if (heightValue < flat)
                    {
                        heightValue = seaLevel - .01f;
                    }
                    else if (heightValue >= flat)
                    {

                        // add fresh water
                        if (freshWaterValue > 0f)
                        {
                            if (freshWaterValue == 1f)
                            {
                                //heightValue = Mathf.Lerp(heightValue, seaLevel - .01f, freshWaterValue);
                                heightValue = seaLevel - .01f;
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
                    if (Biome.GetBiome(temperatureValue, wetnessValue, heightValue, freshWaterValue) == (int)Biome.BiomeType.Desert)
                    {
                        heightValue += .0016f * (Mathf.Sin((x + xOffset - seed + .01f) / .025f) * Mathf.Sin((z + zOffset - seed + .01f) / 2f));
                    }
                    else
                    {
                        heightValue += .005f * Mathf.PerlinNoise((x + xOffset - seed + .01f) / 2f, (z + zOffset - seed + .01f) / 2f);
                    }
                }
                else
                {
                    heightValue = seaLevel - .01f;
                }

                waterHeightValue = seaLevel;
                // -------------------------------------------------------

                // TreeMap
                if (heightValue >= flatLevel)
                {
                    treeValue = true;
                }
                else { treeValue = false; }

                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                MountainMap[x, z] = mountainValue;
                ElevationMap[x, z] = elevationValue;
                BiomeMap[x, z] = biomeValue;
                FreshWaterMap[x, z] = freshWaterValue;
                WetnessMap[x, z] = wetnessValue;
                HeightMap[x, z] = heightValue;
                WaterHeightMap[x, z] = waterHeightValue;
                TreeMap[x, z] = treeValue;

                //Debug.Log(elevationValue);

            }
        }

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



    void PlaceTerrainAndWater()
    {
        
        // initialize properties for meshes
        TerrainVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        TerrainTriangles = new int[(ChunkSize+1) * (ChunkSize+1) * 6];
        TerrainUvs = new Vector2[TerrainVertices.Length];
        TerrainColors = new Color[TerrainVertices.Length];
        WaterVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        WaterTriangles = new int[(ChunkSize + 1) * (ChunkSize + 1) * 6];
        WaterUvs = new Vector2[TerrainVertices.Length];
        WaterColors = new Color[TerrainVertices.Length];


        // set terrain vertices according to HeightMap, and water vertices according to waterLevel
        bool edgeZ, edgeX;
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            if (z == 0 || z == ChunkSize + 1) { edgeZ = true; } else { edgeZ = false; }
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                if (x == 0 || x == ChunkSize + 1) { edgeX = true; } else { edgeX = false; }
                TerrainVertices[i] = new Vector3(x + xOffset, HeightMap[x, z] * ElevationAmplitude, z + zOffset);
                TerrainColors[i] = SetVertexColor(TemperatureMap[x, z], WetnessMap[x, z]);
                WaterVertices[i] = new Vector3(x + xOffset, seaLevel * ElevationAmplitude, z + zOffset);
                Color waterColor = new Color();
                if (edgeZ || edgeX){ waterColor.a = waterAlpha/8; } else { waterColor.a = waterAlpha; }
                WaterColors[i] = waterColor;

                i++;
            }
        }
        

        // set up triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < ChunkSize+1; z++)
        {
            for (int x = 0; x < ChunkSize+1; x++)
            {
                TerrainTriangles[tris + 0] = vert + 0;
                TerrainTriangles[tris + 1] = vert + ChunkSize + 2;
                TerrainTriangles[tris + 2] = vert + 1;
                TerrainTriangles[tris + 3] = vert + 1;
                TerrainTriangles[tris + 4] = vert + ChunkSize + 2;
                TerrainTriangles[tris + 5] = vert + ChunkSize + 3;
                WaterTriangles[tris + 0] = vert + 0;
                WaterTriangles[tris + 1] = vert + ChunkSize + 2;
                WaterTriangles[tris + 2] = vert + 1;
                WaterTriangles[tris + 3] = vert + 1;
                WaterTriangles[tris + 4] = vert + ChunkSize + 2;
                WaterTriangles[tris + 5] = vert + ChunkSize + 3;

                vert++;
                tris += 6;
            }
            vert++;
        }

        for (int i = 0, z = 0; z < ChunkSize+1; z++)
        {
            for (int x = 0; x < ChunkSize+1; x++)
            {
                TerrainUvs[i] = new Vector2((float)x + xOffset, (float)z + zOffset);
                WaterUvs[i] = TerrainUvs[i];
                i++;
            }
        }

        // update meshes
        TerrainMesh.Clear();
        TerrainMesh.vertices = TerrainVertices;
        TerrainMesh.triangles = TerrainTriangles;
        TerrainMesh.uv = TerrainUvs;
        TerrainMesh.colors = TerrainColors;
        TerrainMesh.RecalculateNormals();
        WaterMesh.Clear();
        WaterMesh.vertices = WaterVertices;
        WaterMesh.triangles = WaterTriangles;
        WaterMesh.uv = WaterUvs;
        WaterMesh.colors = WaterColors;
        WaterMesh.RecalculateNormals();

        Terrain.AddComponent<MeshCollider>();
    }


    void PlaceTrees()
    {

        GameObject[][] treesSelection = new GameObject[][] { };
        float[] treeScales = new float[] { -1f, .1f, .05f, .1f, .06f, .09f, .05f, .05f };
        int[] treePassesMultipliers = new int[] { -1, 1, 2, 10, 1, 10, 10, 10 };
        float[] treeMinYNormals = new float[] { -1f, .99f, .7f, .7f, .99f, .7f, .7f, .7f };
        float[] treeAngleMultipliers = new float[] { -1f, 0f, .25f, .5f, .0f, .5f, .1f, .1f };

        for (int z = 0; z < ChunkSize + 1; z++)
        {
            for (int x = 0; x < ChunkSize + 1; x++)
            {
                // tree placement
                if (TreeMap[x, z])
                {
                    GameObject tree = Biome.GetTree(BiomeMap[x, z]);
                    if (tree != null)
                    {
                        //Debug.Log("placing tree");
                        Tuple<float, float, float, float> info = TreeInfo.GetPlacementRequirements(tree.GetComponent<TreeInfo>().type);
                        float treeScale = info.Item1;
                        int passesMultipler = (int)(info.Item2 * 10f);
                        float treeMinYNormal = info.Item3;
                        float treeAngleMultiplier = info.Item4;
                        //Debug.Log(treeScale + " " + passesMultipler + " " + treeMinYNormal + " " + treeAngleMultiplier);
                        Quaternion uprightRot;
                        Quaternion slantedRot;

                        int passes = (int)(WetnessMap[x, z] * passesMultipler * treeDensity);
                        passes = Mathf.Clamp(passes, 1, passes);
                        for (int j = 0; j < treeDensity * passes; j++)
                        {
                            Vector3 castVec = new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10, ElevationAmplitude, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10);
                            if (Physics.Raycast(castVec, Vector3.down, out RaycastHit hit, ElevationAmplitude - (seaLevel * ElevationAmplitude)))
                            {
                                Vector3 point = hit.point;
                                bool xn = hit.normal.x > 0f;
                                if (hit.normal.y > treeMinYNormal)
                                {

                                    uprightRot = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up);
                                    slantedRot = Quaternion.FromToRotation(transform.up, hit.normal);

                                    tree = GameObject.Instantiate(tree, point, Quaternion.Slerp(uprightRot, slantedRot, treeAngleMultiplier), Trees.transform);
                                    tree.transform.localScale = Vector3.one * treeScale * Mathf.Pow(UnityEngine.Random.value + .5f, .2f);
                                }
                            }
                        }
                    }
                }
            } 
        }
    }


    // returns given position translated to chunk coordinates, based on chunkSize
    public static Vector2 ToChunkSpace(Vector3 position)
    {
        return new Vector2(position.x / (ChunkSize+1), position.z / (ChunkSize+1));
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

    public float Posterize(float value, float maxValue, int sections)
    {
        float ret = -1;

        float sectionSize = maxValue / sections;
        for(int i = 0; i < sections; i++)
        {
            ret = sectionSize * (i + 1);
            if(value <= ret)
            {
                return ret;
            }
        }




        return ret;
    }

}