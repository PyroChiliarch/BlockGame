using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main role is an interface for other scripts to interact with the terrain
/// </summary>
public class TerrainController : MonoBehaviour
{

    /////////////////////////////////////////// Debug ////////////////////////////////////////////////////////////////////
    
    bool drawChunkSides = true;





    ///////////////////////////////////////// Settings //////////////////////////////////////////////////////////////////////
    
    readonly int chunkSize = 16;
    readonly int viewDistance = 3;







    ////////////////////////////////////////// Mesh Assets //////////////////////////////////////////////////////////
    
    public Material TerrainMaterial;
    public Material LiquidMaterial;
    VoxelLibrary voxelLibrary = new VoxelLibrary();







    //////////////////////////////////////////// Keep up to date /////////////////////////////////////////////////////////////
    
    Dictionary<WorldPos, ChunkController> chunkList = new Dictionary<WorldPos, ChunkController>();
    Queue<ChunkController> refreshMeshQueue = new Queue<ChunkController>();











    /////////////////////////////////////////// Static Variables //////////////////////////////////////////////////////////////
    // HACK Fixed Chunks loading, need to turn into a function
    //https://en.wikipedia.org/wiki/Gauss_circle_problem

    //List of chunks around player
    List<WorldPos> chunksToLoad;









    ///////////////////////////////////////////// Other ///////////////////////////////////////////////////////////////////////////
    
    // List of objects that spawn in chunks
    // TODO implement chunkLoading around objects
    List<GameObject> chunkLoadersList = new List<GameObject>();
    ProcedualEngine procedualEngine = new ProcedualEngine();









    ////////////////////////////////////////////// Structs ////////////////////////////////////////////////////////////////

    /// <summary>
    /// All positions in terrain are stored using this struct
    /// </summary>
    public struct WorldPos
    {

        //Values
        public int x, y, z;



        //Constructor
        public WorldPos(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }




        //Override the built in equals because its quicker
        public override bool Equals(object obj)
        {
            if (!(obj is WorldPos))
                return false;

            WorldPos pos = (WorldPos)obj;

            if (pos.x != x || pos.y != y || pos.z != z)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        //To remove warning message
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // Convert Vector3 to WorldPos
        public static explicit operator WorldPos(Vector3 v)
        {
            WorldPos pos = new WorldPos((int)v.x, (int)v.y, (int)v.z);
            return pos;
        }

        public override string ToString()
        {
            return x + ", " + y + ", " + z;
        }
    }

    
















































    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////// Initialization ////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////







    // Start is called before the first frame update
    void Start()
    {

        //Initialize Objects
        voxelLibrary.Initialize();
        procedualEngine.Initialize(voxelLibrary);


        chunksToLoad = new List<WorldPos>{
            new WorldPos(0, 0, 0),


            new WorldPos(chunkSize, 0, 0),
            new WorldPos(0, chunkSize, 0),
            new WorldPos(0, 0, chunkSize),

            new WorldPos(-chunkSize, 0, 0),
            new WorldPos(0, -chunkSize, 0),
            new WorldPos(0, 0, -chunkSize),


            new WorldPos(chunkSize, chunkSize, 0),
            new WorldPos(0, chunkSize, chunkSize),
            new WorldPos(chunkSize, 0, chunkSize),

            new WorldPos(-chunkSize, -chunkSize, 0),
            new WorldPos(0, -chunkSize, -chunkSize),
            new WorldPos(-chunkSize, 0, -chunkSize),


            new WorldPos(chunkSize, -chunkSize, 0),
            new WorldPos(0, chunkSize, -chunkSize),
            new WorldPos(-chunkSize, 0, chunkSize),

            new WorldPos(-chunkSize, chunkSize, 0),
            new WorldPos(0, -chunkSize, chunkSize),
            new WorldPos(chunkSize, 0, -chunkSize)



            };


        /*
        for (int i = -50; i < 50; i++)
        {
            Debug.Log(GetChunkPos(new WorldPos(i, 0, 0)));
        }
        */

        /*
        //Create Initial Chunks around center of map
        for (int x = viewDistance * -1; x < viewDistance; x++)
        {
            for (int y = viewDistance * -1; y < viewDistance; y++)
            {
                for (int z = viewDistance * -1; z < viewDistance; z++)
                {
                    CreateChunk(x * chunkSize, y * chunkSize, z * chunkSize);
                }
            }
        }
        */

    }



















    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////// Public Methods ////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////




    /// <summary>
    /// Return Voxel Type from a specific location
    /// <returns></returns>
    
    public VoxelType GetVoxel(int x, int y, int z)
    {
        return GetVoxel(new WorldPos(x, y ,z));
    }

    public VoxelType GetVoxel(WorldPos pos)
    {
        //get chunks position in gameworld (Which is also its key for the dictionary)
        WorldPos chunkPos = GetChunkPos(pos);

        ChunkController chunk;

        if (!chunkList.TryGetValue(new WorldPos(chunkPos.x, chunkPos.y, chunkPos.z), out chunk))
        {
            if (drawChunkSides)
            {
                return voxelLibrary.Lookup("Air");
            }
            return voxelLibrary.Lookup("Nothing");
        }
        else
        {

            return chunk.GetLocalVoxel(pos.x - chunkPos.x, pos.y - chunkPos.y, pos.z - chunkPos.z);
        }
    }




    /// <summary>
    /// Get chunk that contains input position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public WorldPos GetChunkPos(WorldPos pos)
    {
        WorldPos chunkPos = new WorldPos();

        chunkPos.x = (Mathf.FloorToInt(pos.x / (float)chunkSize)) * chunkSize;
        chunkPos.y = (Mathf.FloorToInt(pos.y / (float)chunkSize)) * chunkSize;
        chunkPos.z = (Mathf.FloorToInt(pos.z / (float)chunkSize)) * chunkSize;

        return chunkPos;
    }


    /// <summary>
    /// Draw chunks around the selected object
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns>True on success</returns>
    public bool RegisterChunkLoader (GameObject gameObject)
    {
        if (!chunkLoadersList.Contains(gameObject))
        {
            chunkLoadersList.Add(gameObject);
            return true;
        }
        return false;

    }


    /// <summary>
    /// Stop drawing chunks around the selected object
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public bool DeRegisterChunkLoader (GameObject gameObject)
    {
        if (chunkLoadersList.Remove(gameObject))
        {
            return true;
        }

        return false;
    }



    /// <summary>
    /// Tells the terrain controller that a chunks mesh needs to be recalculated
    /// </summary>
    /// <param name="chunk">Chunk to refresh</param>
    /// <returns>Returns true if chuck is added to queue</returns>
    public bool RefreshChunkMesh(ChunkController chunk)
    {

        if (!refreshMeshQueue.Contains(chunk))
        {
            refreshMeshQueue.Enqueue(chunk);
            return true;
        }

        return false;
    }





    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// Update Events ///////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////


    void Update()
    {

        


        //Refresh the mesh of any chunks that need it
        if (refreshMeshQueue.Count > 0)
        {
            ChunkController chunk = refreshMeshQueue.Dequeue();
            chunk.GenMesh();
            chunk.awaitingUpdate = false;
        }

        
        // Create chunks around every chunkLoader
        foreach (GameObject obj in chunkLoadersList)
        {
            WorldPos loaderPos = (WorldPos)obj.transform.position;
            


            for (int i = 0; i < chunksToLoad.Count; i++)
            {
                WorldPos chunkPos = GetChunkPos(new WorldPos(
                    chunksToLoad[i].x + (int)loaderPos.x,
                    chunksToLoad[i].y + (int)loaderPos.y,
                    chunksToLoad[i].z + (int)loaderPos.z));



                if (!chunkList.ContainsKey(chunkPos))
                {
                    CreateChunk(chunkPos);
                }
            }
        }
        

    }













    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////// Internal Methods ///////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////











    








    /// <summary>
    /// Spawns a chunk in the gameworld
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    ChunkController CreateChunk(WorldPos pos)
    {
        GameObject newChunk = new GameObject(pos.x + "," + pos.y + "," + pos.z);
        ChunkController newChunkController = newChunk.AddComponent<ChunkController>();

        
        newChunkController.Initialize(TerrainMaterial, LiquidMaterial, chunkSize, this, new Vector3(pos.x, pos.y, pos.z), pos, voxelLibrary, procedualEngine);

        //Add to quene to refresh, must be done atleast once
        RefreshChunkMesh(newChunkController);

        chunkList.Add(pos, newChunkController);
        return newChunkController;
    }










    /// <summary>
    /// Removes a chunk from the game world
    /// </summary>
    /// <returns>Success</returns>
    bool DespawnChunk (int x, int y, int z)
    {
        ChunkController targetChunk;

        if (chunkList.TryGetValue(new WorldPos(x, y, z), out targetChunk))
        {
            //Code if it exists
            chunkList.Remove(new WorldPos(x, y, z));
            GameObject.Destroy(targetChunk.gameObject);
            return true;
        }
        return false;

    }











    

    



}
