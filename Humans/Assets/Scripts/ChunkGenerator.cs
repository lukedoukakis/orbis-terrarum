using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChunkGenerator : MonoBehaviour
{
    public static int Seed = 100;
    public static int ChunkSize = 50;
    public static int ChunkRenderDistance = 2;
    public static float ElevationAmplitude = 180;
    public static float MinElevation = -.292893219f;
    public static float MaxElevation = .224744871f;
    public static int MountainMapScale = 200;
    public static float ElevationMapScale = 2300;
    public static int TemperatureMapScale = 450;
    public static int HumidityMapScale = 300;
    public static bool LoadingChunks;
    static GameObject Chunk;
    static GameObject Terrain;
    static GameObject Water;

    public Transform cameraT;
    public Vector3 cameraPos;
    public Vector2 cameraPos_chunkSpace;
    public Vector2 currentChunkCoord;


    [SerializeField] CameraController cameraController;
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

    float[,] TemperatureMap;
    float[,] HumidityMap;
    float[,] ElevationMap;
    float[,] MountainMap;
    int[,] BiomeMap;
    float[,] FreshWaterMap;
    float[,] WetnessMap;
    float[,] HeightMap;
    bool[,] TreeMap;

    public float scale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public float flatLevel;
    public float seaLevel;
    public float snowLevel;


    // feature
    GameObject Trees;
    [Range(0f, 1f)] public float treeDensity;


    // Start is called before the first frame update
    void Start()
    {

        Seed = UnityEngine.Random.Range(0, 1000);

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

        if (Seed == -1) { Seed = UnityEngine.Random.Range(-100000, 100000); }

        //RiverGenerator.Generate();

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


        cameraPos = cameraT.position;
        cameraPos_chunkSpace = ToChunkSpace(cameraPos);
        currentChunkCoord = new Vector2(Mathf.Floor(cameraPos_chunkSpace.x), Mathf.Floor(cameraPos_chunkSpace.y));


        // get neighbor chunk coordinates
        Vector2 halfVec = Vector3.one / 2f;
        Vector2[] neighborChunkCoords = new Vector2[(int)Mathf.Pow(ChunkRenderDistance * 2, 2)];
        int i = 0;
        for (int z = (int)currentChunkCoord.y - ChunkRenderDistance; z < (int)currentChunkCoord.y + ChunkRenderDistance; z++)
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
            if (Vector2.Distance(cameraPos_chunkSpace, chunkCoord + halfVec) < ChunkRenderDistance)
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

        LoadingChunks = true;
        foreach (ChunkData cd in ChunkDataToLoad.ToArray())
        {
            if (!cd.loaded)
            {
                LoadChunk(cd);
                ChunkDataLoaded.Add(cd);
            }
        }
        LoadingChunks = false;
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


        GenerateTerrainMaps();
        cd.TemperatureMap = TemperatureMap;
        cd.HumidityMap = HumidityMap;
        cd.ElevationMap = ElevationMap;
        cd.MountainMap = MountainMap;
        cd.BiomeMap = BiomeMap;
        cd.FreshWaterMap = FreshWaterMap;
        cd.WetnessMap = WetnessMap;
        cd.HeightMap = HeightMap;
        cd.TreeMap = TreeMap;

        PlaceTerrainAndWater();
        PlaceFeatures();

    }


    void GenerateTerrainMaps()
    {

        TemperatureMap = new float[ChunkSize + 2, ChunkSize + 2];
        HumidityMap = new float[ChunkSize + 2, ChunkSize + 2];
        ElevationMap = new float[ChunkSize + 2, ChunkSize + 2];
        MountainMap = new float[ChunkSize + 2, ChunkSize + 2];
        BiomeMap = new int[ChunkSize + 2, ChunkSize + 2];
        FreshWaterMap = new float[ChunkSize + 2, ChunkSize + 2];
        WetnessMap = new float[ChunkSize + 2, ChunkSize + 2];
        HeightMap = new float[ChunkSize + 2, ChunkSize + 2];
        TreeMap = new bool[ChunkSize + 2, ChunkSize + 2];

        float temperatureValue, humidityValue, elevationValue, mountainValue, freshWaterValue, wetnessValue, heightValue;
        float elevationValueSmooth;
        int biomeValue;
        bool treeValue;

        float tMod, rough;
        if (scale <= 0) { scale = .0001f; }

        // loop start
        for (int z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {



                float e = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / ElevationMapScale, (z + zOffset - Seed + .01f) / ElevationMapScale);

                // [-.292893219, .224744871]
                elevationValue = elevationValueSmooth = Mathf.Pow(e + .5f, .5f) - 1f;
                //Debug.Log(elevationValue);
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 30f, (z + zOffset + .01f) / 30f) + .5f, .1f) - 1f;
                //rough += Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, .1f) - 1f;
                elevationValue += rough;

                // -------------------------------------------------------

                // MountainMap [0, 1]
                mountainValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / ElevationMapScale, (z + zOffset - Seed + .01f) / ElevationMapScale);
                mountainValue = Mathf.Pow(mountainValue, 2f);

                // -------------------------------------------------------



                // TemperatureMap [0, 1]
                temperatureValue = 1.2f - (e);
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 50f, (z + zOffset + .01f) / 50f) + .5f, .1f) - 1f;
                temperatureValue += rough;

                float latitudeMod;
                latitudeMod = ((Mathf.PerlinNoise((x + xOffset + .01f) / TemperatureMapScale, (z + zOffset + .01f) / TemperatureMapScale) + .5f) - 1f) * 1.5f;

                temperatureValue += latitudeMod;
                temperatureValue = Mathf.Clamp01(temperatureValue);
                //temperatureValue = .8f;


                // -------------------------------------------------------

                // HumidityMap [0, 1]
                humidityValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / HumidityMapScale, (z + zOffset - Seed + .01f) / HumidityMapScale);
                humidityValue += mountainValue * .5f;
                humidityValue = Mathf.Clamp01(humidityValue);
                humidityValue = Mathf.InverseLerp(0f, 1f, humidityValue);
                //humidityValue = .9f;
                // -------------------------------------------------------



                // FreshWaterMap [0, 1]

                float riverWindingScale = 75f + (25f * (mountainValue * 2f - 1f));
                freshWaterValue = Mathf.PerlinNoise((x + xOffset - Seed + .01f) / riverWindingScale, (z + zOffset - Seed + .01f) / riverWindingScale) * 2f - 1f;
                rough = Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, .3f) - 1f;
                freshWaterValue += rough;
                freshWaterValue -= Mathf.PerlinNoise((x + xOffset + .01f) / 400f, (z + zOffset + .01f) / 400f) / 2f;
                freshWaterValue = Mathf.Abs(freshWaterValue);
                freshWaterValue *= -1f;
                freshWaterValue += 1f;
                if (freshWaterValue > .93f) { freshWaterValue = Mathf.Pow(freshWaterValue, 1f); }
                else if (freshWaterValue > .85f)
                {
                    freshWaterValue = .93f;
                }
                else
                {
                    //freshWaterValue = Mathf.Pow(freshWaterValue, 7f * temperatureValue);
                    freshWaterValue = Mathf.Pow(freshWaterValue, 1f);
                }


                //freshWaterValue = -1f;
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

                // WetnessMap [0, 1]
                wetnessValue = freshWaterValue;
                float fwThreshhold = 1f + Mathf.Pow(Mathf.PerlinNoise((x + xOffset + .01f) / 10f, (z + zOffset + .01f) / 10f) + .5f, 1f) - 1f;
                fwThreshhold = Mathf.Clamp01(fwThreshhold);
                if (freshWaterValue < fwThreshhold)
                {
                    if (elevationValueSmooth < .02f)
                    {
                        wetnessValue = Mathf.Max(wetnessValue, fwThreshhold);
                    }
                }
                float mtnMod = Mathf.Pow(mountainValue + .5f, 1f) - 1f;
                wetnessValue += mtnMod;

                wetnessValue = Mathf.Clamp01(wetnessValue);

                // -------------------------------------------------------

                // BiomeMap
                biomeValue = Biome.GetBiome(temperatureValue, humidityValue);

                // -------------------------------------------------------

                // HeightMap

                heightValue = 0f;
                float amplitude = 1;
                float frequency = 1;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + xOffset) / scale * frequency + Seed;
                    float sampleZ = (z + zOffset) / scale * frequency + Seed;

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

                // reduce hills
                if (heightValue < flatLevel)
                {
                    heightValue = flatLevel;
                }
                else if (heightValue >= flatLevel)
                {
                    tMod = Mathf.Pow(temperatureValue + .5f, .6f);
                    heightValue = Mathf.Lerp(heightValue, flatLevel, (1f - mountainValue) * tMod);
                }

                // apply ElevationMap with respect to TemperatureMap
                heightValue += elevationValueSmooth * .1f;

                // flatten high areas with respect to TemperatureMap
                tMod = 1f - Mathf.InverseLerp(temperatureValue, .45f, .55f);
                float heightCutoff = Mathf.Lerp(flatLevel + .01f, 1f, tMod);
                heightCutoff = Mathf.Clamp(heightCutoff, .86f + (flatLevel - flatLevel), 1f);
                if (heightValue > heightCutoff)
                {
                    heightValue = Mathf.Lerp(heightValue, heightCutoff, tMod);
                }

                // create ocean where height is below flatLevel
                if (heightValue < flatLevel)
                {
                    //heightValue = (seaLevel - .01f);
                    float f = Mathf.InverseLerp(0f, .004f, flatLevel - heightValue);
                    freshWaterValue = Mathf.Max(freshWaterValue, f);
                }

                if (heightValue >= seaLevel)
                {

                    // add fresh water
                    if (freshWaterValue > 0f)
                    {
                        if (freshWaterValue > .93f)
                        {
                            float f = Mathf.InverseLerp(.93f, 1f, freshWaterValue);
                            heightValue = Mathf.Lerp(Mathf.Min(flatLevel, heightValue), (seaLevel - .01f), f);
                        }
                        else
                        {
                            rough = 0f;
                            if (freshWaterValue <= .9f)
                            {
                                rough = (Mathf.PerlinNoise((float)(x + xOffset) / .1f + .01f, (float)((z + zOffset) / .1f + .01f)) * 2f - 1f) / 1f;
                            }
                            heightValue = Mathf.Lerp(heightValue, flatLevel, freshWaterValue + rough);
                        }
                    }


                }
                else{
                    if (freshWaterValue > 0f)
                    {
                        if (freshWaterValue > .93f)
                        {
                            float f = Mathf.InverseLerp(.93f, 1f, freshWaterValue);
                            heightValue = Mathf.Lerp(Mathf.Min(flatLevel, heightValue), (seaLevel - .01f), f);
                        }
                    }
                }

                // create slight roughness in terrain
                if (biomeValue == (int)Biome.BiomeType.Desert)
                {
                    float duneMag = .012f * (1f - Mathf.Pow(Mathf.Clamp01(freshWaterValue), 1.3f)) * (1f - Mathf.Pow(wetnessValue, 1.2f));
                    heightValue += duneMag * (1f - Mathf.Abs(Mathf.Sin((x + xOffset - Seed + .01f + Mathf.Sin(z+zOffset)*8f) / 15f)));
                }
                else
                {
                    heightValue += .0025f * Mathf.PerlinNoise((x + xOffset - Seed + .01f) / 2f, (z + zOffset - Seed + .01f) / 2f);
                }

                // -------------------------------------------------------

                // TreeMap
                if (heightValue > seaLevel && freshWaterValue < 1f)
                {
                    treeValue = true;
                }
                else { treeValue = false; }

                // -------------------------------------------------------

                TemperatureMap[x, z] = temperatureValue;
                HumidityMap[x, z] = humidityValue;
                MountainMap[x, z] = mountainValue;
                ElevationMap[x, z] = elevationValue;
                BiomeMap[x, z] = biomeValue;
                FreshWaterMap[x, z] = freshWaterValue;
                WetnessMap[x, z] = wetnessValue;
                HeightMap[x, z] = heightValue;
                TreeMap[x, z] = treeValue;

            }
        }

    }

    Color SetVertexColor(int biome, float height, float temperature, float humidity, float wetness, float fw)
    {

        
        Color c = new Color();

        
        if (biome == (int)Biome.BiomeType.Tundra || biome == (int)Biome.BiomeType.SnowyTaiga)
        {
            c.b = 255f;
            return c;
        }
        else if (temperature < (5f / 11f))
        {
            if (height > snowLevel + ((1f - temperature) * .05f) * (UnityEngine.Random.value * .05f))
            {
                c.b = 255f;
                return c;
            }
        }
        

        // wetness (darkness of land)
        if (biome == (int)Biome.BiomeType.Desert)
        {
            c.g = 255f * Mathf.Clamp01(Mathf.Pow(fw, 1.5f)) *.3f;
        }else if(biome == (int)Biome.BiomeType.Chaparral){
            c.g = 255f * Mathf.Clamp01(Mathf.Pow(fw, 1.5f)) *.3f;
        }else{
            c.g = 255f * wetness;
        }
        

        // yellowness
        c.r = 255f * Mathf.Lerp(1f - wetness, temperature, .5f);

        return c;
    }



    void PlaceTerrainAndWater()
    {

        // initialize properties for meshes
        TerrainVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        TerrainTriangles = new int[(ChunkSize + 2) * (ChunkSize + 2) * 6];
        TerrainUvs = new Vector2[TerrainVertices.Length];
        TerrainColors = new Color[TerrainVertices.Length];
        WaterVertices = new Vector3[(ChunkSize + 2) * (ChunkSize + 2)];
        WaterTriangles = new int[(ChunkSize + 1) * (ChunkSize + 1) * 6];
        WaterUvs = new Vector2[TerrainVertices.Length];
        WaterColors = new Color[TerrainVertices.Length];


        // set terrain vertices according to HeightMap
        for (int i = 0, z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {
                TerrainVertices[i] = new Vector3(x + xOffset, HeightMap[x, z] * ElevationAmplitude, z + zOffset);
                TerrainColors[i] = SetVertexColor(BiomeMap[x, z], HeightMap[x, z], TemperatureMap[x, z], HumidityMap[x, z], WetnessMap[x, z], FreshWaterMap[x, z]);
                WaterVertices[i] = new Vector3(x + xOffset, seaLevel * ElevationAmplitude, z + zOffset);
                Color waterColor = new Color();
                waterColor.a = waterAlpha;
                WaterColors[i] = waterColor;
                i++;
            }
        }


        // set up triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < ChunkSize + 1; z++)
        {
            for (int x = 0; x < ChunkSize + 1; x++)
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

        for (int i = 0, z = 0; z < ChunkSize + 1; z++)
        {
            for (int x = 0; x < ChunkSize + 1; x++)
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


    void PlaceFeatures()
    {
        int biome;
        float wetness;
        float temperature;
        float height;
        float fw;

        for (int z = 0; z < ChunkSize + 2; z++)
        {
            for (int x = 0; x < ChunkSize + 2; x++)
            {

                biome = BiomeMap[x, z];
                wetness = WetnessMap[x, z];
                temperature = TemperatureMap[x, z];
                height = HeightMap[x, z];
                fw = FreshWaterMap[x, z];
                bool onWater;

            
                if (TreeMap[x, z])
                {
                    if(seaLevel >= height-.00025f){
                        onWater = seaLevel - height <= .0018f;
                    }
                    else{ onWater = false; }

                    // tree placement
                    var treeTuple = Biome.GetTree(biome, wetness, fw);
                    if (treeTuple != null && treeTuple.Item2.Item2 > 0f)
                    {
                        PlaceTreeBundle(treeTuple, wetness, onWater, x, z);
                    }

                    // feature placement
                    var featureTuple = Biome.GetFeature(biome, wetness, fw, onWater);
                    if(featureTuple != null)
                    {
                        PlaceTreeBundle(featureTuple, wetness, onWater, x, z);
                    }
                }
            }
        }
    }

    void PlaceTreeBundle(Tuple<GameObject, Tuple<float, float, float, float, float>> treeTuple, float wetness, bool onWater, int x, int z)
    {
        GameObject tree = treeTuple.Item1;
        float treeScale = treeTuple.Item2.Item1;
        int passesMultipler = (int)(treeTuple.Item2.Item2 * 10f);
        float treeMinYNormal = treeTuple.Item2.Item3;
        float treeAngleMultiplier = treeTuple.Item2.Item4;
        float spreadMultiplier = treeTuple.Item2.Item5;
        Quaternion uprightRot;
        Quaternion slantedRot;
        Vector3 castVec;
        float castLength;

        int passes = (int)(wetness * passesMultipler * treeDensity);
        passes = Mathf.Clamp(passes, (int)(UnityEngine.Random.value + .25f), passes);
        if (passes > 0)
        {
            for (int j = 0; j < treeDensity * passes; j++)
            {
                castVec = new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * spreadMultiplier * 10, ElevationAmplitude, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * spreadMultiplier * 10);
                
                if(onWater){ castLength = ElevationAmplitude - ((seaLevel - .0003f)* ElevationAmplitude); }
                else{ castLength = ElevationAmplitude - ((seaLevel + .002f)* ElevationAmplitude); }
                if (Physics.Raycast(castVec, Vector3.down, out RaycastHit hit, castLength))
                {
                    Vector3 point = hit.point;
                    if (hit.normal.y > treeMinYNormal)
                    {
                        if(onWater){
                            if(point.y < (seaLevel - .0018f) * ElevationAmplitude){
                                uprightRot = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up);
                                slantedRot = Quaternion.FromToRotation(transform.up, hit.normal);

                                tree = GameObject.Instantiate(tree, point, Quaternion.Slerp(uprightRot, slantedRot, treeAngleMultiplier), Trees.transform);
                                tree.transform.localScale = Vector3.one * treeScale * Mathf.Pow(UnityEngine.Random.value + .5f, .2f);
                            }
                        }

                        uprightRot = Quaternion.AngleAxis(UnityEngine.Random.value * 360f, Vector3.up);
                        slantedRot = Quaternion.FromToRotation(transform.up, hit.normal);

                        tree = GameObject.Instantiate(tree, point, Quaternion.Slerp(uprightRot, slantedRot, treeAngleMultiplier), Trees.transform);
                        tree.transform.localScale = Vector3.one * treeScale * Mathf.Pow(UnityEngine.Random.value + .5f, .2f);
                    }
                }
            }
        }
    }


    // returns given position translated to chunk coordinates, based on chunkSize
    public static Vector2 ToChunkSpace(Vector3 position)
    {
        return new Vector2(position.x / (ChunkSize), position.z / (ChunkSize));
    }

    // retrieve ChunkData in ChunkDataLoaded associated with the chunk coordinate
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

    // retrieve ChunkData in ChunkDataLoaded associated with the raw position given
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
        for (int i = 0; i < sections; i++)
        {
            ret = sectionSize * (i + 1);
            if (value <= ret)
            {
                return ret;
            }
        }




        return ret;
    }

}