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
    public const int chunkSizeY = 128;
    
    public int minBlockHeightGenerated = 48;

    private BlockTypes[,,] blocksData;
    private RenderChunk[,,] chunkData;

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

    private readonly Dictionary<BlockTypes, Block> blockTypesProperties = new Dictionary<BlockTypes, Block>()
    {
        {BlockTypes.GRASS, new Grass()},
        {BlockTypes.AIR, new Air()},
        {BlockTypes.DIRT, new Dirt()},
        {BlockTypes.STONE, new Stone()}
    };


    // Start is called before the first frame update
    void Start()
    {
        if (!_texturePacker)
        {
            _texturePacker = FindObjectOfType<TexturePacker>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateBlockData()
    {
        blocksData = new BlockTypes[chunkSizeX, chunkSizeY, chunkSizeZ];
        chunkData = new RenderChunk[chunkSizeX / RenderChunk.xSize, chunkSizeY / RenderChunk.ySize,
            chunkSizeZ / RenderChunk.zSize];
        
        _terrainGen = FindObjectOfType<TerrainGen>();
        
        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int z = 0; z < chunkSizeZ; z++)
            {
                float chunkCoordOffsetX = chunkPos.x * noiseScale;
                float chunkCoordOffsetZ = chunkPos.y * noiseScale;
                        
                float xCoord = chunkCoordOffsetX + (float)x / RegionChunk.chunkSizeX * noiseScale;
                float zCoord = chunkCoordOffsetZ + (float)z / RegionChunk.chunkSizeZ * noiseScale;

                int groundHeight = SampleNoiseHeight(xCoord, zCoord);

                int y;
                for (y = 0; y < groundHeight-1; y++)
                {
                    blocksData[x, y, z] = BlockTypes.STONE;
                }

                for (; y < groundHeight; y++)
                {
                    blocksData[x, y, z] = BlockTypes.DIRT;
                }

                for (; y < groundHeight + 1; y++)
                {
                    blocksData[x, y, z] = BlockTypes.GRASS;
                }
                
                for (; y < chunkSizeY; y++)
                {
                    blocksData[x, y, z] = BlockTypes.AIR;
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
                    GameObject renderChunk = Instantiate(renderChunkPrefab, this.transform);
                    renderChunk.transform.localPosition = new Vector3(x * RenderChunk.xSize, y * RenderChunk.ySize,
                        z * RenderChunk.zSize);
                    List<Vector3> renderChunkVertices = new List<Vector3>();
                    List<int> renderChunkTris = new List<int>();
                    List<Vector2> renderChunkUVs = new List<Vector2>();
                    
                    CalculateDrawnMesh(x, y, z, renderChunkVertices, renderChunkTris, renderChunkUVs);
                    renderChunk.GetComponent<RenderChunk>().BuildMesh(renderChunkVertices.ToArray(), renderChunkTris.ToArray(), renderChunkUVs.ToArray());
                    yield return null;
                }
            }
        }
    }

    int SampleNoiseHeight(float x, float y)
    {
        float sample = noise.snoise(new float2(x, y));
        float sampleNormd = (sample+1f)/2f;
        int roundedRes = Mathf.RoundToInt(minBlockHeightGenerated + sampleNormd * heightScale * (chunkSizeY - minBlockHeightGenerated));

        return roundedRes;
    }

    void CalculateDrawnMesh(int rChunkX,int rChunkY, int rChunkZ, List<Vector3> vertices, List<int> tris, List<Vector2> uvs)
    {
        if (!_texturePacker)
        {
            _texturePacker = FindObjectOfType<TexturePacker>();
        }
        int startX = rChunkX * RenderChunk.xSize;
        int startY = rChunkY * RenderChunk.ySize;
        int startZ = rChunkZ * RenderChunk.zSize;
        
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
                        int oldLength = vertices.Count;
                        vertices.AddRange(blockTypesProperties[blocksData[x,y,z]].GetSideVertices(side, new Vector3(localX,localY,localZ)));
                        
                        uvs.AddRange(GetBlockSideUVs(blocksData[x,y,z], side));
                        var blockTris = blockTypesProperties[blocksData[x, y, z]].GetSideTriangles();

                        foreach (var offset in blockTris)
                        {
                            tris.Add(oldLength+offset);
                        }
                    }
                }
            }
        }
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

    List<Sides> CheckBorderingTransparent(int xCoord, int yCoord, int zCoord)
    {
        List<Sides> sidesToDraw = new List<Sides>();
        foreach (var pair in sideVector)
        {
            Vector3Int dirVector = pair.Value;
            int checkBlockX = xCoord + dirVector.x;
            int checkBlockY = yCoord + dirVector.y;
            int checkBlockZ = zCoord + dirVector.z;
            if (IsValidCoordinates(checkBlockX, checkBlockY, checkBlockZ))
            {
                if (checkBlockY < 0 || checkBlockY >= chunkSizeY)
                    sidesToDraw.Add(pair.Key);
                else if( CheckBlockIsTransparent(checkBlockX, checkBlockY, checkBlockZ))
                    sidesToDraw.Add(pair.Key);
            }
            else
            {
                //Check for the block in the neighbouring chunk
                if (checkBlockX < 0)
                {
                    if (_terrainGen.CheckBlockIsTransparent(new Vector2Int(chunkPos.x - 1, chunkPos.y), chunkSizeX - 1,
                        checkBlockY, checkBlockZ))
                    {
                        sidesToDraw.Add(pair.Key);
                    }
                }else if (checkBlockX >= chunkSizeX)
                {
                    if (_terrainGen.CheckBlockIsTransparent(new Vector2Int(chunkPos.x + 1, chunkPos.y), 0,
                        checkBlockY, checkBlockZ))
                    {
                        sidesToDraw.Add(pair.Key);
                    }
                }else if (checkBlockZ < 0)
                {
                    if (_terrainGen.CheckBlockIsTransparent(new Vector2Int(chunkPos.x, chunkPos.y - 1), checkBlockX,
                        checkBlockY, chunkSizeZ - 1))
                    {
                        sidesToDraw.Add(pair.Key);
                    }
                }else if (checkBlockZ >= chunkSizeZ)
                {
                    if (_terrainGen.CheckBlockIsTransparent(new Vector2Int(chunkPos.x, chunkPos.y + 1), checkBlockX,
                        checkBlockY, 0))
                    {
                        sidesToDraw.Add(pair.Key);
                    }
                }
            }
        }

        return sidesToDraw;
    }

    public bool CheckBlockIsTransparent(int x, int y, int z)
    {
        if (y < 0 || y >= chunkSizeY)
            return true;
        return blockTypesProperties[blocksData[x, y, z]].isTransparent;
    }
    bool IsValidCoordinates(int x, int y, int z)
    {
        if(x<0 || x>=chunkSizeX || z<0 || z>=chunkSizeZ)
            return false;

        return true;
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

}
