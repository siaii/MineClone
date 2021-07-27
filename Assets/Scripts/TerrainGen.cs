using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TerrainGen : MonoBehaviour
{
    [SerializeField] private Image noiseImage;
    [FormerlySerializedAs("scale")] [SerializeField][Min(0.001f)] public float macroScale = 10;
    [SerializeField] [Min(0.001f)] public float microScale = 5;

    [SerializeField][Min(100)] private int textureWidth = 100;
    [SerializeField][Min(100)] private int textureHeight = 100;

    [SerializeField] private float xOffset = 0;
    [SerializeField] private float yOffset = 0;
    [SerializeField] private int seed;

    [SerializeField] private GameObject regionChunkPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerTransform;
    
    [SerializeField] private int renderDistance = 8;
    [SerializeField] private int minGenerationHeight = 32;
    [SerializeField][Range(0,1)] private float heightScale = 0.7f;
    [SerializeField] private int maxTreePerChunk = 5;
    [SerializeField] private float chunksWithTreesRatio = 0.4f;

    private Texture2D noiseTexture;

    private readonly Dictionary<Vector2Int, RegionChunk> activeRegionChunks = new Dictionary<Vector2Int, RegionChunk>();

    private List<RegionChunk> pooledRegionChunks = new List<RegionChunk>();

    private List<Vector2Int> toLoad = new List<Vector2Int>();
    
    private Dictionary<Vector2Int, BlockTypes[][][]> inactiveBlocksData = new Dictionary<Vector2Int, BlockTypes[][][]>();

    private Vector2Int _prevPlayerChunk;

    private int inLoadingChunk = 0;

    private int maxSimultaneousChunkLoading = 4;

    private GameObject playerGO;
    
    // Start is called before the first frame update
    void Start()
    {
        ProcessChunkChanges(true);
        _prevPlayerChunk = ChunkFromPosition(playerTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int curPlayerChunk = ChunkFromPosition(playerTransform.position);
        if (_prevPlayerChunk != curPlayerChunk)
        {
            ProcessChunkChanges();
        }

    }

    void ProcessChunkChanges(bool init = false)
    {
        var curPlayerChunk = ChunkFromPosition(playerTransform.position);
        if (_prevPlayerChunk == curPlayerChunk && !init)
        {
            return;
        }
        
        for (var i = -renderDistance; i <= renderDistance; i++)
        for (var j = -renderDistance; j <= renderDistance; j++)
        {
            var chunkCoord = new Vector2Int(curPlayerChunk.x + i, curPlayerChunk.y + j);
            if(!activeRegionChunks.ContainsKey(chunkCoord) && !toLoad.Contains(chunkCoord))
                toLoad.Add(chunkCoord);
        }

        List<Vector2Int> toDestroy = new List<Vector2Int>();
        
        foreach (var regionChunk in activeRegionChunks)
        {
            var chunkCoord = regionChunk.Key;
            if (chunkCoord.x < curPlayerChunk.x - renderDistance || chunkCoord.x > curPlayerChunk.x + renderDistance ||
                chunkCoord.y < curPlayerChunk.y - renderDistance || chunkCoord.y > curPlayerChunk.y + renderDistance)
            {
                toDestroy.Add(chunkCoord);
            }
        }

        DestroyChunk(toDestroy);
        
        StartCoroutine(DelayedLoadChunks());

        _prevPlayerChunk = curPlayerChunk;

        if (init)
        {
            StartCoroutine(WaitSpawnPlayer());
        }
    }

    IEnumerator WaitSpawnPlayer()
    {
        yield return new WaitUntil(() => toLoad.Count==0 && inLoadingChunk==0);
        //Spawn player
        int spawnX = Random.Range(0, RegionChunk.chunkSizeX);
        int spawnZ = Random.Range(0, RegionChunk.chunkSizeZ);
        float spawnY = SampleNoiseHeight((float)spawnX/(float)RegionChunk.chunkSizeX, (float)spawnZ/(float)RegionChunk.chunkSizeZ) + 2.5f;
        playerGO = Instantiate(playerPrefab, new Vector3(spawnX, spawnY, spawnZ), Quaternion.identity);
        playerTransform = playerGO.transform;
    }

    void DestroyChunk(List<Vector2Int> toDestroy)
    {
        foreach (var chunkCoord in toDestroy)
        {
            if (!activeRegionChunks.ContainsKey(chunkCoord))
                return;
            
            pooledRegionChunks.Add(activeRegionChunks[chunkCoord]);
                
            if (inactiveBlocksData.ContainsKey(chunkCoord))
            {
                inactiveBlocksData[chunkCoord] = activeRegionChunks[chunkCoord].BlocksData;
            }
            else
            {
                inactiveBlocksData.Add(chunkCoord, activeRegionChunks[chunkCoord].BlocksData);
            }
            StopCoroutine(activeRegionChunks[chunkCoord].renderCoroutine);            
            activeRegionChunks[chunkCoord].gameObject.SetActive(false);
            activeRegionChunks.Remove(chunkCoord);
        }
    }

    IEnumerator DelayedLoadChunks()
    {
        while (toLoad.Count > 0)
        {
            StartCoroutine(ActivateOrCreateChunk(toLoad[0]));
            toLoad.RemoveAt(0);
            yield return new WaitUntil(() => inLoadingChunk < maxSimultaneousChunkLoading);
        }
    }

    IEnumerator ActivateOrCreateChunk(Vector2Int chunkCoord)
    {
        inLoadingChunk++;
        var curPlayerChunk = ChunkFromPosition(playerTransform.position);
        if (chunkCoord.x < curPlayerChunk.x - renderDistance || chunkCoord.x > curPlayerChunk.x + renderDistance ||
            chunkCoord.y < curPlayerChunk.y - renderDistance || chunkCoord.y > curPlayerChunk.y + renderDistance || 
            activeRegionChunks.ContainsKey(chunkCoord))
        {
            inLoadingChunk--;
            yield break;
        }
        
        RegionChunk chunk;
        bool fromPooled = false;
        if (pooledRegionChunks.Count > 0)
        {
            chunk = pooledRegionChunks[0];
            pooledRegionChunks.RemoveAt(0);
            fromPooled = true;
        }
        else
        {
            var curChunk = Instantiate(regionChunkPrefab, new Vector3(chunkCoord.x*RegionChunk.chunkSizeX, 0, chunkCoord.y*RegionChunk.chunkSizeZ), Quaternion.identity);
            chunk = curChunk.GetComponent<RegionChunk>();
        }
        chunk.gameObject.SetActive(true);
        chunk.transform.position = new Vector3(chunkCoord.x * RegionChunk.chunkSizeX, 0,
            chunkCoord.y * RegionChunk.chunkSizeZ);
        activeRegionChunks.Add(chunkCoord, chunk);
        if (fromPooled)
        {
            chunk.ClearRenderMesh();
        }
        for (int x = 0; x < RegionChunk.chunkSizeX + 2; x++)
        {
            for (int z = 0; z < RegionChunk.chunkSizeZ + 2; z++)
            {
                int xPos = chunkCoord.x * RegionChunk.chunkSizeX + x - 1;
                int zPos = chunkCoord.y * RegionChunk.chunkSizeZ + z - 1;
                //Convert int coords to float with 1 chunk == 1 unit
                float xCoord = (float) xPos / (float)RegionChunk.chunkSizeX;
                float zCoord = (float) zPos / (float) RegionChunk.chunkSizeZ;

                int groundHeight = SampleNoiseHeight(xCoord, zCoord);
                int y;
                for (y = 0; y < groundHeight - 2; y++)
                {
                    chunk.BlocksData[x][y][z] = BlockTypes.STONE;
                }

                for (; y < groundHeight; y++)
                {
                    chunk.BlocksData[x][y][z] = BlockTypes.DIRT;
                }

                for (; y == groundHeight; y++)
                {
                    chunk.BlocksData[x][y][z] = BlockTypes.GRASS;
                }

                for (; y < RegionChunk.chunkSizeY; y++)
                {
                    chunk.BlocksData[x][y][z] = BlockTypes.AIR;
                }
            }
            yield return new WaitForSeconds(0.03f);
        }
        
        if (chunk.enabled)
        {
            GenerateTrees(chunk, chunkCoord);
        
            yield return StartCoroutine(chunk.GenerateRenderChunks());
        }
        inLoadingChunk--;
    }

    private void GenerateTrees(RegionChunk chunk, Vector2Int chunkCoord)
    {
        //Seed the random generator
        Random.InitState(seed + chunkCoord.x * chunkCoord.y);
        float sample = noise.snoise(new float2(Mathf.Sqrt(seed) + chunkCoord.x, Mathf.Sqrt(seed) + chunkCoord.y));
        if (sample > chunksWithTreesRatio)
        {
            List<Vector2Int> treeCoords = new List<Vector2Int>();
            int treeCount = Random.Range(0, maxTreePerChunk+1);
            for (int i = 0; i < treeCount; i++)
            {
                Random.InitState(seed + chunkCoord.x * chunkCoord.y + (i * 2 + 1));
                int treeX = RegionChunk.chunkSizeX/2 + Mathf.RoundToInt((Random.insideUnitCircle.magnitude * (RegionChunk.chunkSizeX / 2 - 2) * Random.insideUnitCircle.normalized).x);
                int treeZ = RegionChunk.chunkSizeZ/2 + Mathf.RoundToInt((Random.insideUnitCircle.magnitude * (RegionChunk.chunkSizeZ / 2 - 2) * Random.insideUnitCircle.normalized).y);
                
                //Don't generate if there is already another tree in a 3x3 centered on the generated coords
                bool skip = false;
                for (int a = -1; a <= 1; a++)
                {
                    for (int b = -1; b <= 1; b++)
                    {
                        if(treeCoords.Contains(new Vector2Int(treeX+a, treeZ+b)))
                        {
                            skip = true;
                            break;
                        }
                    }
                }

                if (skip)
                {
                    continue;
                }
                
                int treeHeight = Random.Range(4, 6);
                
                int y = RegionChunk.chunkSizeY - 1;
                while (chunk.BlocksData[treeX + 1][y][treeZ + 1] == BlockTypes.AIR)
                {
                    y--;
                }

                y++; //Move one block up from the actual ground
                for(int j=0; j<treeHeight; j++)
                {
                    chunk.BlocksData[treeX + 1][y+j][treeZ + 1] = BlockTypes.WOOD;
                }
                
                //Generate leaves
                Random.InitState(seed + chunkCoord.x * chunkCoord.y + (i * 2 + 1) + i + 10);
                int leafStart = Random.Range(1, treeHeight/2);
                
                Random.InitState(seed + chunkCoord.x * chunkCoord.y + (i * 2 + 1) + i);
                //Use 1-x^2 curve(?)
                int leafHeight = Random.Range(treeHeight - leafStart, treeHeight + 1);
                for (int c = leafStart; c <= leafStart + leafHeight; c++)
                {
                    for (int a = -2; a <= 2; a++)
                    {
                        for (int b = -2; b <= 2; b++)
                        {
                            if (a * a + (c - leafStart) + b * b <= leafHeight)
                            {
                                if (chunk.BlocksData[treeX + 1 + a][y + c][treeZ + 1 + b] == BlockTypes.AIR)
                                {
                                    chunk.BlocksData[treeX + 1 + a][y + c][treeZ + 1 + b] = BlockTypes.LEAF;
                                }
                            }           
                        }
                    }    
                }
                
                
                treeCoords.Add(new Vector2Int(treeX, treeZ));
            }
        }
    }

    public static Vector2Int ChunkFromPosition(Vector3 playerPosition)
    {
        int x = Mathf.RoundToInt(playerPosition.x);
        int z = Mathf.RoundToInt(playerPosition.z);

        int chunkX = x / RegionChunk.chunkSizeX;
        if (x < 0)
            chunkX -= 1;
        int chunkZ = z / RegionChunk.chunkSizeZ;
        if (z < 0)
            chunkZ -= 1;

        return new Vector2Int(chunkX, chunkZ);
    }

    int SampleNoiseHeight(float x, float y)
    {
        float macroSample = (noise.snoise(new float2(x * macroScale, y * macroScale)) + 1f) / 2f; //Normalized to 0-1
        int macroHeight = Mathf.RoundToInt(minGenerationHeight +
                                          macroSample * heightScale * (RegionChunk.chunkSizeY - minGenerationHeight));

        float microSample = noise.snoise(new float2((1000 + Mathf.Cos(1000) + x) * microScale, (1000 + Mathf.Sin(1000) +y) * microScale))/2f;
        int microHeight = Mathf.RoundToInt(Mathf.Sign(microSample) * microSample * microSample * 3);
        return macroHeight + microHeight;
    }

    BlockTypes GetBlockType(int xPos,int yPos, int zPos)
    {
        BlockTypes res = BlockTypes.AIR;

        //Convert int coords to float with 1 chunk == 1 unit
        float xCoord = (float) xPos / (float)RegionChunk.chunkSizeX;
        float zCoord = (float) zPos / (float) RegionChunk.chunkSizeZ;

        int groundHeight = SampleNoiseHeight(xCoord, zCoord);

        if (yPos > groundHeight)
            res = BlockTypes.AIR;
        else if(yPos==groundHeight)
        {
            res = BlockTypes.GRASS;
        }
        else if (yPos < groundHeight && yPos > groundHeight - 2)
        {
            res = BlockTypes.DIRT;
        }
        else
        {
            res = BlockTypes.STONE;
        }

        return res;
    }

    public RegionChunk GetRegionChunk(Vector2Int chunkID)
    {
        if (activeRegionChunks.ContainsKey(chunkID))
        {
            return activeRegionChunks[chunkID];
        }
        print("Chunk not found");
        return null;
    }
}
