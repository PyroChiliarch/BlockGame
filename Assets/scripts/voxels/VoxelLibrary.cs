using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelLibrary
{
    
    //Where all the voxel types are stored
    private Dictionary<string, VoxelType> voxels = new Dictionary<string, VoxelType>();

    //What area of the sprite map each texture is mapped to.
    //0 is bottom left, 1 is to the right of 0
    enum Textures { 
        //Special Values
        ERROR = 0,

        //Solid Values
        Gravel = 1, 
        RoughGravel = 2, 
        Mud = 3,
        Grass = 4,

        //Transparent Values
        Water = 5,
        FlameGas = 1}



    enum Opaque
    {
        FALSE = 0,
        TRUE = 1
    }

    /// <summary>
    /// Setup the default blocks in game
    /// </summary>
    public void Initialize ()
    {
        voxels.Add(
            "ERROR",
            new VoxelType(
                "ERROR",
                (int)Textures.ERROR));

        voxels.Add(
            "Nothing",
            new VoxelType(
                "Nothing",
                true,
                false,
                new int[] { (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR }));

        voxels.Add(
            "Air",
            new VoxelType(
                "Air",
                false,
                false,
                new int[] { (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR, (int)Textures.ERROR }));

        voxels.Add(
            "Mud",
            new VoxelType(
                "Mud", 
                (int)Textures.Mud));

        voxels.Add(
            "Gravel",
            new VoxelType(
                "Gravel", 
                (int)Textures.Gravel));

        voxels.Add(
            "RoughGravel",
            new VoxelType(
                "RoughGravel", 
                (int)Textures.RoughGravel));

        voxels.Add(
            "Grass",
            new VoxelType(
                "Grass",
                true,
                true,
                new int[] { (int)Textures.Grass, (int)Textures.Gravel, (int)Textures.Gravel, (int)Textures.Gravel, (int)Textures.Gravel, (int)Textures.Gravel }));

        voxels.Add(
            "Water",
             new VoxelType(
                "Water",
                false,
                true,
                new int[] { (int)Textures.Water, (int)Textures.Water, (int)Textures.Water, (int)Textures.Water, (int)Textures.Water, (int)Textures.Water }));


        voxels.Add(
            "FlameGas",
             new VoxelType(
                "FlameGas",
                false,
                true,
                new int[] { (int)Textures.FlameGas, (int)Textures.FlameGas, (int)Textures.FlameGas, (int)Textures.FlameGas, (int)Textures.FlameGas, (int)Textures.FlameGas }));


    }

    /// <summary>
    /// Returns a block from the Library
    /// </summary>
    /// <param name="lookupName">Name of block to lookup</param>
    /// <returns></returns>
    public VoxelType Lookup (string lookupName)
    {
        VoxelType voxelType;

        if (!voxels.TryGetValue(lookupName, out voxelType))
        {
            Debug.LogError("Could not find " + lookupName + " in voxel Library");
            if (!voxels.TryGetValue("ERROR", out voxelType))
                throw new System.Exception("Failed to substitute error block on bad library lookup");
        }

        return voxelType;

    }


}
