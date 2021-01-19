using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]

public class ChunkGenerator : MonoBehaviour
{

    public Transform cameraT;

    public GameObject chunkPrefab;
    GameObject chunk;
    Mesh mesh;

    public int xIndex;
    public int zIndex;
    public int xOffset;
    public int zOffset;

    List<ChunkData> ChunkDataToLoad;
    List<ChunkData> ChunkDataLoaded;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Color[] colors;

    public float elevationMapScale;
    public float riverMapScale;
    public float mountainMapScale;
    public float wetnessMapScale;
    public float temperatureMapScale;

    public float[,] ElevationMap;
    public float[,] MountainMap;
    public float[,] WetnessMap;
    public float[,] TemperatureMap;

    public float[,] CliffMap;
    public float[,] RiverMap;
    public float[,] TerrainMap;
    public float[,] NormalMap;
    public bool[,] TreeMap;
    public bool[,] ShoreRockMap;

    public int chunkRenderDistance;
    public int chunkSize;
    public int seed;
    public float scale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public float slopeMagnitude;

    public int elevationLevel;
    public float flatLevel;
    public float waterLevel;
    public float treeLevel;




    public Gradient colorGradient;

    // feature restrictions
    [Range(.5f, 1.5f)] public float cliffRandomness;

    [Range(1, 10)] public int treeDensity;
    [Range(.5f, 1.5f)] public float treeRandomness;
    [Range(.9f, 1)] public float treeMinYNormal;

    [Range(1, 8)] public int shoreRockDensity;
    [Range(0f, 1f)] public float shoreRockRandomness;
    [Range(.9f, 1)] public float shoreRockMinYNormal;

    // feature options
    public float cliffiness;
    public float foliage;
    public float rockiness;

    // feature assets
    public GameObject treeParent;
    public GameObject[] trees;
    public GameObject rockParent;
    public GameObject[] rocks;

    [SerializeField] GameObject WaterTilePrefab;



    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.R)) {
            UpdateChunksToLoad();
            LoadChunks();
            DeloadChunks();
       //}
    }

    void init()
    {
        cameraT.position = new Vector3(0, 40, 0);

        if (seed == -1) { seed = UnityEngine.Random.Range(-100000, 100000); }

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }


    void UpdateChunksToLoad()
    {

        
        Vector3 cameraPos = cameraT.position; cameraPos.y = 0f;
        Vector2 cameraPos_chunkSpace = new Vector2(cameraPos.x / chunkSize, cameraPos.z / chunkSize);
        Vector2 currentChunkCoord = new Vector2(Mathf.Floor(cameraPos_chunkSpace.x), Mathf.Floor(cameraPos_chunkSpace.y));

        // get neighbor chunk coordinates
        Vector2 halfVec = new Vector2(.5f, .5f);
        Vector2[] neighborChunkCoords = new Vector2[]
        {
            currentChunkCoord + Vector2.up,
            currentChunkCoord + Vector2.down,
            currentChunkCoord + Vector2.left,
            currentChunkCoord + Vector2.right,
            currentChunkCoord + Vector2.up + Vector2.right,
            currentChunkCoord + Vector2.up + Vector2.left,
            currentChunkCoord + Vector2.down + Vector2.right,
            currentChunkCoord + Vector2.up + Vector2.left
        };

        // remove chunks out of rendering range from ChunksToLoad
        foreach (ChunkData cd in ChunkDataToLoad)
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
 
        foreach (ChunkData cd in ChunkDataToLoad)
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
        
        foreach (ChunkData loadedCd in ChunkDataLoaded)
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

        cd.RiverMap = GenerateRiverMap();
        cd.MountainMap = GenerateMountainMap();
        cd.TemperatureMap = GenerateTemperatureMap();
        cd.ElevationMap = GenerateElevationMap();
        //cd.WetnessMap = GenerateWetnessMap();
        cd.CliffMap = GenerateCliffMap();
        cd.TerrainMap = GenerateTerrainMap();
        cd.TreeMap = GenerateTreeMap();
        cd.ShoreRockMap = GenerateShoreRockMap();
        PlaceTerrain();
        PlaceTrees();
        //PlaceShoreRocks();

        GenerateWater(cd.sea, WaterTilePrefab, xOffset, zOffset);
        

    }


    public float[,] GenerateElevationMap()
    {
        ElevationMap = new float[chunkSize + 2, chunkSize + 2];
        float[] levels = new float[] { 0f, .1f, .2f, .3f, 4f, .5f, .6f, .7f, .8f, .9f, 1f };
        float[] adds = new float[] { 0f, .05f, .1f, .15f, .2f, .25f, .3f, .35f, .4f, .45f };

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                float perlinValue = Mathf.PerlinNoise((x + xOffset - seed + .01f) / elevationMapScale, (z + zOffset - seed + .01f) / elevationMapScale);
                
                
                perlinValue = Mathf.InverseLerp(.45f, .55f, perlinValue);
                perlinValue = Mathf.Clamp(perlinValue, 0f, 1f);
                int i = 0;
                while (perlinValue > levels[i] && i < levels.Length)
                {
                    i++;
                }
                i--;
                if (i >= levels.Length) { i = levels.Length - 1; }
                if (i < 0) { i = 0; }

                perlinValue = adds[i];
                
                ElevationMap[x, z] = perlinValue;

            }
        }

        return ElevationMap;
    }

    public float[,] GenerateMountainMap()
    {
        MountainMap = new float[chunkSize + 2, chunkSize + 2];

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                float perlinValue = Mathf.PerlinNoise((x + xOffset - seed) / mountainMapScale, (z + zOffset - seed) / mountainMapScale);
                perlinValue = Mathf.InverseLerp(.45f, .55f, perlinValue);
                perlinValue = Mathf.Clamp(perlinValue, 0f, 1f);
                MountainMap[x, z] = perlinValue;
            }
        }
        return MountainMap;
    }

    public float[,] GenerateTemperatureMap()
    {
        TemperatureMap = new float[chunkSize + 2, chunkSize + 2];

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                float perlinValue = Mathf.PerlinNoise((x + xOffset - seed) / mountainMapScale, (z + zOffset - seed) / mountainMapScale);
                TemperatureMap[x, z] = 1f - perlinValue;
            }
        }
        return MountainMap;
    }



    public float[,] GenerateCliffMap()
    {
        CliffMap = new float[chunkSize + 2, chunkSize + 2];

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                float perlinValue = Mathf.PerlinNoise((float)((z + zOffset + seed + 2000) / 10f + .01f), (float)((x + xOffset + seed + 2000) / 10f + .01f));
                CliffMap[x, z] = perlinValue;
            }
        }
        return CliffMap;
    }

    public float[,] GenerateRiverMap()
    {

        RiverMap = new float[chunkSize + 2, chunkSize + 2];

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {
                
                float riverX = (3f + 96f * Mathf.PerlinNoise((float)((z + zOffset + seed) / riverMapScale + .01f), (float)((x + xOffset + seed) / riverMapScale + .01f)));
                float distance = Mathf.Abs(riverX - (x + xOffset));
                if (distance < 60f)
                {
                    if(distance < 5f)
                    {
                        RiverMap[x, z] = 1f;
                    }
                    else
                    {
                        float distanceNorm = Mathf.InverseLerp(0f, 60f, distance);
                        distanceNorm = Mathf.Clamp(distanceNorm, .1f, 1f);
                        RiverMap[x, z] = 1f - distanceNorm;
                    }
                    
                    

                }
                else
                {
                    RiverMap[x, z] = 0f;
                }

            }
        }
        return RiverMap;
    }





    public float[,] GenerateTerrainMap()
    {

        TerrainMap = new float[chunkSize + 2, chunkSize + 2];

        if (scale <= 0) { scale = .0001f; }

        for (int z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float height = 0f;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + xOffset) / scale * frequency + seed;
                    float sampleZ = (z + zOffset) / scale * frequency + seed;

                    float scale2 = scale + 5f;
                    float scale3 = scale - 5f;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    perlinValue += Mathf.PerlinNoise(sampleX * scale / scale2, sampleZ) * 2 - 1;
                    perlinValue -= Mathf.PerlinNoise(sampleX * scale / scale3, sampleZ) * 2 - 1;

                    height += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                height *= (40f * Mathf.PerlinNoise(((x + xOffset) / 120f) + 1000, ((z + zOffset) / 120f) + 1000));

                //ABS and INVERT, and normalize value
                height = Mathf.Abs(height);
                height *= -1f;
                height = Mathf.InverseLerp(-75f, .01f, height);

                // flatten land at plains level
                if (height < flatLevel)
                {
                    height = Mathf.Lerp(height, flatLevel, .95f);
                    height = Mathf.Clamp(height, waterLevel + .01f, 1f);
                }
                else if (height > flatLevel)
                {
                    height = Mathf.Lerp(height, flatLevel, (1f - MountainMap[x, z]));
                }

                // add rivers
                if(RiverMap[x,z] > 0f)
                {
                    if (RiverMap[x, z] == 1f)
                    {
                        height = Mathf.Lerp(height, waterLevel - .01f, RiverMap[x, z]);
                    }
                    else
                    {
                        height = Mathf.Lerp(height, flatLevel, RiverMap[x, z]);
                    }
                }

                /*
                // add height from ElevationMap
                if(height >= flatLevel + .001f)
                {
                    height += ElevationMap[x, z] * .5f;
                }
                */
                
                /*
                // create cliffs
                if (CliffMap[x, z] > (1f - cliffiness))
                {

                    float[] elevationLevels = new float[] {flatLevel, .95f, 1f };
                    int i = 0;
                    while (height > elevationLevels[i] && i < elevationLevels.Length)
                    {
                        i++;
                    }
                    i--;
                    if (i >= elevationLevels.Length) { i = elevationLevels.Length - 1; }
                    if (i < 0) { i = 0; }

                    height = elevationLevels[i];
                }
                */

                TerrainMap[x, z] = height;
            }

        }
        return TerrainMap;
    }
 


    // generates TreeMap based off TerrainMap
    public bool[,] GenerateTreeMap()
    {
        TreeMap = new bool[chunkSize + 2, chunkSize + 2];
        for (int z = 0; z < chunkSize+1; z++)
        {
            for (int x = 0; x < chunkSize+1; x++)
            {
                if (TerrainMap[x, z] >= treeLevel)
                {
                    if(RiverMap[x, z] < 1f)
                    {
                        float perlinValue = Mathf.PerlinNoise((x + seed + xOffset + 1000) / 75f, (z + seed + zOffset + 1000) / 75f);
                        if ((perlinValue > (1f - foliage)))
                        {
                            TreeMap[x, z] = true;
                        }
                    } 
                }
            }
        }
        return TreeMap;
    }

    public bool[,] GenerateShoreRockMap()
    {
        ShoreRockMap = new bool[chunkSize + 1, chunkSize + 1];
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                if (TerrainMap[x, z] > .28f && TerrainMap[x, z] < .31f)
                {
                    float perlinValue = Mathf.PerlinNoise((x + xOffset) / 10f + seed, (z + zOffset) / 10f + seed);
                    if ((perlinValue > (1f - rockiness)))
                    {
                        ShoreRockMap[x, z] = true;
                    }
                }

            }
        }
        return ShoreRockMap;
    }






    void PlaceTerrain()
    {
        
        // initialize properties for mesh
        vertices = new Vector3[(chunkSize + 2) * (chunkSize + 2)];
        triangles = new int[(chunkSize+1) * (chunkSize+1) * 6];
        uvs = new Vector2[vertices.Length];
        colors = new Color[vertices.Length];

        // set vertices according to TerrainMap
        for (int i = 0, z = 0; z < chunkSize + 2; z++)
        {
            for (int x = 0; x < chunkSize + 2; x++)
            {
                float y = TerrainMap[x, z];
                y *= 50f;
                vertices[i] = new Vector3(x + xOffset, y, z + zOffset);
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
                uvs[i] = new Vector2((float)x/(chunkSize-2) + xOffset, (float)z / (chunkSize-2) + zOffset);
                i++;
            }
        }

        // update mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        chunk.AddComponent<MeshCollider>();
        

    }


    void PlaceTrees()
    {

        foreach (Transform child in treeParent.transform)
        {
            //GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < treeDensity; i++)
        {
            for (int z = 0; z < chunkSize + 1; z++)
            {
                for (int x = 0; x < chunkSize + 1; x++)
                {
                    if (TreeMap[x, z] == true)
                    {
                        if (Physics.Raycast(new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10, 100, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10), Vector3.down, out RaycastHit hit, 100f))
                        {
                            Vector3 point = hit.point;
                            if (hit.normal.y > treeMinYNormal && hit.collider.gameObject.tag == "Terrain")
                            {
                                //Debug.Log("Placing tree");
                                GameObject t = GameObject.Instantiate(trees[x%trees.Length], point, Quaternion.identity, treeParent.transform);
                                t.transform.localScale = Vector3.one * .05f;
                            }
                        }
                    }
                }
            } 
        }
    }



    void PlaceShoreRocks()
    {
        foreach (Transform child in rockParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < shoreRockDensity; i++)
        {
            for (int z = 0; z < chunkSize; z += 1)
            {
                for (int x = 0; x < chunkSize; x += 1)
                {
                    if (ShoreRockMap[x, z] == true)
                    {
                        if (Physics.Raycast(new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * shoreRockRandomness * 10, 100, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * shoreRockRandomness * 10), Vector3.down, out RaycastHit hit, 100f))
                        {
                            Vector3 point = hit.point;
                            if (hit.normal.y > shoreRockMinYNormal && hit.collider.gameObject.tag == "Terrain")
                            {
                                GameObject t = GameObject.Instantiate(rocks[x%rocks.Length], point + Vector3.down*.075f, Quaternion.identity, rockParent.transform);
                                t.transform.localScale = Vector3.one * .25f * Mathf.Pow(UnityEngine.Random.value, .1f);
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
        AddWaterCollider(parent);

    }

    void PlaceWaterTiles(GameObject parent, GameObject waterUnit, float xOffset, float zOffset)
    {

        Vector3 placePos;
        for (int x = 0; x < chunkSize/5f; x++)
        {
            for (int z = 0; z < chunkSize / 5f; z++)
            {
                placePos = new Vector3(x * 5 + xOffset, (waterLevel) * 50f, z * 5 + zOffset);
                GameObject w = GameObject.Instantiate(waterUnit, placePos, Quaternion.identity, parent.transform);
            }
        }
        parent.transform.Rotate(Vector3.up);
    }

    void AddWaterCollider(GameObject parent)
    {
        BoxCollider bc = parent.AddComponent<BoxCollider>();
        bc.size = new Vector3(chunkSize, 4f, chunkSize);
        bc.center = new Vector3(chunkSize / 2f, waterLevel * 50f - bc.size.y / 2f, chunkSize / 2f);
        bc.isTrigger = true;
    }


    private void OnValidate()
    {
        if(lacunarity < 1) { lacunarity = 1; }
        if(octaves < 0) { octaves = 0; }

    }

}