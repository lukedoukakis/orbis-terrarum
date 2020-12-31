using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]

public class TerrainGenerator : MonoBehaviour
{

    Mesh mesh;

    public Vector3[] vertices;
    public int[] triangles;
    public Color[] colors;

    public int xSize;
    public int zSize;
    public int seed;
    public float scale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public TerrainType[] terrainTypes;
   

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //CreateShape();
        //UpdateMesh();
    }

    private void Update()
    {
        CreateShape();
        UpdateMesh();
    }


    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        colors = new Color[(xSize + 1) * (zSize + 1)];
        float[,] noiseMap = Noise.GenerateNoiseMap(xSize + 1, zSize + 1, seed, scale, octaves, persistance, lacunarity);
        for (int i = 0, z = 0; z < zSize + 1; z++)
        {
            for(int x = 0; x < xSize + 1; x++)
            {
                float y = noiseMap[x, z]*10f;
                vertices[i] = new Vector3(x, y, z);
                //colors[i] = CalculateVertexColor(noiseMap[x, z]);
                i++; 
            }
        }

        triangles = new int[xSize*zSize*6];
        int vert = 0;
        int tris = 0;

        for(int z = 0; z < zSize; z++)
        {
            Color c;
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                c = CalculateVertexColor((vertices[vert + 0].y/10f + vertices[vert + xSize + 1].y/10f + vertices[vert+1].y/10f) / 3f);
                colors[vert + 0] = c;
                colors[vert + xSize + 1] = c;
                colors[vert + 1] = c;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;
                c = CalculateVertexColor((vertices[vert + 1].y/10f + vertices[vert + xSize + 1].y/10f + vertices[vert + 2].y/10f) / 3f);
                colors[vert + 1] = c;
                colors[vert + xSize + 1] = c;
                colors[vert + 2] = c;

                vert++;
                tris += 6;
            }
            vert++;
        }

       
       
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }


    private void OnValidate()
    {
        if(lacunarity < 1) { lacunarity = 1; }
        if(octaves < 0) { octaves = 0; }
    }

    Color CalculateVertexColor(float height)
    {
        int lowCount = (int)((1f - height) * 100f);
        int highCount = 100 - lowCount;
 
        Color[] colors = new Color[100];
        Color finalColor = new Color(0, 0, 0, 0);
        for (int i = 0; i < lowCount; i++)
        {
            finalColor += Color.green;
        }
        for (int i = 0; i < highCount; i++)
        {
            finalColor += Color.white;
        }
        finalColor /= 100;
        return finalColor;

    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public Color color;
}
