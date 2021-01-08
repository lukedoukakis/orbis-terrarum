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

    public int xSize;
    public int zSize;
    public int seed;
    public float scale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public float amplitude;

    public float slopeMagnitude;
    public float plainsLevel;
    public float waterLevel;

    public Gradient colorGradient;



    public GameObject tree;

    // Start is called before the first frame update
    void Start()
    {

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
        PlaceTrees();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            CreateShape();
            UpdateMesh();
            PlaceTrees();
        }
    }


    void CreateShape()
    {

        // initialize properties for mesh
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        triangles = new int[xSize * zSize * 6];
        uvs = new Vector2[vertices.Length];
        colors = new Color[(xSize + 1) * (zSize + 1)];

        // set up vertices/noise map
        float[,] noiseMap = Noise.GenerateNoiseMap(xSize + 1, zSize + 1, seed, scale, octaves, persistance, lacunarity);
        for (int i = 0, z = 0; z < zSize + 1; z++)
        {
            for (int x = 0; x < xSize + 1; x++)
            {
                float y = noiseMap[x, z];
                if (y < plainsLevel)
                {
                    y = Mathf.Lerp(y, plainsLevel, .9f);
                }

                Vector2 center = new Vector2((xSize) / 2, (zSize) / 2);
                float dFromCenter = Vector2.Distance(new Vector2(x, z), center);
                float dNormalized = dFromCenter / center.magnitude;
                y *= Mathf.Pow((1f - dNormalized), slopeMagnitude);
                y *= 50f * amplitude;
                vertices[i] = new Vector3(x, y, z);
                vertices[i] = new Vector3(x, y, z);

                //colors[i] = CalculateVertexColor(noiseMap[x, z], true);
                i++;
            }
        }

        // set up triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < zSize; z++)
        {
            Color c = Color.white;
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

    }


    void PlaceTrees()
    {
        Debug.Log("placing trees");

        RaycastHit hit;
        for (float z = 0; z < zSize; z += .1f)
        {
            for (float x = 0; x < xSize; x += .1f)
            {
                if(Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit, 100f)){
                    Vector3 point = hit.point;
                    if(point.y > 18f && point.y < 50f && hit.collider.gameObject.tag == "Terrain")
                    {
                        if(UnityEngine.Random.Range(0, 3) == 0)
                        {
                            GameObject t = GameObject.Instantiate(tree, point, Quaternion.identity);
                            t.transform.localScale = Vector3.one * .1f;
                        }
                    }
                }
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();

        gameObject.AddComponent<MeshCollider>();
    }


    private void OnValidate()
    {
        if(lacunarity < 1) { lacunarity = 1; }
        if(octaves < 0) { octaves = 0; }

        //CreateShape();
        //UpdateMesh();

    }


    Color CalculateVertexColor(float height, bool blend)
    {

        Color finalColor = new Color(0, 0, 0, 0);
        if (blend)
        {
            finalColor = colorGradient.Evaluate(height);
        }
        else
        {
            if(height > .75f)
            {
                finalColor = Color.white;
            }
            else
            {
                finalColor = Color.green;
            }
        }

        return finalColor;
    }
}