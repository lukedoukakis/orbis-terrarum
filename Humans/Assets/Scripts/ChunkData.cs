using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkData
{
 
    public Vector2 coord;
    public bool loaded;
    public int randomState;

    public GameObject chunk;
    public GameObject terrain;
    public GameObject water;
    public GameObject trees;

    public MeshFilter terrainMeshFilter;
    public MeshFilter waterMeshFilter;
    public MeshRenderer terrainMeshRenderer;
    public MeshRenderer waterMeshRenderer;
    public Mesh terrainMesh;
    public Mesh waterMesh;

    public float[,] TemperatureMap;
    public float[,] HumidityMap;
    public float[,] ElevationMap;
    public float[,] MountainMap;
    public int[,] BiomeMap;
    public float[,] FreshWaterMap;
    public float[,] WetnessMap;
    public float[,] HeightMap;
    public bool[,] TreeMap;

    public ChunkData(Vector2 _coord)
    {
        coord = _coord;
        loaded = false;
    }

    public void Init(GameObject chunkPrefab)
    {
        randomState = (int)(coord.x + coord.y * 10f);

        chunk = GameObject.Instantiate(chunkPrefab);
        terrain = chunk.transform.Find("Terrain").gameObject;
        water = chunk.transform.Find("Water").gameObject;
        chunk.transform.position = Vector3.zero;
        trees = new GameObject();
        trees.transform.SetParent(chunk.transform);

        terrainMeshRenderer = terrain.GetComponent<MeshRenderer>();
        waterMeshRenderer = water.GetComponent<MeshRenderer>();
        terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        waterMeshFilter = water.GetComponent<MeshFilter>();
        terrainMesh = new Mesh();
        waterMesh = new Mesh();
        terrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        waterMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        terrainMeshFilter.mesh = terrainMesh;
        waterMeshFilter.mesh = waterMesh;

        loaded = true;
    }

    public void Deload()
    {
        Component.Destroy(terrainMesh);
        Component.Destroy(waterMesh);
        GameObject.Destroy(chunk);
        GameObject.Destroy(trees);
        loaded = false;

    }

}
