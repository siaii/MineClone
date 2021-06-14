using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TerrainGen : MonoBehaviour
{
    [SerializeField] private Image noiseImage;
    [SerializeField][Min(0.001f)] private float scale = 10;

    [SerializeField][Min(100)] private int textureWidth = 100;
    [SerializeField][Min(100)] private int textureHeight = 100;

    [SerializeField] private float xOffset = 0;
    [SerializeField] private float yOffset = 0;
    [SerializeField] private int seed;

    [SerializeField] private GameObject regionChunkPrefab;
    [SerializeField] private Transform playerTransform;
    
    [SerializeField] private int renderDistance = 8;

    private Texture2D noiseTexture;

    private readonly Dictionary<Vector2Int, RegionChunk> activeRegionChunks = new Dictionary<Vector2Int, RegionChunk>();

    private List<RegionChunk> pooledRegionChunks = new List<RegionChunk>();

    private List<Vector2Int> toLoad = new List<Vector2Int>();
    
    private Dictionary<Vector2Int, BlockTypes[,,]> inactiveBlocksData = new Dictionary<Vector2Int, BlockTypes[,,]>();

    private Vector2Int _prevPlayerChunk;

    private Vector2Int[] _chunkRerenderDir = new[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
    };
    
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

        foreach (var chunkCoord in toLoad)
        {
            if (chunkCoord.x < curPlayerChunk.x - renderDistance || chunkCoord.x > curPlayerChunk.x + renderDistance ||
                chunkCoord.y < curPlayerChunk.y - renderDistance || chunkCoord.y > curPlayerChunk.y + renderDistance)
            {
                toLoad.Remove(chunkCoord);
            }
        }

        DelayedDestroyChunk(toDestroy);
        
        StartCoroutine(DelayedLoadChunks());

        _prevPlayerChunk = curPlayerChunk;
    }

    void DelayedDestroyChunk(List<Vector2Int> toDestroy)
    {
        foreach (var chunkCoord in toDestroy)
        {
            if (!activeRegionChunks.ContainsKey(chunkCoord))
                return;
            
            activeRegionChunks[chunkCoord].gameObject.SetActive(false);
            pooledRegionChunks.Add(activeRegionChunks[chunkCoord]);
                
            if (inactiveBlocksData.ContainsKey(chunkCoord))
            {
                inactiveBlocksData[chunkCoord] = activeRegionChunks[chunkCoord].BlocksData;
            }
            else
            {
                inactiveBlocksData.Add(chunkCoord, activeRegionChunks[chunkCoord].BlocksData);
            }
                
            activeRegionChunks.Remove(chunkCoord);
        }
    }

    IEnumerator DelayedLoadChunks()
    {
        while (toLoad.Count > 0)
        {
            ActivateOrCreateChunk(toLoad[0]);
            toLoad.RemoveAt(0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ActivateOrCreateChunk(Vector2Int chunkCoord)
    {
        RegionChunk chunk;
        if (pooledRegionChunks.Count > 0)
        {
            chunk = pooledRegionChunks[0];
            pooledRegionChunks.RemoveAt(0);
        }
        else
        {
            var curChunk = Instantiate(regionChunkPrefab, new Vector3(chunkCoord.x*RegionChunk.chunkSizeX, 0, chunkCoord.y*RegionChunk.chunkSizeZ), Quaternion.identity);
            chunk = curChunk.GetComponent<RegionChunk>();
        }

        for (int x = 0; x < RegionChunk.chunkSizeX + 2; x++)
        {
            for (int z = 0; z < RegionChunk.chunkSizeZ + 2; z++)
            {
                for (int y = 0; y < RegionChunk.chunkSizeY; y++)
                {
                    chunk.BlocksData[x, y, z] = GetBlockType(chunkCoord.x * RegionChunk.chunkSizeX + x - 1,
                        y, chunkCoord.y * RegionChunk.chunkSizeZ + z - 1);
                }
            }
        }
            
        StartCoroutine(chunk.GenerateRenderChunks());
        chunk.transform.position = new Vector3(chunkCoord.x * RegionChunk.chunkSizeX, 0,
            chunkCoord.y * RegionChunk.chunkSizeZ);
        activeRegionChunks.Add(chunkCoord, chunk);
        chunk.gameObject.SetActive(true);
    }

    Vector2Int ChunkFromPosition(Vector3 playerPosition)
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
        float sample = noise.snoise(new float2(x, y));
        float sampleNormd = (sample+1f)/2f;
        int roundedRes = Mathf.RoundToInt(RegionChunk.chunkSizeY * 0.5f + sampleNormd * 0.5f * (RegionChunk.chunkSizeY * 0.5f));

        return roundedRes;
    }

    BlockTypes GetBlockType(int xPos,int yPos, int zPos)
    {
        BlockTypes res = BlockTypes.AIR;

        float xCoord = (float) xPos / (float)RegionChunk.chunkSizeX * scale;
        float zCoord = (float) zPos / (float) RegionChunk.chunkSizeZ * scale;

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

    void GenerateSingleChunk(int xCoord, int zCoord)
    {
        var curChunk = Instantiate(regionChunkPrefab, new Vector3(xCoord*RegionChunk.chunkSizeX, 0, zCoord*RegionChunk.chunkSizeZ), Quaternion.identity);
        var curRegionChunk = curChunk.GetComponent<RegionChunk>();
        curRegionChunk.SetChunkPos(xCoord,zCoord);
        curRegionChunk.SetNoiseScale(scale);
        
        for (int x = 0; x < RegionChunk.chunkSizeX + 2; x++)
        {
            for (int z = 0; z < RegionChunk.chunkSizeZ + 2; z++)
            {
                for (int y = 0; y < RegionChunk.chunkSizeY; y++)
                {
                    curRegionChunk.BlocksData[x, y, z] = GetBlockType(xCoord * RegionChunk.chunkSizeX + x - 1,
                        y, zCoord * RegionChunk.chunkSizeZ - 1);
                }
            }
        }

        curRegionChunk.GenerateRenderChunks();
        
        activeRegionChunks.Add(new Vector2Int(xCoord, zCoord), curRegionChunk);
    }
    void GenerateChunks()
    {
        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            for (int j = -renderDistance; j <= renderDistance; j++)
            {
                GenerateSingleChunk(i,j);
            }            
        }
    }

    void GenerateChunksMesh()
    {
        foreach (var pair in activeRegionChunks)
        {
            StartCoroutine(pair.Value.GenerateRenderChunks());
        }
    }

    public void CalcTexture()
    {
        seed = Random.Range(-100, 100);
        noiseTexture = new Texture2D(textureWidth, textureHeight);
        for (int i = -(renderDistance / 2)*RegionChunk.chunkSizeX; i <= (renderDistance / 2)*RegionChunk.chunkSizeX; i++)
        {
            for (int j = -(renderDistance / 2)*RegionChunk.chunkSizeZ; j < (renderDistance / 2)*RegionChunk.chunkSizeZ; j++)
            {
                float xCoord = xOffset + (float)i / noiseTexture.width * scale;
                float yCoord = yOffset + (float)j / noiseTexture.height * scale;
                
                //Perlin noise repeats at integers, and returns a float value of 0-1
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                noiseTexture.SetPixel(i, j, new Color(sample, sample, sample));
            }
        }
        noiseTexture.Apply();
    }

    public bool CheckBlockIsTransparent(Vector2Int chunkID, int xCoord, int yCoord, int zCoord)
    {
        if (!activeRegionChunks.ContainsKey(chunkID))
            return false;
        return activeRegionChunks[chunkID].CheckBlockIsTransparent(xCoord, yCoord, zCoord);
    }
}
