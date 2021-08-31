using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegionChunk : MonoBehaviour
{
    [SerializeField] private GameObject renderChunkPrefab;
    [SerializeField] private GameObject waterChunkPrefab;
    [SerializeField] public ChunkUpdater _chunkUpdater;
    
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
        {BlockTypes.WATER_SOURCE, new Water_Source()},
        {BlockTypes.WATER_FLOWING, new Water_Flowing()}
    };

    private static readonly Dictionary<Sides, int[]> sidePartialMaxVertices = new Dictionary<Sides, int[]>()
    {
        {Sides.UP, new int[] {1, 2}},
        {Sides.DOWN, new int[] { }},
        {Sides.FRONT, new int[] {0, 3}},
        {Sides.BACK, new int[] {}},
        {Sides.RIGHT, new[] {0, 1}},
        {Sides.LEFT, new[] {2, 3}}
    };


    private void Awake()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
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
                    //Check all 6 direction if the face needs to be drawn
                    foreach (var pair in sideVector)
                    {
                        float rotationAmount = 0;
                        switch (BlocksData[x][y][z].BlockDirection)
                        {
                            case Sides.BACK:
                                rotationAmount = 0;
                                break;
                            case Sides.RIGHT:
                                rotationAmount = 90;
                                break;
                            case Sides.FRONT:
                                rotationAmount = 180;
                                break;
                            case Sides.LEFT:
                                rotationAmount = 270;
                                break;
                        }
                        Vector3 directionVector = pair.Value;
                        if (blockTypesProperties[BlocksData[x][y][z].BlockType].isDirectional)
                        {
                            directionVector = Quaternion.AngleAxis(rotationAmount, Vector3.up) * directionVector;
                        }
                        Vector3Int directionVectorInt = new Vector3Int(Mathf.RoundToInt(directionVector.x), Mathf.RoundToInt(directionVector.y), Mathf.RoundToInt(directionVector.z));
                        Vector3Int checkBlock = new Vector3Int(x, y, z) + directionVectorInt;

                        /*
                         * Conditions for the face to be drawn
                         * - It is bottom most or topmost block in the chunk
                         * - The neighbouring block is transparent AND different type of block
                         */
                        if (checkBlock.y < 0 || checkBlock.y >= chunkSizeY ||
                            (CheckBlockIsTransparent(checkBlock) && CheckIsNotSameBlock(BlocksData[x][y][z].BlockType, checkBlock)) || 
                            (blockTypesProperties[BlocksData[x][y][z].BlockType].isFluid && CheckIsNotSameBlock(BlocksData[x][y][z].BlockType, checkBlock) 
                                && pair.Key == Sides.UP))
                        {
                            BlockData data = BlocksData[x][y][z];
                            int localX = x - startX;
                            int localY = y - startY;
                            int localZ = z - startZ;
                            int oldLength;
                            //Different render chunk for water and solid
                            if (blockTypesProperties[data.BlockType].isFluid)
                            {
                                oldLength = waterVertices.Count;
                                if (blockTypesProperties[data.BlockType].isLeveled)
                                {
                                    if (data.BlockDirection != data.SubDirection)
                                    {
                                        var vertMainDir = blockTypesProperties[data.BlockType]
                                            .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ),
                                                data.BlockDirection, data.Level);
                                        
                                        //Bugged somewhere around here
                                        int subDirSideInt;
                                        if((int) data.BlockDirection > (int) data.SourceDirection)
                                        {
                                            subDirSideInt = (((int) data.BlockDirection - 2 - ((int) data.SourceDirection - 2)) % 4 + (int) pair.Key) % 6;
                                            if (subDirSideInt < 2)
                                            {
                                                subDirSideInt += 2;
                                            }
                                        }
                                        else
                                        {
                                            subDirSideInt = (((int) data.BlockDirection - 2 - ((int) data.SourceDirection - 2)) + (int) pair.Key) % 6;
                                            if (subDirSideInt < 2)
                                            {
                                                subDirSideInt += 4;
                                            }
                                        }
                                        

                                        var subDirSide = (Sides) subDirSideInt;
                                        
                                        var vertSubDir = blockTypesProperties[data.BlockType]
                                            .GetSideVertices(pair.Key == Sides.UP ? pair.Key : subDirSide, new Vector3(localX, localY, localZ),
                                                data.SubDirection, data.Level);

                                        //Both offset is -2 because Sides for int 0 and 1 is occupied by sides up and down (not relevant in this calculation)
                                        int offset = Math.Abs((int) data.BlockDirection - 2 - (int) data.SubDirection - 2);
                                        
                                        Sides reverseSide = ReverseHorizontalSide(pair.Key);
                                        var ax = blockTypesProperties[data.BlockType].GetSideVertices(reverseSide,
                                            new Vector3(localX, localY, localZ), data.SubDirection, data.Level + 1);
                                        
                                        waterVertices.AddRange(PartialMaxHeightVertex(
                                            MaxHeightVertex(vertMainDir, vertSubDir, offset), ax, pair.Key == Sides.UP ? offset : 0,
                                            sidePartialMaxVertices[pair.Key]));
                                        // waterVertices.AddRange(MaxHeightVertex(vertMainDir, vertSubDir, offset));
                                    }
                                    else
                                    {
                                        if (data.SourceDirection==data.BlockDirection)
                                        {
                                            waterVertices.AddRange(blockTypesProperties[data.BlockType]
                                                .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ),
                                                    data.BlockDirection, data.Level));
                                        }
                                        else
                                        {
                                            Sides reverseSide = ReverseHorizontalSide(pair.Key);
                                            var ax = blockTypesProperties[data.BlockType].GetSideVertices(reverseSide,
                                                new Vector3(localX, localY, localZ), data.SourceDirection, data.Level + 1);
                                            var vertMainDir = blockTypesProperties[data.BlockType]
                                                .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ),
                                                    data.BlockDirection, data.Level);
                                            int offset =  pair.Key == Sides.UP ? Math.Abs((int) data.BlockDirection - 2 - (int) data.SourceDirection - 2) : 0;
                                            waterVertices.AddRange(PartialMaxHeightVertex(vertMainDir, ax, offset,
                                                //Probably need to change this dictionary to account for vertex rotation along y axis
                                                sidePartialMaxVertices[pair.Key]));
                                            // if (pair.Key == Sides.UP)
                                            // {
                                            //     var ax = blockTypesProperties[data.BlockType].GetSideVertices(reverseSide,
                                            //         new Vector3(localX, localY, localZ), data.SourceDirection, data.Level + 1);
                                            //     var vertMainDir = blockTypesProperties[data.BlockType]
                                            //         .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ),
                                            //             data.BlockDirection, data.Level);
                                            //     int offset = Math.Abs((int) data.BlockDirection - 2 - (int) data.SourceDirection - 2);
                                            //     waterVertices.AddRange(PartialMaxHeightVertex(vertMainDir, ax, offset,
                                            //         //Probably need to change this dictionary to account for vertex rotation along y axis
                                            //         sidePartialMaxVertices[pair.Key]));
                                            //
                                            // }
                                            // else
                                            // {
                                            //     var ax = blockTypesProperties[data.BlockType].GetSideVertices(reverseSide,
                                            //         new Vector3(localX, localY, localZ), data.SourceDirection, data.Level + 1);
                                            //     var vertMainDir = blockTypesProperties[data.BlockType]
                                            //         .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ),
                                            //             data.BlockDirection, data.Level);
                                            //     int offset = Math.Abs((int) data.BlockDirection - 2 - (int) data.SourceDirection - 2);
                                            //     waterVertices.AddRange(PartialMaxHeightVertex(vertMainDir, ax, 0,
                                            //         //Probably need to change this dictionary to account for vertex rotation along y axis
                                            //         sidePartialMaxVertices[pair.Key]));
                                            // }


                                        }
                                    }
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
        //Also don't draw the face if it is both water
        if (currentBlockType == BlockTypes.WATER_SOURCE || currentBlockType == BlockTypes.WATER_FLOWING)
        {
            return BlocksData[checkBlock.x][checkBlock.y][checkBlock.z].BlockType != BlockTypes.WATER_SOURCE &&
                   BlocksData[checkBlock.x][checkBlock.y][checkBlock.z].BlockType != BlockTypes.WATER_FLOWING;
        }
        return currentBlockType != BlocksData[checkBlock.x][checkBlock.y][checkBlock.z].BlockType;
    }

    Vector2[] GetBlockSideUVs(BlockTypes type, Sides side, Sides upDirection = Sides.UP)
    {
        //If the block can be placed in multiple direction (eg. wood log) then take into account the direction for the uv
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

    private Vector3[] MaxHeightVertex(Vector3[] vertices1, Vector3[] vertices2, int offset)
    {
        Vector3[] res = new Vector3[vertices1.Length];
        for (int i = 0; i < vertices1.Length; i++)
        {
            res[i] = new Vector3(vertices1[i].x, Mathf.Max(vertices1[i].y, vertices2[(i + offset) % 4].y), vertices1[i].z);
        }
        return res;
    }

    private Vector3[] PartialMaxHeightVertex(Vector3[] vertices1, Vector3[] vertices2, int offset, int[] idx)
    {
        Vector3[] res = new Vector3[vertices1.Length];
        for (int i = 0; i < vertices1.Length; i++)
        {
            if (idx.Contains(i))
                res[i] = new Vector3(vertices1[i].x, Mathf.Max(vertices1[i].y, vertices2[(i + offset) % 4].y),
                    vertices1[i].z);
            else
                res[i] = vertices1[i];
        }
        return res;
    }

    private Sides ReverseHorizontalSide(Sides side)
    {
        switch (side)
        {
            case Sides.LEFT:
                return Sides.RIGHT;
            case Sides.RIGHT:
                return Sides.LEFT;
            case Sides.FRONT:
                return Sides.BACK;
            case Sides.BACK:
                return Sides.FRONT;
            default:
                return side;
        }
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
