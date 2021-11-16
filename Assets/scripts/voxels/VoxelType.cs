using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary>
///Holds data on a specific voxel type
///</summary>
public class VoxelType
{



    public string Name;
    public bool isSolid;
    public bool isVisible;
    //Block sides
    public enum Side { TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };

    ///<summary>
    ///0:Top, 1:Bottom, 2:Left, 3:Right, 4:Front, 5:Back
    ///</summary>
    Vector2[][] UVs;
    
    int spriteMapWidth = 8;
    int spriteMapHeight = 8;










    ///<summary>
    ///Create a block with a single sprite
    ///</summary>
    public VoxelType(string _name, int spriteID)
    {
        Name = _name;
        isSolid = true;
        isVisible = true;
        UVs = new Vector2[][]
        {
            CalculateUVs(spriteMapWidth, spriteMapHeight, spriteID)
        };
        
    }

    

    ///<summary>
    ///Create a block with a different sprite on each side
    ///</summary>
    public VoxelType(string _name, bool _isOpaque, bool _isVisible, int[] spriteIDs)
    {
        if (spriteIDs.Length != 6) {Debug.LogError(_name + " Sprite ID array length is incorrect");}

        Name = _name;

        isSolid = _isOpaque;
        isVisible = _isVisible;

        UVs = new Vector2 [6][];
        for (int i = 0; i < spriteIDs.Length; i++)
        {
            UVs[i] = CalculateUVs(spriteMapWidth, spriteMapHeight, spriteIDs[i]);
        }

    }











    ///<summary>
    ///Get the UVs for a specific side
    ///</summary>
    public Vector2[] GetUVSide (Side side)
    {
        //Will default to the first side if there is only one side used
        if ((int)side < UVs.Length)
        {
            return UVs[(int)side];
        }
        return UVs[0];
    }





    ///<summary>
    ///Given the height and width of a sprite sheet, calculate the UV values
    ///for the selected sprite.
    ///Only for grids
    ///</summary>
    static Vector2[] CalculateUVs (int width, int height, int pos)
    {
        //calculate the dimensions of each sprite in UV values
        float widthStep = 1f / width;
        float heightStep = 1f / height;

        //The x and y coordinates of the sprite
        int x = pos % width;
        int y = Mathf.FloorToInt(pos / height);

        //Calculate the 4 possible values that each UV value could be
        float xMin = x * widthStep;
        float yMin = y * heightStep;
        float xMax = (x + 1) * widthStep;
        float yMax = (y + 1) * heightStep;

        //UV starts at the top right and goes anti clockwise
        return new Vector2[]
        {
            new Vector2(xMax, yMax),
            new Vector2(xMin, yMax),
            new Vector2(xMin, yMin),
            new Vector2(xMax, yMin)
        };
    }
}
