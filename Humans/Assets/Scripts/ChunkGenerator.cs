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
    public float xOffset;
    public float zOffset;

    List<ChunkData> ChunkDataToLoad;
    List<ChunkData> ChunkDataLoaded;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Color[] colors;

    public static float[,] TerrainMap;
    public static float[,] CliffMap;
    public static float[,] NormalMap;
    public static bool[,] TreeMap;
    public static bool[,] ShoreRockMap;

    public int chunkSize;
    public int seed;
    public float scale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public float slopeMagnitude;

    public float[] elevationLevels;

    public Gradient colorGradient;

    // feature restrictions
    [Range(.5f, 1.5f)] public float cliffRandomness;

    [Range(1, 5)] public int treeDensity;
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

    public Vector3[] normals;

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
       // }
    }



    void init()
    {
        cameraT.position = new Vector3(0, 40, 0);

        if (seed == -1) { seed = UnityEngine.Random.Range(-100000, 100000); }
        UnityEngine.Random.InitState(seed);

        ChunkDataToLoad = new List<ChunkData>();
        ChunkDataLoaded = new List<ChunkData>();

        //LoadChunk(0, 0);
    }


    void UpdateChunksToLoad()
    {
        Vector3 cameraPos = cameraT.position; cameraPos.y = 0f;
        Vector2 cameraPos_chunkSpace = new Vector2(cameraPos.x / chunkSize, cameraPos.z / chunkSize);
        Vector2 currentChunkCoord = new Vector2(Mathf.Floor(cameraPos_chunkSpace.x), Mathf.Floor(cameraPos_chunkSpace.y));
        //Vector2 currentChunkCenter = currentChunkCoord * chunkSize + new Vector2(chunkSize / 2, chunkSize / 2);

        //Debug.Log(cameraPos_chunkSpace.ToString("F3"));

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
            if (Vector2.Distance(cameraPos_chunkSpace, cd.coord + halfVec) >= 1f)
            {
                ChunkDataToLoad.Remove(cd);
            }
        }

        // add chunks in rendering range to ChunksToLoad
        foreach (Vector2 chunkCoord in neighborChunkCoords)
        {
            if(Vector2.Distance(cameraPos_chunkSpace, chunkCoord + halfVec) < 1f)
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
        chunk = cd.chunk;
        mesh = cd.mesh;
        xIndex = (int)(cd.coord.x);
        zIndex = (int)(cd.coord.y);
        xOffset = xIndex * chunkSize;
        zOffset = zIndex * chunkSize;

        /*
        GenerateCliffMap();
        GenerateTerrainMap();
        PlaceTerrain();
        GenerateTreeMap();
        GenerateShoreRockMap();
        PlaceTrees();
        PlaceShoreRocks();
        */
    }


    void PlaceTerrain()
    {

        // initialize properties for mesh
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        triangles = new int[chunkSize * chunkSize * 6];
        uvs = new Vector2[vertices.Length];
        colors = new Color[(chunkSize + 1) * (chunkSize + 1)];

        // set vertices according to TerrainMap
        for (int i = 0, z = 0; z < chunkSize + 1; z++)
        {
            for (int x = 0; x < chunkSize + 1; x++)
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
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + chunkSize + 1;
                triangles[tris + 2] = vert + 1;
              
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + chunkSize + 1;
                triangles[tris + 5] = vert + chunkSize + 2;
        
                vert++;
                tris += 6;
            }
            vert++;
        }

        // update mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();

        chunk.AddComponent<MeshCollider>();


    }


    void PlaceTrees()
    {

        foreach (Transform child in treeParent.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < treeDensity; i++)
        {
            for (int z = 0; z < chunkSize; z += 1)
            {
                for (int x = 0; x < chunkSize; x += 1)
                {
                    if (TreeMap[x, z] == true)
                    {
                        if (Physics.Raycast(new Vector3(x + xOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10, 100, z + zOffset + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10), Vector3.down, out RaycastHit hit, 100f))
                        {
                            Vector3 point = hit.point;
                            if (hit.normal.y > treeMinYNormal && hit.collider.gameObject.tag == "Terrain")
                            {
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


    public void GenerateCliffMap()
    {
        CliffMap = new float[chunkSize + 1, chunkSize + 1];

        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float perlinValue = Mathf.PerlinNoise((x) / 10f - seed + xOffset, (z) / 10f - seed + zOffset);
                CliffMap[x, z] = perlinValue;
            }
        }

    }



    public void GenerateTerrainMap()
    {

        TerrainMap = new float[chunkSize+1, chunkSize+1];

        if (scale <= 0) { scale = .0001f; }

        float heightMax = float.MinValue;
        float heightMin = float.MaxValue;

        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
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

                // update yMax and yMin for normalization
                height *= (40f * Mathf.PerlinNoise(((x + xOffset) / 120f) + 1000, ((z + zOffset) / 120f) + 1000));
                if (height > heightMax)
                {
                    heightMax = height;
                }
                else if (height < heightMin)
                {
                    heightMin = height;
                }

                TerrainMap[x, z] = height;
            }

        }

        // normalize TerrainMap values in range [0,1] and apply modifications
        float y;
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                y = Mathf.InverseLerp(heightMin, heightMax, TerrainMap[x, z]);



                // flatten land at plains level
                if (y < .5f && y > .375f)
                {
                    y = Mathf.Lerp(y, .5f, .8f);
                }

                
                // slope land into the sea based on distance from the center
                Vector2 center = new Vector2((chunkSize + 1) / 2, (chunkSize + 1) / 2);
                float dFromCenter = Vector2.Distance(new Vector2(x, z), center);
                float dNormalized = dFromCenter / center.magnitude;
                dNormalized = Mathf.Clamp(dNormalized, .25f, 1f);
                y *= Mathf.Pow((1f - dNormalized), slopeMagnitude);
                


                

                // create cliffs
                if (CliffMap[x, z] > (1f-cliffiness))
                {
                    int i = 0;
                    while(y > elevationLevels[i] && i < elevationLevels.Length)
                    {
                        i++;
                    }
                    i--;
                    if(i >= elevationLevels.Length) { i = elevationLevels.Length - 1; }
                    if (i < 0) { i = 0; }                   

                    y = Mathf.Lerp(y, elevationLevels[i], .8f);
                }
                TerrainMap[x, z] = y;
            }
        }
    }


    // generates TreeMap based off TerrainMap
    public void GenerateTreeMap()
    {
        TreeMap = new bool[chunkSize + 1, chunkSize + 1];
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                if (TerrainMap[x, z] > .31f && TerrainMap[x, z] < 1f)
                {
                    float perlinValue = Mathf.PerlinNoise((x) / 40f + seed + xOffset, (z)/ 40f + seed + zOffset);
                    if ((perlinValue > (1f - foliage)))
                    {
                        TreeMap[x, z] = true;
                    } 
                }
            }
        }
    }

    public void GenerateShoreRockMap()
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
    }


    private void OnValidate()
    {
        if(lacunarity < 1) { lacunarity = 1; }
        if(octaves < 0) { octaves = 0; }

    }

}