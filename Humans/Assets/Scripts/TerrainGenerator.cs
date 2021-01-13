using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]

public class TerrainGenerator : MonoBehaviour
{


    Mesh mesh;

    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Color[] colors;

    public static float[,] TerrainMap;
    public static float[,] CliffMap;
    public static float[,] NormalMap;
    public static bool[,] TreeMap;
    public static bool[,] ShoreRockMap;

    public int xSize;
    public int zSize;
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


        if (seed == -1) { seed = UnityEngine.Random.Range(-100000, 100000); }
        UnityEngine.Random.InitState(seed);
       
      
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        CreateScene();


    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            CreateScene();
        }
    }

    void CreateScene()
    {

        GenerateCliffMap();

        GenerateTerrainMap();
        PlaceTerrain();

     
        GenerateTreeMap();
        PlaceTrees();

        GenerateShoreRockMap();
        PlaceShoreRocks();


    }


    void PlaceTerrain()
    {

        // initialize properties for mesh
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        triangles = new int[xSize * zSize * 6];
        uvs = new Vector2[vertices.Length];
        colors = new Color[(xSize + 1) * (zSize + 1)];

        // set vertices according to TerrainMap
        for (int i = 0, z = 0; z < zSize + 1; z++)
        {
            for (int x = 0; x < xSize + 1; x++)
            {
                float y = TerrainMap[x, z];
                y *= 50f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        // set up triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
              
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
        
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

        gameObject.AddComponent<MeshCollider>();

    }


    void PlaceTrees()
    {
        for (int i = 0; i < treeDensity; i++)
        {
            for (int z = 0; z < zSize; z += 1)
            {
                for (int x = 0; x < xSize; x += 1)
                {
                    if (TreeMap[x, z] == true)
                    {
                        if (Physics.Raycast(new Vector3(x + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10, 100, z + (UnityEngine.Random.value * 2f - 1f) * treeRandomness * 10), Vector3.down, out RaycastHit hit, 100f))
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
        for (int i = 0; i < shoreRockDensity; i++)
        {
            for (int z = 0; z < zSize; z += 1)
            {
                for (int x = 0; x < xSize; x += 1)
                {
                    if (ShoreRockMap[x, z] == true)
                    {
                        if (Physics.Raycast(new Vector3(x + (UnityEngine.Random.value * 2f - 1f) * shoreRockRandomness * 10, 100, z + (UnityEngine.Random.value * 2f - 1f) * shoreRockRandomness * 10), Vector3.down, out RaycastHit hit, 100f))
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
        CliffMap = new float[xSize + 1, zSize + 1];

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float perlinValue = Mathf.PerlinNoise(x / 10f - seed, z / 10f - seed);
                CliffMap[x, z] = perlinValue;
            }
        }

    }



    public void GenerateTerrainMap()
    {

        TerrainMap = new float[xSize+1, zSize+1];

        if (scale <= 0) { scale = .0001f; }

        float heightMax = float.MinValue;
        float heightMin = float.MaxValue;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float height = 0f;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + seed;
                    float sampleZ = z / scale * frequency + seed;

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
                height *= (40f * Mathf.PerlinNoise((x / 120f) + 1000, (z / 120f) + 1000));
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
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                y = Mathf.InverseLerp(heightMin, heightMax, TerrainMap[x, z]);



                // flatten land at plains level
                if (y < .5f && y > .375f)
                {
                    y = Mathf.Lerp(y, .5f, .8f);
                }


                // slope land into the sea based on distance from the center
                Vector2 center = new Vector2((xSize + 1) / 2, (zSize + 1) / 2);
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
        TreeMap = new bool[xSize + 1, zSize + 1];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (TerrainMap[x, z] > .31f && TerrainMap[x, z] < 1f)
                {
                    float perlinValue = Mathf.PerlinNoise(x / 40f + seed, z / 40f + seed);
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
        ShoreRockMap = new bool[xSize + 1, zSize + 1];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                if (TerrainMap[x, z] > .28f && TerrainMap[x, z] < .31f)
                {
                    float perlinValue = Mathf.PerlinNoise(x / 10f + seed, z / 10f + seed);
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