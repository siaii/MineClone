using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
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
                        //Account face check vector for the direction of the block
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
                        if (BlockPropertyManager.blockProperties[BlocksData[x][y][z].BlockType].isDirectional)
                        {
                            directionVector = Quaternion.AngleAxis(rotationAmount, Vector3.up) * directionVector;
                        }
                        Vector3Int directionVectorInt = new Vector3Int(Mathf.RoundToInt(directionVector.x), Mathf.RoundToInt(directionVector.y), Mathf.RoundToInt(directionVector.z));
                        Vector3Int checkBlock = new Vector3Int(x, y, z) + directionVectorInt;
                        
                        if (IsBlockSideDrawn(new Vector3Int(x,y,z), checkBlock, pair.Key))
                        {
                            BlockData curData = BlocksData[x][y][z];
                            int localX = x - startX;
                            int localY = y - startY;
                            int localZ = z - startZ;
                            int oldLength;
                            //Different render chunk for water and solid
                            if (BlockPropertyManager.blockProperties[curData.BlockType].isFluid)
                            {
                                oldLength = waterVertices.Count;
                                //If isLeveled (flowing water only for now)
                                if (BlockPropertyManager.blockProperties[curData.BlockType].isLeveled)
                                {
                                    var localRenderChunkPos = new Vector3(localX, localY, localZ);
                                    var mainVert = BlockPropertyManager.blockProperties[curData.BlockType]
                                        .GetSideVertices(pair.Key, localRenderChunkPos, curData.BlockDirection, curData.Level);
                                    Vector3[] res = (Vector3[]) mainVert.Clone();
                                    if (pair.Key == Sides.UP)
                                    {
                                        Dictionary<Sides, Vector3Int> checkDict = new Dictionary<Sides, Vector3Int>()
                                        {
                                            {Sides.RIGHT, Vector3Int.right},
                                            {Sides.FRONT, Vector3Int.back},
                                            {Sides.LEFT, Vector3Int.left},
                                            {Sides.BACK, Vector3Int.forward}
                                        };

                                        foreach (var vertCheck in checkDict)
                                        {
                                            Vector3Int vertCheckBlock = new Vector3Int(x, y, z) + vertCheck.Value;
                                            BlockData vertCheckData =
                                                BlocksData[vertCheckBlock.x][vertCheckBlock.y][vertCheckBlock.z];

                                            Vector3[] sideVertMainDirection =
                                            {
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                            };
                                            Vector3[] sideVertSubDirection =
                                            {
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                            };
                                            if (vertCheckData.BlockType == BlockTypes.WATER_FLOWING && vertCheckData.Level>=curData.Level 
                                                || vertCheckData.BlockType == BlockTypes.WATER_SOURCE)
                                            {
                                                sideVertMainDirection = BlockPropertyManager.blockProperties[vertCheckData.BlockType]
                                                    .GetSideVertices(
                                                        ConvertGlobalSideToLocalSide(Side.ReverseHorizontalSide(vertCheck.Key),
                                                            vertCheckData.BlockDirection),
                                                        localRenderChunkPos + vertCheck.Value,
                                                        vertCheckData.BlockDirection, vertCheckData.Level);
                                                sideVertSubDirection = BlockPropertyManager.blockProperties[vertCheckData.BlockType]
                                                    .GetSideVertices(
                                                        ConvertGlobalSideToLocalSide(Side.ReverseHorizontalSide(vertCheck.Key),
                                                            vertCheckData.SubDirection),
                                                        localRenderChunkPos + vertCheck.Value,
                                                        vertCheckData.SubDirection, vertCheckData.Level);
                                            }

                                            int offset = 0;
                                            switch (curData.BlockDirection)
                                            {
                                                case Sides.RIGHT:
                                                    offset = 1;
                                                    break;
                                                case Sides.FRONT:
                                                    offset = 2;
                                                    break;
                                                case Sides.LEFT:
                                                    offset = 3;
                                                    break;
                                            }
                                            
                                            switch (vertCheck.Key)
                                            {
                                                case Sides.RIGHT:
                                                    res[(3 + offset) % 4].y = Mathf.Max(res[(3 + offset) % 4].y, sideVertMainDirection[0].y);
                                                    res[(2 + offset) % 4].y = Mathf.Max(res[(2 + offset) % 4].y, sideVertMainDirection[3].y);
                                                    res[(3 + offset) % 4].y = Mathf.Max(res[(3 + offset) % 4].y, sideVertSubDirection[0].y);
                                                    res[(2 + offset) % 4].y = Mathf.Max(res[(2 + offset) % 4].y, sideVertSubDirection[3].y);
                                                    break;
                                                case Sides.FRONT:
                                                    res[(2 + offset) % 4].y = Mathf.Max(res[(2 + offset) % 4].y, sideVertMainDirection[0].y);
                                                    res[(1 + offset) % 4].y = Mathf.Max(res[(1 + offset) % 4].y, sideVertMainDirection[3].y);
                                                    res[(2 + offset) % 4].y = Mathf.Max(res[(2 + offset) % 4].y, sideVertSubDirection[0].y);
                                                    res[(1 + offset) % 4].y = Mathf.Max(res[(1 + offset) % 4].y, sideVertSubDirection[3].y);
                                                    break;
                                                case Sides.LEFT:
                                                    res[(1 + offset) % 4].y = Mathf.Max(res[(1 + offset) % 4].y, sideVertMainDirection[0].y);
                                                    res[(0 + offset) % 4].y = Mathf.Max(res[(0 + offset) % 4].y, sideVertMainDirection[3].y);
                                                    res[(1 + offset) % 4].y = Mathf.Max(res[(1 + offset) % 4].y, sideVertSubDirection[0].y);
                                                    res[(0 + offset) % 4].y = Mathf.Max(res[(0 + offset) % 4].y, sideVertSubDirection[3].y);
                                                    break;
                                                case Sides.BACK:
                                                    res[(0 + offset) % 4].y = Mathf.Max(res[(0 + offset) % 4].y, sideVertMainDirection[0].y);
                                                    res[(3 + offset) % 4].y = Mathf.Max(res[(3 + offset) % 4].y, sideVertMainDirection[3].y);
                                                    res[(0 + offset) % 4].y = Mathf.Max(res[(0 + offset) % 4].y, sideVertSubDirection[0].y);
                                                    res[(3 + offset) % 4].y = Mathf.Max(res[(3 + offset) % 4].y, sideVertSubDirection[3].y);
                                                    break;
                                                default:
                                                    print("error");
                                                    break;
                                            }
                                        }
                                    }
                                    else if(pair.Key == Sides.DOWN)
                                    {
                                        //Do nothing
                                    }
                                    else
                                    {
                                        Dictionary<Sides, Vector3Int> checkDict = new Dictionary<Sides, Vector3Int>()
                                        {
                                            {Sides.UP, Vector3Int.up},
                                            {Sides.RIGHT, Vector3Int.right},
                                            {Sides.BACK, Vector3Int.forward},
                                            {Sides.LEFT, Vector3Int.left},
                                        };

                                        foreach (var vertCheck in checkDict)
                                        {
                                            //Rotate the vertCheck
                                            var adjustedVertCheck =
                                                new KeyValuePair<Sides, Vector3Int>(
                                                    ConvertLocalToGlobalSide( //rename this
                                                        vertCheck.Key,
                                                        ConvertLocalToGlobalSide(pair.Key, curData.BlockDirection)),
                                                    sideVector[
                                                        ConvertLocalToGlobalSide(
                                                            vertCheck.Key,
                                                            ConvertLocalToGlobalSide(pair.Key, curData.BlockDirection))]);
                                            
                                            Vector3Int vertCheckBlock = new Vector3Int(x, y, z) + adjustedVertCheck.Value;
                                            BlockData vertCheckData =
                                                BlocksData[vertCheckBlock.x][vertCheckBlock.y][vertCheckBlock.z];

                                            Vector3[] sideVertMainDirection =
                                            {
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                            };
                                            Vector3[] sideVertSubDirection =
                                            {
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                                Vector3.negativeInfinity, 
                                            };
                                            if (vertCheckData.BlockType == BlockTypes.WATER_FLOWING && vertCheckData.Level>=curData.Level 
                                                || vertCheckData.BlockType == BlockTypes.WATER_SOURCE)
                                            {
                                                sideVertMainDirection = BlockPropertyManager.blockProperties[vertCheckData.BlockType]
                                                    .GetSideVertices(
                                                        ConvertGlobalSideToLocalSide(Side.ReverseHorizontalSide(adjustedVertCheck.Key), vertCheckData.BlockDirection),
                                                        localRenderChunkPos + adjustedVertCheck.Value, //Adjust vertcheck value here later
                                                        vertCheckData.BlockDirection, vertCheckData.Level);
                                                sideVertSubDirection = BlockPropertyManager.blockProperties[vertCheckData.BlockType]
                                                    .GetSideVertices(
                                                        ConvertGlobalSideToLocalSide(Side.ReverseHorizontalSide(adjustedVertCheck.Key), vertCheckData.SubDirection),
                                                        localRenderChunkPos + adjustedVertCheck.Value,
                                                        vertCheckData.SubDirection, vertCheckData.Level);
                                            }
                                            
                                            switch (vertCheck.Key)
                                            {
                                                case Sides.RIGHT:
                                                    res[0].y = Mathf.Max(res[0].y, sideVertMainDirection[0].y);
                                                    res[0].y = Mathf.Max(res[0].y, sideVertSubDirection[0].y);
                                                    break;
                                                case Sides.BACK:
                                                    res[0].y = Mathf.Max(res[0].y, sideVertMainDirection[3].y);
                                                    res[3].y = Mathf.Max(res[3].y, sideVertMainDirection[0].y);
                                                    res[0].y = Mathf.Max(res[0].y, sideVertSubDirection[3].y);
                                                    res[3].y = Mathf.Max(res[3].y, sideVertSubDirection[0].y);
                                                    break;
                                                case Sides.LEFT:
                                                    res[3].y = Mathf.Max(res[3].y, sideVertMainDirection[3].y);
                                                    res[3].y = Mathf.Max(res[3].y, sideVertSubDirection[3].y);
                                                    break;
                                                case Sides.UP:
                                                    res[0].y = Mathf.Max(res[0].y, sideVertMainDirection[3].y);
                                                    res[3].y = Mathf.Max(res[3].y, sideVertMainDirection[0].y);
                                                    res[0].y = Mathf.Max(res[0].y, sideVertSubDirection[3].y);
                                                    res[3].y = Mathf.Max(res[3].y, sideVertSubDirection[0].y);
                                                    break;
                                                default:
                                                    print("error");
                                                    break;
                                            }
                                        }
                                    }
                                    waterVertices.AddRange(res);
                                }
                                else
                                {
                                    waterVertices.AddRange(BlockPropertyManager.blockProperties[curData.BlockType]
                                        .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ)));
                                }
                                waterUvs.AddRange(GetBlockSideUVs(curData.BlockType, pair.Key));
                                var blockTris = BlockPropertyManager.blockProperties[curData.BlockType].GetSideTriangles(pair.Key);
                                
                                foreach (var offset in blockTris)
                                {
                                    waterTris.Add(oldLength+offset);
                                }    
                            }
                            else
                            {
                                oldLength = vertices.Count;
                                vertices.AddRange(BlockPropertyManager.blockProperties[curData.BlockType]
                                    .GetSideVertices(pair.Key, new Vector3(localX, localY, localZ)));

                                uvs.AddRange(GetBlockSideUVs(curData.BlockType, pair.Key, curData.BlockDirection));
                                var blockTris = BlockPropertyManager.blockProperties[curData.BlockType].GetSideTriangles(pair.Key);
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

    /*
     * Conditions for the face to be drawn
     * - It is bottom most or topmost block in the chunk
     * - The neighbouring block is transparent AND different type of block
     *  - The block is a fluid and the block above is a different type of block
     */
    private bool IsBlockSideDrawn(Vector3Int blockPos, Vector3Int checkBlock, Sides checkSide)
    {
        BlockData blockData = BlocksData[blockPos.x][blockPos.y][blockPos.z];
        return checkBlock.y < 0 ||
               checkBlock.y >= chunkSizeY ||
               (CheckBlockIsTransparent(checkBlock) &&
                CheckIsNotSameBlock(blockData.BlockType, checkBlock)) ||
               (BlockPropertyManager.blockProperties[blockData.BlockType].isFluid &&
                CheckIsNotSameBlock(blockData.BlockType, checkBlock)
                && checkSide == Sides.UP);
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
        var localUV = BlockPropertyManager.blockProperties[type].GetSideUVs(side, BlockPropertyManager.blockProperties[type].isDirectional ? upDirection : Sides.UP);
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
        return BlockPropertyManager.blockProperties[BlocksData[coord.x][coord.y][coord.z].BlockType].isTransparent;
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

    //Only get the max height for vertices with index contained in idx
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

    //Horizontal side only, still not working correctly
    private Sides ConvertGlobalSideToLocalSide(Sides globalSide, Sides blockDirection)
    {
        if (globalSide == Sides.UP || globalSide == Sides.DOWN)
        {
            return globalSide;
        }

        int offset = 0;
        switch (blockDirection)
        {
            case Sides.BACK:
                offset = 0;
                break;
            case Sides.RIGHT:
                offset = 1;
                break;
            case Sides.FRONT:
                offset = 2;
                break;
            case Sides.LEFT:
                offset = 3;
                break;
        }

        int res = (((int) globalSide - offset) % Enum.GetNames(typeof(Sides)).Length + Enum.GetNames(typeof(Sides)).Length) % Enum.GetNames(typeof(Sides)).Length;

        if (res < 2)
            res += 4;
        
        return (Sides) res;
    }
    
    private Sides ConvertLocalToGlobalSide(Sides localSide, Sides blockDirection)
    {
        if (localSide == Sides.UP || localSide == Sides.DOWN)
        {
            return localSide;
        }

        int offset = 0;
        switch (blockDirection)
        {
            case Sides.BACK:
                offset = 0;
                break;
            case Sides.RIGHT:
                offset = 1;
                break;
            case Sides.FRONT:
                offset = 2;
                break;
            case Sides.LEFT:
                offset = 3;
                break;
        }

        int res = ((int) localSide + offset) % Enum.GetNames(typeof(Sides)).Length;

        if (res < 2)
            res += 2;

        if (offset == 3 && localSide == Sides.BACK)
            res += 2;
        
        return (Sides) res;
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
