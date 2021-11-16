using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedualEngine
{
    bool isInitialized = false;
    VoxelLibrary voxelLibrary;

    
    public void Initialize (VoxelLibrary lib)
    {
        voxelLibrary = lib;
        isInitialized = true;
    }

    public VoxelType GetVoxel(int worldX, int worldY, int worldZ)
    {
        
        if (isInitialized == false)
            throw new System.Exception ("ProcedualEngine not initialized");

        return PerlinHeightNoise(worldX, worldY, worldZ, 30f, 0.08f, 0.08f);
    }


    VoxelType RandomNoise (int x, int y, int z) 
    {
        float ran = Random.Range(0f, 1f);
        if (ran > 0.8f)
        {
            return voxelLibrary.Lookup("Grass");
        }
        return voxelLibrary.Lookup("Air");
    }


    VoxelType CheckerBoardNoise (int x, int y, int z)
    {
        if (x % 2 == 0)
        {
            if (y % 2 == 0)
            {
                if (z % 2 == 0)
                {
                    return voxelLibrary.Lookup("Grass");
                }
                return voxelLibrary.Lookup("Air");
            }
            return voxelLibrary.Lookup("Grass");
        } else
        {
            if (y % 2 == 1)
            {
                if (z % 2 == 1)
                {
                    return voxelLibrary.Lookup("Air");
                }
                return voxelLibrary.Lookup("Grass");
            }
            return voxelLibrary.Lookup("Air");
        }
    }

    VoxelType PerlinHeightNoise (int x, int y, int z, float amplitude, float scaleX, float scaleZ)
    {
        
        float perlin = Mathf.PerlinNoise(
            (x * scaleX) + 0.5f,
            (z * scaleZ) + 0.5f);

        //Amplify and centre the number around the original value
        float height = (perlin * amplitude) - (amplitude/2);

        if (y < height)
        {
            return voxelLibrary.Lookup("Mud");
        } else if (y < height + 1)
        {
            return voxelLibrary.Lookup("Grass");
        } else if (y < 1)
        {
            return voxelLibrary.Lookup("Water");
        } else if (y < 2)
        {
            return voxelLibrary.Lookup("Air");
        } else
        {
            return voxelLibrary.Lookup("Air");
        }


        

    }



}
