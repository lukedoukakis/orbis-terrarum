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
        placeWaterTiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void placeWaterTiles()
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
        transform.Rotate(Vector3.right);
    }



}
