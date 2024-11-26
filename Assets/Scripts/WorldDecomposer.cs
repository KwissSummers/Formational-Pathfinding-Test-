using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldDecomposer : MonoBehaviour
{
    private int[,] worldData;
    private int rows;
    private int cols;
    private int nodeSize = 5;
    public bool isInitialized = false;

    public int[,] GetWorldData()
    {
        return worldData;
    }

    public int GetRows()
    {
        return rows;
    }

    public int GetCols()
    {
        return cols;
    }

    private void Start()
    {
        int terrainWidth = 200;
        int terrainLength = 200;
        int nodeSize = 5;

        rows = terrainWidth / nodeSize;
        cols = terrainLength / nodeSize;

        worldData = new int[rows, cols];
        DecomposeWorld();
        isInitialized = true;
    }

    private void DecomposeWorld()
    {
        // Add your world decomposition logic here
        float startX = 0;
        float startZ = 0;

        float nodeCenterOffset = nodeSize / 2f;


        for (int row = 0; row < rows; row++)
        {

            for (int col = 0; col < cols; col++)
            {

                float x = startX + nodeCenterOffset + (nodeSize * col);
                float z = startZ + nodeCenterOffset + (nodeSize * row);

                Vector3 startPos = new Vector3(x, 20f, z);



                // Does our raycast hit anything at this point in the map

                RaycastHit hit;

                // Bit shift the index of the layer (8) to get a bit mask
                int layerMask = 1 << 8;

                // This would cast rays only against colliders in layer 8.
                // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
                layerMask = ~layerMask;

                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(startPos, Vector3.down, out hit, Mathf.Infinity, layerMask))
                {

                    print("Hit something at row: " + row + " col: " + col);
                    Debug.DrawRay(startPos, Vector3.down * 20, Color.red, 50000);
                    worldData[row, col] = 1;

                }
                else
                {
                    Debug.DrawRay(startPos, Vector3.down * 20, Color.green, 50000);
                    worldData[row, col] = 0;
                }
            }
        }

    }
}


