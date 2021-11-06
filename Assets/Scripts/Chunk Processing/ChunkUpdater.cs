using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkUpdater : MonoBehaviour
{
    [SerializeField] private RegionChunk _regionChunk;
    [SerializeField] private float tickSpeed;
    public HashSet<Vector3Int> updateCurrentTick = new HashSet<Vector3Int>();
    public HashSet<Vector3Int> updateNextTick = new HashSet<Vector3Int>();

    public HashSet<Vector3Int> renderChunkToReDraw = new HashSet<Vector3Int>();
    
    
    private float tickTimer = 0;

    private BlockPropertyManager _blockPropertyManager;
    // Start is called before the first frame update
    void Start()
    {
        _blockPropertyManager = BlockPropertyManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (tickTimer >= tickSpeed && updateNextTick.Count>0)
        {
            updateCurrentTick = new HashSet<Vector3Int>(updateNextTick);
            updateNextTick.Clear();
            ProcessTick();
            tickTimer = 0;
        }

        tickTimer += Time.fixedDeltaTime;
    }

    void ProcessTick()
    {
        bool blockChanged = false;

        foreach (var processCoord in updateCurrentTick)
        {
            var blockData = _regionChunk.BlocksData[processCoord.x + 1][processCoord.y][processCoord.z + 1];
            if (_blockPropertyManager.blockClass[blockData.BlockType].BlockUpdate(_regionChunk, processCoord))
            {
                blockChanged = true;
            }
        }

        if (blockChanged)
        {
            //If block change, re render
            RedrawRenderChunks();
        }
    }

    void RedrawRenderChunks()
    {
        foreach (var idx in renderChunkToReDraw)
        {
            StartCoroutine(_regionChunk.CalculateDrawnMesh(idx.x, idx.y, idx.z));
        }
        renderChunkToReDraw.Clear();
    }

    public void AddToUpdateNextTick(Vector3Int blockPos)
    {
        updateNextTick.Add(blockPos);
        renderChunkToReDraw.Add(new Vector3Int(blockPos.x / RenderChunk.xSize,
            blockPos.y / RenderChunk.ySize, blockPos.z / RenderChunk.zSize));
        TerrainGen.instance.UpdateBorderingChunkData(_regionChunk, blockPos,
            _regionChunk.BlocksData[blockPos.x + 1][blockPos.y][blockPos.z + 1]);
    }
}
