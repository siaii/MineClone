using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    [SerializeField] private int renderDistance = 8;
    
    private Texture2D noiseTexture;

    private readonly Dictionary<Vector2Int, RegionChunk> _regionChunks = new Dictionary<Vector2Int, RegionChunk>();
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateChunks();
        GenerateChunksMesh();
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    void GenerateChunks()
    {
        for (int i = -renderDistance / 2; i <= renderDistance / 2; i++)
        {
            for (int j = -renderDistance / 2; j <= renderDistance / 2; j++)
            {
                var curChunk = Instantiate(regionChunkPrefab, new Vector3(i*RegionChunk.chunkSizeX, 0, j*RegionChunk.chunkSizeZ), Quaternion.identity);
                var curRegionChunk = curChunk.GetComponent<RegionChunk>();
                curRegionChunk.SetChunkPos(i,j);
                curRegionChunk.SetNoiseScale(scale);
                curRegionChunk.GenerateBlockData();
                
                _regionChunks.Add(new Vector2Int(i, j), curRegionChunk);
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
