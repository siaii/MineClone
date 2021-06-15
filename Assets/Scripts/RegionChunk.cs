using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class RegionChunk : MonoBehaviour
{
    [SerializeField] private GameObject renderChunkPrefab;

    
    public const int chunkSizeX = 16;
    public const int chunkSizeZ = 16;
    public const int chunkSizeY = 128;
    
    public int minBlockHeightGenerated = 48;

    public BlockTypes[][][] BlocksData
    {
        get;
        set;
    } = new BlockTypes[chunkSizeX + 2][][];
    private RenderChunk[][][] chunkData;

    private Vector2Int chunkPos;

    private float noiseScale = 1f;
    private float heightScale = 0.6f;

    private TerrainGen _terrainGen;
    private TexturePacker _texturePacker;
    
    private static readonly Dictionary<Sides, Vector3Int> sideVector = new Dictionary<Sides, Vector3Int>()
    {
        {Sides.UP, Vector3Int.up},
        {Sides.DOWN, Vector3Int.down},
        {Sides.FRONT, Vector3Int.back},
        {Sides.BACK, Vector3Int.forward},
        {Sides.LEFT, Vector3Int.left},
        {Sides.RIGHT, Vector3Int.right}
    };

    private static readonly Dictionary<BlockTypes, Block> blockTypesProperties = new Dictionary<BlockTypes, Block>()
    {
        {BlockTypes.GRASS, new Grass()},
        {BlockTypes.AIR, new Air()},
        {BlockTypes.DIRT, new Dirt()},
        {BlockTypes.STONE, new Stone()}
    };


    private void Awake()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
        _terrainGen = FindObjectOfType<TerrainGen>();
        for (int i = 0; i < chunkSizeX+2; i++)
        {
            BlocksData[i] = new BlockTypes[chunkSizeY][];
            for (int j = 0; j < chunkSizeY; j++)
            {
                BlocksData[i][j] = new BlockTypes[chunkSizeZ+2];
            }
        }
        chunkData = new RenderChunk[chunkSizeX / RenderChunk.xSize][][];
        for (int i = 0; i < chunkSizeX / RenderChunk.xSize; i++)
        {
            chunkData[i] = new RenderChunk[chunkSizeY / RenderChunk.ySize][];
            for (int j = 0; j < chunkSizeY / RenderChunk.ySize; j++)
            {
                chunkData[i][j] = new RenderChunk[chunkSizeZ / RenderChunk.zSize];
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Make multithreaded
    public void GenerateRenderChunks()
    {
        for (int x = 0; x < chunkSizeX / RenderChunk.xSize; x++)
        {
            for (int z = 0; z < chunkSizeZ / RenderChunk.zSize; z++)
            {
                for (int y = 0; y < chunkSizeY / RenderChunk.ySize; y++)
                {
                    if (chunkData[x][y][z] == null)
                    {
                        GameObject renderChunkGO = Instantiate(renderChunkPrefab, this.transform);
                        RenderChunk renderChunkScript = renderChunkGO.GetComponent<RenderChunk>();
                        renderChunkGO.transform.localPosition = new Vector3(x * RenderChunk.xSize, y * RenderChunk.ySize,
                            z * RenderChunk.zSize);
                        chunkData[x][y][z] = renderChunkScript;
                    }
                    List<Vector3> renderChunkVertices = new List<Vector3>();
                    List<int> renderChunkTris = new List<int>();
                    List<Vector2> renderChunkUVs = new List<Vector2>();
                        
                    CalculateDrawnMesh(x, y, z, renderChunkVertices, renderChunkTris, renderChunkUVs);
                }
            }
        }
    }
    
    void CalculateDrawnMesh(int rChunkX,int rChunkY, int rChunkZ, List<Vector3> vertices, List<int> tris, List<Vector2> uvs)
    {
        NativeList<Vector3> verts = new NativeList<Vector3>(Allocator.Persistent);
        NativeList<int> triss = new NativeList<int>(Allocator.Persistent);
        NativeList<Vector2> uvss = new NativeList<Vector2>(Allocator.Persistent);
        NativeArray<BlockTypes> flatBlocksData = new NativeArray<BlockTypes>((BlocksData.SelectMany(a => a).ToArray()).SelectMany(b=>b).ToArray(), Allocator.Persistent);
        NativeArray<Rect> atlasRects = new NativeArray<Rect>(_texturePacker.blockTextureRects, Allocator.Persistent);
        NativeHashMap<int, int> textureRectIndex = new NativeHashMap<int, int>(_texturePacker.textureDictIndex.Count, Allocator.Persistent);

        foreach (var pair in _texturePacker.textureDictIndex)
        {
            textureRectIndex.Add((int)pair.Key, pair.Value);
        }

        var job = new CalculateMeshJob()
        {
            verts = verts,
            tris = triss,
            uvs = uvss,
            BlocksData = flatBlocksData,
            atlasRect = atlasRects,
            textureRectIndex = textureRectIndex,
            renderChunkCoords = new Vector3Int(rChunkX, rChunkY, rChunkZ)
        };

        var jobHandle = job.Schedule();

        jobHandle.Complete();
        chunkData[rChunkX][rChunkY][rChunkZ].BuildMesh(verts.ToArray(), triss.ToArray(), uvss.ToArray());

        
        //Move some of these upwards out of function
        verts.Dispose();
        triss.Dispose();
        uvss.Dispose();
        atlasRects.Dispose();
        flatBlocksData.Dispose();
        textureRectIndex.Dispose();
    }

    Vector2[] GetBlockSideUVs(BlockTypes type, Sides side)
    {
        var localUV = blockTypesProperties[type].GetSideUVs(side);
        Vector2[] res = new Vector2[localUV.Length];
        var textureRect = _texturePacker.blockTextureRects[_texturePacker.textureDictIndex[type]];

        for(int i=0; i<localUV.Length; i++)
        {
            res[i] = new Vector2(textureRect.x + localUV[i].x * textureRect.width,
                textureRect.y + localUV[i].y * textureRect.height);
        }

        return res;
    }

    //Maybe convert this to job?
    //Convert blocksdata into nativearray and give readonly access to this
    public struct CalculateMeshJob : IJob
    {
        public NativeList<Vector3> verts;
        public NativeList<int> tris;
        public NativeList<Vector2> uvs;

        [Unity.Collections.ReadOnly]
        public NativeArray<BlockTypes> BlocksData; 
        
        [Unity.Collections.ReadOnly]
        public NativeArray<Rect> atlasRect;
        
        [Unity.Collections.ReadOnly]
        public NativeHashMap<int, int> textureRectIndex;

        public Vector3Int renderChunkCoords;
        public void Execute()
        {
            int startX = renderChunkCoords.x * RenderChunk.xSize + 1;
            int startY = renderChunkCoords.y * RenderChunk.ySize;
            int startZ = renderChunkCoords.z * RenderChunk.zSize + 1;
        
            for (int x = startX; x < startX + RenderChunk.xSize; x++)
            {
                for (int y = startY; y < startY + RenderChunk.ySize; y++)
                {
                    for (int z = startZ; z < startZ + RenderChunk.zSize; z++)
                    {
                        var sidesToDraw = CheckBorderingTransparent(x, y, z);
                        foreach (var side in sidesToDraw)
                        {
                            int localX = x - startX;
                            int localY = y - startY;
                            int localZ = z - startZ;
                            int oldLength = verts.Length;
                            NativeArray<Vector3> vertRes = new NativeArray<Vector3>(blockTypesProperties[BlocksData[ConvertCoordToIdx(x, y, z)]]
                                .GetSideVertices(side, new Vector3(localX, localY, localZ)), Allocator.Temp);
                            verts.AddRange(vertRes);

                            NativeArray<Vector2> uvRes =
                                new NativeArray<Vector2>(GetBlockSideUVs(BlocksData[ConvertCoordToIdx(x, y, z)], side),
                                    Allocator.Temp);
                            uvs.AddRange(uvRes);
                            var blockTris = blockTypesProperties[BlocksData[ConvertCoordToIdx(x,y,z)]].GetSideTriangles();

                            foreach (var offset in blockTris)
                            {
                                tris.Add(oldLength+offset);
                            }

                            vertRes.Dispose();
                            uvRes.Dispose();
                        }
                    }
                }
            }
            
            
        }
        
        Vector2[] GetBlockSideUVs(BlockTypes type, Sides side)
        {
            var localUV = blockTypesProperties[type].GetSideUVs(side);
            Vector2[] res = new Vector2[localUV.Length];
            var textureRect = atlasRect[textureRectIndex[(int)type]];

            for(int i=0; i<localUV.Length; i++)
            {
                res[i] = new Vector2(textureRect.x + localUV[i].x * textureRect.width,
                    textureRect.y + localUV[i].y * textureRect.height);
            }

            return res;
        }
        
        List<Sides> CheckBorderingTransparent(int xCoord, int yCoord, int zCoord)
        {
            List<Sides> sidesToDraw = new List<Sides>();
            foreach (var pair in sideVector)
            {
                Vector3Int dirVector = pair.Value;
                int checkBlockX = xCoord + dirVector.x;
                int checkBlockY = yCoord + dirVector.y;
                int checkBlockZ = zCoord + dirVector.z;
            
                if(checkBlockY<0 || checkBlockY>=chunkSizeY || CheckBlockIsTransparent(checkBlockX, checkBlockY, checkBlockZ))
                    sidesToDraw.Add(pair.Key);
            
            }

            return sidesToDraw;
        }
        
        public bool CheckBlockIsTransparent(int x, int y, int z)
        {
            if (y < 0 || y >= chunkSizeY)
                return true;
            try
            {
                return blockTypesProperties[BlocksData[ConvertCoordToIdx(x, y, z)]].isTransparent;
            }
            catch
            {
                print(x+","+y+","+z);
                return false;
            }
        }

        private int ConvertCoordToIdx(int x, int y, int z)
        {
            return (x * chunkSizeY * (chunkSizeZ + 2)) + y * (chunkSizeZ+2) + z;
        }
    }
    List<Sides> CheckBorderingTransparent(int xCoord, int yCoord, int zCoord)
    {
        List<Sides> sidesToDraw = new List<Sides>();
        foreach (var pair in sideVector)
        {
            Vector3Int dirVector = pair.Value;
            int checkBlockX = xCoord + dirVector.x;
            int checkBlockY = yCoord + dirVector.y;
            int checkBlockZ = zCoord + dirVector.z;
            
            if(checkBlockY<0 || checkBlockY>=chunkSizeY || CheckBlockIsTransparent(checkBlockX, checkBlockY, checkBlockZ))
                sidesToDraw.Add(pair.Key);
            
        }

        return sidesToDraw;
    }

    public bool CheckBlockIsTransparent(int x, int y, int z)
    {
        if (y < 0 || y >= chunkSizeY)
            return true;
        return blockTypesProperties[BlocksData[x][y][z]].isTransparent;
    }

    public void SetChunkPos(int x, int z)
    {
        chunkPos = new Vector2Int(x, z);
    }

    public void SetNoiseScale(float scale)
    {
        noiseScale = scale;
    }

    public void SetHeightScale(float scale)
    {
        heightScale = scale;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

}
