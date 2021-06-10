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

    private readonly Dictionary<Vector2Int, RegionChunk> _regionChunks = new Dictionary<Vector2Int, RegionChunk>();

    private Vector2Int _prevPlayerChunk;

    private List<Vector2Int> _loadedChunkCoords = new List<Vector2Int>();

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
        GenerateChunks();
        GenerateChunksMesh();
        _prevPlayerChunk = ChunkFromPosition(playerTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int curPlayerChunk = ChunkFromPosition(playerTransform.position);
        if (_prevPlayerChunk != curPlayerChunk)
        {
            StartCoroutine(ProcessChunkChanges(curPlayerChunk));
        }

        
    }

    IEnumerator ProcessChunkChanges(Vector2Int curPlayerChunk)
    {
        List<Vector2Int> chunksToLoad = new List<Vector2Int>();
        for (var i = -renderDistance; i <= renderDistance; i++)
        for (var j = -renderDistance; j <= renderDistance; j++)
        {
            var chunkCoord = new Vector2Int(curPlayerChunk.x + i, curPlayerChunk.y + j);
            chunksToLoad.Add(chunkCoord);
            ActivateOrCreateChunk(chunkCoord);
            yield return null;
        }

        foreach (var chunkCoord in _regionChunks.Keys)
            if (!chunksToLoad.Contains(chunkCoord))
                _regionChunks[chunkCoord].SetActive(false);
        _prevPlayerChunk = curPlayerChunk;
    }

    void ActivateOrCreateChunk(Vector2Int chunkCoord)
    {
        if (_regionChunks.ContainsKey(chunkCoord))
        {
            _regionChunks[chunkCoord].SetActive(true);
        }
        else
        {
            GenerateSingleChunk(chunkCoord.x, chunkCoord.y);
            StartCoroutine(_regionChunks[chunkCoord].GenerateRenderChunks());
            foreach (var dirVec in _chunkRerenderDir)
            {
                if (_regionChunks.ContainsKey(chunkCoord + dirVec))
                {
                    StartCoroutine(_regionChunks[chunkCoord + dirVec].GenerateRenderChunks());
                }
            }
        }
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

    void GenerateSingleChunk(int xCoord, int zCoord)
    {
        var curChunk = Instantiate(regionChunkPrefab, new Vector3(xCoord*RegionChunk.chunkSizeX, 0, zCoord*RegionChunk.chunkSizeZ), Quaternion.identity);
        curChunk.name = xCoord + "," + zCoord;
        var curRegionChunk = curChunk.GetComponent<RegionChunk>();
        curRegionChunk.SetChunkPos(xCoord,zCoord);
        curRegionChunk.SetNoiseScale(scale);
        curRegionChunk.GenerateBlockData();

        var chunkCoords = new Vector2Int(xCoord, zCoord);
        _regionChunks.Add(chunkCoords, curRegionChunk);
        _loadedChunkCoords.Add(chunkCoords);
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
        foreach (var pair in _regionChunks)
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
        if (!_regionChunks.ContainsKey(chunkID))
            return false;
        return _regionChunks[chunkID].CheckBlockIsTransparent(xCoord, yCoord, zCoord);
    }
}
