using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using Random = UnityEngine.Random;

public class RegionChunk : MonoBehaviour
{
    [SerializeField] private GameObject renderChunkPrefab;

    
    public const int chunkSizeX = 16;
    public const int chunkSizeZ = 16;
    public const int chunkSizeY = 64;
    
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
    
    private readonly Dictionary<Sides, Vector3Int> sideVector = new Dictionary<Sides, Vector3Int>()
    {
        {Sides.UP, Vector3Int.up},
        {Sides.DOWN, Vector3Int.down},
        {Sides.FRONT, Vector3Int.back},
        {Sides.BACK, Vector3Int.forward},
        {Sides.LEFT, Vector3Int.left},
        {Sides.RIGHT, Vector3Int.right}
    };

    public static readonly Dictionary<BlockTypes, Block> blockTypesProperties = new Dictionary<BlockTypes, Block>()
    {
        {BlockTypes.GRASS, new Grass()},
        {BlockTypes.AIR, new Air()},
        {BlockTypes.DIRT, new Dirt()},
        {BlockTypes.STONE, new Stone()},
        {BlockTypes.WOOD, new Wood()},
        {BlockTypes.LEAF, new Leaf()}
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

    public IEnumerator GenerateRenderChunks()
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

                    StartCoroutine(CalculateDrawnMesh(x, y, z));
                    yield return null;
                }
            }
        }
    }
    
    public IEnumerator CalculateDrawnMesh(int rChunkX,int rChunkY, int rChunkZ)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        int startX = rChunkX * RenderChunk.xSize + 1;
        int startY = rChunkY * RenderChunk.ySize;
        int startZ = rChunkZ * RenderChunk.zSize + 1;
        
        for (int x = startX; x < startX + RenderChunk.xSize; x++)
        {
            for (int y = startY; y < startY + RenderChunk.ySize; y++)
            {
                for (int z = startZ; z < startZ + RenderChunk.zSize; z++)
                {
                    foreach (var pair in sideVector)
                    {
                        Vector3Int checkBlock = new Vector3Int(x, y, z) + pair.Value;

                        if (checkBlock.y < 0 || checkBlock.y >= chunkSizeY ||
                            CheckBlockIsTransparent(checkBlock))
                        {
                            int localX = x - startX;
                            int localY = y - startY;
                            int localZ = z - startZ;
                            int oldLength = vertices.Count;
                            vertices.AddRange(blockTypesProperties[BlocksData[x][y][z]].GetSideVertices(pair.Key, new Vector3(localX,localY,localZ)));
                            
                            uvs.AddRange(GetBlockSideUVs(BlocksData[x][y][z], pair.Key));
                            var blockTris = blockTypesProperties[BlocksData[x][y][z]].GetSideTriangles();
                            
                            foreach (var offset in blockTris)
                            {
                                tris.Add(oldLength+offset);
                            }
                        }
                    }
                }
            }
            yield return null;
        }
        
        chunkData[rChunkX][rChunkY][rChunkZ].BuildMesh(vertices.ToArray(), tris.ToArray(), uvs.ToArray());
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

    public bool CheckBlockIsTransparent(Vector3Int coord)
    {
        if (coord.y < 0 || coord.y >= chunkSizeY)
            return true;
        return blockTypesProperties[BlocksData[coord.x][coord.y][coord.z]].isTransparent;
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
