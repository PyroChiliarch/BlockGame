using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChunkController : MonoBehaviour
{
    ///////////// Debug //////////////
    public bool UpdateMesh = false;

    bool isInitialized = false;

    //Vars set on Init
    TerrainController terrainController;
    TerrainController.WorldPos chunkPos;
    VoxelLibrary voxelLibrary;
    ProcedualEngine procedualEngine;
    Material solidMaterial;
    Material liquidMaterial;
    int chunkSize;

    //Generated on Init
    VoxelType[,,] voxelData;

    //Is in the quene to update this mesh
    //Terrain controller takes car of this
    public bool awaitingUpdate = false;

    public enum VoxelSide { TOP, BOTTOM, LEFT, RIGHT, FRONT, BACK };


    /////////////////////// Meshes ///////////////////

    //Object and components for drawing the Solid mesh
    GameObject meshObjectSolid;
    MeshFilter meshFilterSolid;
    MeshRenderer meshRendererSolid;

    //Collider meshes
    MeshCollider meshColliderSolid;
    

    //Object and components for drawing the transparent mesh
    GameObject meshObjectLiquids;
    MeshFilter meshFilterLiquids;
    MeshRenderer meshRendererLiquids;














    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////// Initialization ////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////


    // Start is called before the first frame update
    void Start()
    {
        if (!isInitialized) throw new System.Exception(this.gameObject.name + " chunk was not Initialized");

    }



    /// <summary>
    /// Attempt at a constructor for a monobehavior object
    /// </summary>
    public void Initialize(Material _solidMaterial, Material _liquidMaterial, int _chunkSize, TerrainController _terrainController, Vector3 _pos, TerrainController.WorldPos _terrainPos, VoxelLibrary _voxelLibrary, ProcedualEngine _procedualEngine)
    {
        //Take initial Variables from calling object
        solidMaterial = _solidMaterial;
        liquidMaterial = _liquidMaterial;
        chunkSize = _chunkSize;
        terrainController = _terrainController;
        this.transform.position = _pos;
        chunkPos = _terrainPos;
        voxelLibrary = _voxelLibrary;
        procedualEngine = _procedualEngine;

        //Set own variables
        isInitialized = true;



        //////////////////Mesh Setup////////////////////////
        

        //Add components for drawing solid meshes
        meshObjectSolid = this.gameObject;
        meshFilterSolid = this.gameObject.AddComponent<MeshFilter>();
        meshRendererSolid = this.gameObject.AddComponent<MeshRenderer>();

        meshColliderSolid = this.gameObject.AddComponent<MeshCollider>();
        


        //The liquids mesh is on a separate child object
        meshObjectLiquids = new GameObject(name + "L");
        meshObjectLiquids.transform.position = meshObjectSolid.transform.position;
        meshFilterLiquids = meshObjectLiquids.AddComponent<MeshFilter>();
        meshRendererLiquids = meshObjectLiquids.AddComponent<MeshRenderer>();


        //Generate Initial Data
        GenVoxelsData();
    }






    private void Update()
    {
        if (UpdateMesh)
        {
            UpdateMesh = false;
            terrainController.RefreshChunkMesh(this);
        }
    }







    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////// Public Methods ////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////



    public VoxelType GetLocalVoxel(int x, int y, int z)
    {
        return voxelData[x, y, z];
    }

    public VoxelType GetLocalVoxel(TerrainController.WorldPos pos)
    {
        return voxelData[pos.x, pos.y, pos.z];
    }




    public (int, int, int) LocalToWorldPosInt (int x, int y, int z)
    {
        return (chunkPos.x + x, chunkPos.y + y, chunkPos.z + z);
    }










    /// <summary>
    /// Generates fresh terrain data
    /// </summary>
    public void GenVoxelsData()
    {
        voxelData = new VoxelType[chunkSize, chunkSize, chunkSize];

        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);

                    int worldX = chunkPos.x + x;
                    int worldY = chunkPos.y + y;
                    int worldZ = chunkPos.z + z;

                    voxelData[x, y, z] = procedualEngine.GetVoxel(worldX, worldY, worldZ);

                }
            }
        }
    }






    


    //The following data structures are for storing meshs in the following method
    //Declared here since its only to be used here
    enum MeshType
    {
        Solid,
        Liquid
    }
    struct MeshData
    {
        public List<Vector3> vertices;
        public List<Vector3> normals;
        public List<Vector2> uvs;
        public List<int> triangles;
    }


    /// <summary>
    /// Generate the Mesh according to Voxel Data
    /// </summary>
    public void GenMesh()
    {

        //Create new mesh data variables
        //2 Meshes need to be made, one transparent and the other Solid
        MeshData[] meshData = new MeshData[2];
        for (int i = 0; i < meshData.Length; i++)
        {
            meshData[i].vertices = new List<Vector3>();
            meshData[i].normals = new List<Vector3>();
            meshData[i].uvs = new List<Vector2>();
            meshData[i].triangles = new List<int>();
        }







        for (int z = 0; z < chunkSize; z++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    
                    //Skip if Block is not visble
                    if (!terrainController.GetVoxel(x + chunkPos.x, y + chunkPos.y, z + chunkPos.z).isVisible)
                       continue;

                    VoxelType curVoxel = voxelData[x, y, z];

                    MeshType meshType;
                    if (curVoxel.isSolid)
                    {
                        meshType = MeshType.Solid;
                    } else
                    {
                        meshType = MeshType.Liquid;
                    }

                    


                    //Offset for when drawing triangles
                    int vertOffset;

                    //Calculate possible Vertices
                    float top = y + 0.5f;
                    float bottom = y - 0.5f;
                    float left = x - 0.5f;
                    float right = x + 0.5f;
                    float front = z + 0.5f;
                    float back = z - 0.5f;

                    //World position is required for checking adjacent voxels
                    var worldPos = LocalToWorldPosInt(x, y ,z);
                    int gx = worldPos.Item1;
                    int gy = worldPos.Item2;
                    int gz = worldPos.Item3;




                    //Need to check adjacent voxels before drawing face
                    VoxelType otherVoxel = terrainController.GetVoxel(gx, gy - 1, gz);
                    if (!(otherVoxel.isSolid || curVoxel.Name == otherVoxel.Name))
                    {
                        //Bottom Face
                        vertOffset = meshData[(int)meshType].vertices.Count;
                        meshData[(int)meshType].vertices.Add(new Vector3(left, bottom, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, bottom, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, bottom, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, bottom, back));

                        meshData[(int)meshType].normals.Add(Vector3.down);
                        meshData[(int)meshType].normals.Add(Vector3.down);
                        meshData[(int)meshType].normals.Add(Vector3.down);
                        meshData[(int)meshType].normals.Add(Vector3.down);

                        meshData[(int)meshType].uvs.AddRange(curVoxel.GetUVSide(VoxelType.Side.BOTTOM));
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                        meshData[(int)meshType].triangles.Add(0 + vertOffset);
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(2 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                    }


                    otherVoxel = terrainController.GetVoxel(gx, gy + 1, gz);
                    if (!(otherVoxel.isSolid || curVoxel.Name == otherVoxel.Name))
                    {
                        //Top Face
                        vertOffset = meshData[(int)meshType].vertices.Count;
                        meshData[(int)meshType].vertices.Add(new Vector3(right, top, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, top, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, top, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, top, back));
                        meshData[(int)meshType].normals.Add(Vector3.up);
                        meshData[(int)meshType].normals.Add(Vector3.up);
                        meshData[(int)meshType].normals.Add(Vector3.up);
                        meshData[(int)meshType].normals.Add(Vector3.up);

                        meshData[(int)meshType].uvs.AddRange(curVoxel.GetUVSide(VoxelType.Side.TOP));
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                        meshData[(int)meshType].triangles.Add(0 + vertOffset);
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(2 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                    }

                    otherVoxel = terrainController.GetVoxel(gx - 1, gy, gz);
                    if (!(otherVoxel.isSolid || curVoxel.Name == otherVoxel.Name))
                    {
                        //Left Face
                        vertOffset = meshData[(int)meshType].vertices.Count;
                        meshData[(int)meshType].vertices.Add(new Vector3(left, top, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, top, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, bottom, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, bottom, back));
                        meshData[(int)meshType].normals.Add(Vector3.left);
                        meshData[(int)meshType].normals.Add(Vector3.left);
                        meshData[(int)meshType].normals.Add(Vector3.left);
                        meshData[(int)meshType].normals.Add(Vector3.left);


                        meshData[(int)meshType].uvs.AddRange(curVoxel.GetUVSide(VoxelType.Side.LEFT));
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                        meshData[(int)meshType].triangles.Add(0 + vertOffset);
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(2 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                    }


                    otherVoxel = terrainController.GetVoxel(gx + 1, gy, gz);
                    if (!(otherVoxel.isSolid || curVoxel.Name == otherVoxel.Name))
                    {
                        //Right Face
                        vertOffset = meshData[(int)meshType].vertices.Count;
                        meshData[(int)meshType].vertices.Add(new Vector3(right, top, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, top, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, bottom, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, bottom, front));
                        meshData[(int)meshType].normals.Add(Vector3.right);
                        meshData[(int)meshType].normals.Add(Vector3.right);
                        meshData[(int)meshType].normals.Add(Vector3.right);
                        meshData[(int)meshType].normals.Add(Vector3.right);

                        meshData[(int)meshType].uvs.AddRange(curVoxel.GetUVSide(VoxelType.Side.RIGHT));
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                        meshData[(int)meshType].triangles.Add(0 + vertOffset);
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(2 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                    }

                    otherVoxel = terrainController.GetVoxel(gx, gy, gz + 1);
                    if (!(otherVoxel.isSolid || curVoxel.Name == otherVoxel.Name))
                    {
                        //Front Face
                        vertOffset = meshData[(int)meshType].vertices.Count;
                        meshData[(int)meshType].vertices.Add(new Vector3(left, top, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, top, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, bottom, front));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, bottom, front));
                        meshData[(int)meshType].normals.Add(Vector3.forward);
                        meshData[(int)meshType].normals.Add(Vector3.forward);
                        meshData[(int)meshType].normals.Add(Vector3.forward);
                        meshData[(int)meshType].normals.Add(Vector3.forward);

                        meshData[(int)meshType].uvs.AddRange(curVoxel.GetUVSide(VoxelType.Side.FRONT));
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                        meshData[(int)meshType].triangles.Add(0 + vertOffset);
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(2 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                    }


                    otherVoxel = terrainController.GetVoxel(gx, gy, gz - 1);
                    if (!(otherVoxel.isSolid || curVoxel.Name == otherVoxel.Name))
                    {
                        //Back Face
                        vertOffset = meshData[(int)meshType].vertices.Count;
                        meshData[(int)meshType].vertices.Add(new Vector3(right, top, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, top, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(left, bottom, back));
                        meshData[(int)meshType].vertices.Add(new Vector3(right, bottom, back));

                        meshData[(int)meshType].normals.Add(Vector3.back);
                        meshData[(int)meshType].normals.Add(Vector3.back);
                        meshData[(int)meshType].normals.Add(Vector3.back);
                        meshData[(int)meshType].normals.Add(Vector3.back);

                        meshData[(int)meshType].uvs.AddRange(curVoxel.GetUVSide(VoxelType.Side.BACK));
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                        meshData[(int)meshType].triangles.Add(0 + vertOffset);
                        meshData[(int)meshType].triangles.Add(3 + vertOffset);
                        meshData[(int)meshType].triangles.Add(2 + vertOffset);
                        meshData[(int)meshType].triangles.Add(1 + vertOffset);
                    }
                    
                }
            }

        }

        //////////////////////////////////////////////////////////////////////////////
        ////////////////////////////Apply newly created meshes////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////
        ///Apply the solid visual mesh
        //Move all the newly generated data into the solid mesh
        Mesh newSolidMesh = new Mesh();
        newSolidMesh.vertices = meshData[(int)MeshType.Solid].vertices.ToArray();
        newSolidMesh.normals = meshData[(int)MeshType.Solid].normals.ToArray();
        newSolidMesh.uv = meshData[(int)MeshType.Solid].uvs.ToArray();
        newSolidMesh.triangles = meshData[(int)MeshType.Solid].triangles.ToArray();

        //Add the mesh
        meshFilterSolid.mesh = newSolidMesh;
        meshRendererSolid.material = solidMaterial;

        ////////////////////////////////
        ///Apply the solid Collider mesh
        meshColliderSolid.sharedMesh = newSolidMesh;




        ////////////////////////////////
        ///Apply the Liquid mesh
        //Move all the newly generated data into the liquid mesh
        Mesh newLiquidMesh = new Mesh();
        newLiquidMesh.vertices = meshData[(int)MeshType.Liquid].vertices.ToArray();
        newLiquidMesh.normals = meshData[(int)MeshType.Liquid].normals.ToArray();
        newLiquidMesh.uv = meshData[(int)MeshType.Liquid].uvs.ToArray();
        newLiquidMesh.triangles = meshData[(int)MeshType.Liquid].triangles.ToArray();

        //Add the mesh
        meshFilterLiquids.mesh = newLiquidMesh;
        meshRendererLiquids.material = liquidMaterial;




        

    }

}

