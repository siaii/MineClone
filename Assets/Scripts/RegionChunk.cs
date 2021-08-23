using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionChunk : MonoBehaviour
{
    [SerializeField] private GameObject renderChunkPrefab;
    [SerializeField] private GameObject waterChunkPrefab;
    
    public const int chunkSizeX = 16;
    public const int chunkSizeZ = 16;
    public const int chunkSizeY = 96;
    
    public BlockData[][][] BlocksData
    {
        get;
        set;
    } = new BlockData[chunkSizeX + 2][][];
    private RenderChunk[][][] _renderChunks;
    private WaterChunk[][][] _waterChunks;

    public Vector2Int chunkPos;

    private float noiseScale = 1f;
    private float heightScale = 0.6f;

    private TerrainGen _terrainGen;
    private TexturePacker _texturePacker;

    public Coroutine renderCoroutine;
    
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
        {BlockTypes.LEAF, new Leaf()},
        {BlockTypes.WATER_SOURCE, new Water_Flowing()},
        {BlockTypes.WATER_FLOWING, new Water_Flowing()}
    };


    private void Awake()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
        _terrainGen = FindObjectOfType<TerrainGen>();
        for (int i = 0; i < chunkSizeX+2; i++)
        {
            BlocksData[i] = new BlockData[chunkSizeY][];
            for (int j = 0; j < chunkSizeY; j++)
            {
                BlocksData[i][j] = new BlockData[chunkSizeZ+2];
                for (int k = 0; k < chunkSizeZ + 2; k++)
                {
                    BlocksData[i][j][k] = new BlockData();
                }
            }
        }
        _renderChunks = new RenderChunk[chunkSizeX / RenderChunk.xSize][][];
        _waterChunks = new WaterChunk[chunkSizeX / WaterChunk.xSize][][];
        for (int i = 0; i < chunkSizeX / RenderChunk.xSize; i++)
        {
            _renderChunks[i] = new RenderChunk[chunkSizeY / RenderChunk.ySize][];
            _waterChunks[i] = new WaterChunk[chunkSizeY / WaterChunk.ySize][];
            for (int j = 0; j < chunkSizeY / RenderChunk.ySize; j++)
            {
                _renderChunks[i][j] = new RenderChunk[chunkSizeZ / RenderChunk.zSize];
                _waterChunks[i][j] = new WaterChunk[chunkSizeZ / WaterChunk.zSize];
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

    public void ClearRenderMesh()
    {
        for (int x = 0; x < _renderChunks.Length; x++)
        {
            for (int y = 0; y < _renderChunks[x].Length; y++)
            {
                for (int z = 0; z < _renderChunks[x][y].Length; z++)
                {
                    _renderChunks[x][y][z].BuildMesh(new Vector3[0], new int[0], new Vector2[0]);
                    _waterChunks[x][y][z].BuildMesh(new Vector3[0], new int[0], new Vector2[0]);
                }
            }
        }
    }

    public IEnumerator GenerateRenderChunks()
    {
        for (int x = 0; x < chunkSizeX / RenderChunk.xSize; x++)
        {
            for (int z = 0; z < chunkSizeZ / RenderChunk.zSize; z++)
            {
                for (int y = 0; y < chunkSizeY / RenderChunk.ySize; y++)
                {
                    if (_renderChunks[x][y][z] == null)
                    {
                        GameObject renderChunkGO = Instantiate(renderChunkPrefab, this.transform);
                        RenderChunk renderChunkScript = renderChunkGO.GetComponent<RenderChunk>();
                        renderChunkGO.transform.localPosition = new Vector3(x * RenderChunk.xSize, y * RenderChunk.ySize,
                            z * RenderChunk.zSize);
                        _renderChunks[x][y][z] = renderChunkScript;
                    }

                    if (_waterChunks[x][y][z] == null)
                    {
                        GameObject waterChunkGO = Instantiate(waterChunkPrefab, this.transform);
                        WaterChunk waterChunkScript = waterChunkGO.GetComponent<WaterChunk>();
                        waterChunkGO.transform.localPosition = new Vector3(x * WaterChunk.xSize, y * WaterChunk.ySize,
                            z * WaterChunk.zSize);
                        _waterChunks[x][y][z] = waterChunkScript;
                    }

                    if (this.gameObject.activeSelf)
                    {
                        renderCoroutine  = StartCoroutine(CalculateDrawnMesh(x, y, z));
                    }
                    else
                    {
                        yield break;
                    }
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

        List<Vector3> waterVertices = new List<Vector3>();
        List<int> waterTris = new List<int>();
        List<Vector2> waterUvs = new List<Vector2>();
        
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
                            (CheckBlockIsTransparent(checkBlock) && CheckIsNotSameBlock(BlocksData[x][y][z].BlockType, checkBlock)))
                        {
                            BlockData data = BlocksData[x][y][z];
                            int localX = x - startX;
                            int localY = y - startY;
                            int localZ = z - startZ;
                            int oldLength;
                            if (blockTypesProperties[data.BlockType].isFluid)
                            {
                                oldLength = waterVertices.Count;
                                if (blockTypesProperties[data.BlockType].isLeveled)
                                {
                                    waterVertices.AddRange(blockTypesProperties[data.BlockType]
                                        .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ),
                                            data.BlockDirection, data.Level));
                                }
                                else
                                {
                                    waterVertices.AddRange(blockTypesProperties[data.BlockType]
                                        .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ)));
                                }
                                waterUvs.AddRange(GetBlockSideUVs(data.BlockType, pair.Key));
                                var blockTris = blockTypesProperties[data.BlockType].GetSideTriangles(pair.Key);
                                
                                foreach (var offset in blockTris)
                                {
                                    waterTris.Add(oldLength+offset);
                                }    
                            }
                            else
                            {
                                oldLength = vertices.Count;
                                vertices.AddRange(blockTypesProperties[data.BlockType]
                                    .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ)));

                                uvs.AddRange(GetBlockSideUVs(data.BlockType, pair.Key, data.BlockDirection));
                                var blockTris = blockTypesProperties[data.BlockType].GetSideTriangles(pair.Key);
                                foreach (var offset in blockTris)
                                {
                                    tris.Add(oldLength + offset);
                                }
                            }
                        }
                    }
                }
            }
            yield return null;
        }
        _waterChunks[rChunkX][rChunkY][rChunkZ].BuildMesh(waterVertices.ToArray(), waterTris.ToArray(), waterUvs.ToArray());
        _renderChunks[rChunkX][rChunkY][rChunkZ].BuildMesh(vertices.ToArray(), tris.ToArray(), uvs.ToArray());
    }

    private bool CheckIsNotSameBlock(BlockTypes currentBlockType, Vector3Int checkBlock)
    {
        return currentBlockType != BlocksData[checkBlock.x][checkBlock.y][checkBlock.z].BlockType;
    }

    Vector2[] GetBlockSideUVs(BlockTypes type, Sides side, Sides upDirection = Sides.UP)
    {
        var localUV = blockTypesProperties[type].GetSideUVs(side, blockTypesProperties[type].isDirectional ? upDirection : Sides.UP);
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
        return blockTypesProperties[BlocksData[coord.x][coord.y][coord.z].BlockType].isTransparent;
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
