using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    public int terrainSize;
    public TreeGenerator Forest;
    private float[,] heightMap;



    private void Start()
    {

        // Validation check to ensure terrainSize is not smaller than forestSize, as this will create an indexOutOfBounds exception.
        if (terrainSize < Forest.forestSize)
            terrainSize = Forest.forestSize;

        heightMap = new float[terrainSize, terrainSize];

      
      
        Forest.Generate(heightMap);

    }


    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            Forest.Generate(heightMap);

        }
    }




}