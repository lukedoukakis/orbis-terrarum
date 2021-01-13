using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaGenerator : MonoBehaviour
{

    [SerializeField] GameObject waterUnit;

    public int xSize;
    public int zSize;

    public float waterHeight;


    // Start is called before the first frame update
    void Start()
    {
        PlaceWaterTiles();
        AddCollider();
        AdjustPosition();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlaceWaterTiles()
    {
        Vector3 placePos;
        for(int x = 0; x < zSize; x++)
        {
            for(int z = 0; z < zSize; z++)
            {
                placePos = new Vector3(x*5, waterHeight, z*5);
                GameObject w = GameObject.Instantiate(waterUnit, placePos, Quaternion.identity, transform);
            }
        }
        transform.Rotate(Vector3.up);
    }

    void AddCollider()
    {
        BoxCollider bc = gameObject.AddComponent<BoxCollider>();
        bc.size = new Vector3(xSize*5f, 4f, zSize*5f);
        bc.center = new Vector3(xSize*5f/2f, waterHeight - bc.size.y/2f, zSize*5f/2f);
        bc.isTrigger = true;
    }

    void AdjustPosition()
    {
        transform.position = new Vector3(xSize*5f / -2f, transform.position.y, zSize*5f / -2f);
    }



}
