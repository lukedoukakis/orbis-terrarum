using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkData
{
 
    public Vector2 coord;
    public bool loaded;

    public GameObject chunk;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Mesh mesh;

    public ChunkData(Vector2 _coord)
    {
        coord = _coord;
        loaded = false;
    }

    public void init(GameObject obj)
    {
        chunk = GameObject.Instantiate(obj);
        meshRenderer = chunk.GetComponent<MeshRenderer>();
        meshFilter = chunk.GetComponent<MeshFilter>();
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;

        loaded = true;
    }

    public void Deload()
    {
        mesh.Clear();
        GameObject g = chunk;
        chunk = null;
        GameObject.Destroy(chunk);
        loaded = false;
    }

}
