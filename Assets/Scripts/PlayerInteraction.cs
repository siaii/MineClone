using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Camera _mainCamera;

    private Vector2 _cameraCenter;

    private TerrainGen _terrainGen;
    // Start is called before the first frame update
    void Start()
    {
        _terrainGen = FindObjectOfType<TerrainGen>();
        _mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _cameraCenter = new Vector2(_mainCamera.pixelWidth / 2, _mainCamera.pixelHeight / 2);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMouseInput();
    }

    void ProcessMouseInput()
    {
        if (Input.GetAxis("Fire1") > 0f)
        {
            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(_cameraCenter);
            var hitSomething = Physics.Raycast(ray, out hit);
            if (hitSomething)
            {
                RegionChunk collidedChunk = hit.collider.transform.parent.GetComponent<RegionChunk>();
                //Make sure the coordinate to get the block is the correct block, thus the -0.5f
                Vector3 adjustedHitCoord = hit.point + hit.normal * -0.5f;
                //Local within chunk (0-15) block coordinate
                Vector3Int blockCoord = WorldCoordToChunkBlockCoord(adjustedHitCoord);
                
                //Possibly refactor this when adding ability to put blocks, and also keep the record of modified blocks in TerrainGen
                collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1] = BlockTypes.AIR;

                //Calculate
                StartCoroutine(collidedChunk.CalculateDrawnMesh(blockCoord.x / RenderChunk.xSize,
                    blockCoord.y / RenderChunk.ySize, blockCoord.z / RenderChunk.zSize));
                
                //Rerender the neighbouring render chunks
                switch (blockCoord.y%RenderChunk.ySize)
                {
                    case(0):
                        if (blockCoord.y / RenderChunk.ySize > 0)
                            StartCoroutine(collidedChunk.CalculateDrawnMesh(blockCoord.x / RenderChunk.xSize,
                                blockCoord.y / RenderChunk.ySize - 1, blockCoord.z / RenderChunk.zSize));
                        break;
                    case(RenderChunk.ySize-1):
                        if (blockCoord.y / RenderChunk.ySize < RegionChunk.chunkSizeY/RenderChunk.ySize-1)
                            StartCoroutine(collidedChunk.CalculateDrawnMesh(blockCoord.x / RenderChunk.xSize,
                                blockCoord.y / RenderChunk.ySize + 1, blockCoord.z / RenderChunk.zSize));
                        break;
                }
                
                //Extra logic needed to access the render chunk of different region chunk
                var blockCoordCopy = blockCoord;
                Vector2Int chunkID; 
                RegionChunk regChunk; 
                switch (blockCoord.x)
                {
                    case(0):
                        //Get the neighbouring chunk to update
                        chunkID = _terrainGen.ChunkFromPosition(new Vector3(adjustedHitCoord.x-1f, adjustedHitCoord.y, adjustedHitCoord.z));
                        regChunk = _terrainGen.GetRegionChunk(chunkID);
                        blockCoordCopy.x = RegionChunk.chunkSizeX-1;
                        //Modify the block data copy in the neighbour chunk
                        regChunk.BlocksData[blockCoordCopy.x + 1 + 1][blockCoordCopy.y][blockCoordCopy.z + 1] =
                            collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1];
                        //Recalculate the corresponding render chunk
                        StartCoroutine(regChunk.CalculateDrawnMesh(blockCoordCopy.x / RenderChunk.xSize,
                            blockCoordCopy.y / RenderChunk.ySize, blockCoordCopy.z / RenderChunk.zSize));
                        break;
                    case(RegionChunk.chunkSizeX-1):
                        chunkID= _terrainGen.ChunkFromPosition(new Vector3(adjustedHitCoord.x+1f, adjustedHitCoord.y, adjustedHitCoord.z));
                        regChunk = _terrainGen.GetRegionChunk(chunkID);
                        blockCoordCopy.x = 0;
                        regChunk.BlocksData[blockCoordCopy.x + 1 - 1][blockCoordCopy.y][blockCoordCopy.z + 1] =
                            collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1];
                        StartCoroutine(regChunk.CalculateDrawnMesh(blockCoordCopy.x / RenderChunk.xSize,
                            blockCoordCopy.y / RenderChunk.ySize, blockCoordCopy.z / RenderChunk.zSize));
                        break;
                }
                
                blockCoordCopy = blockCoord;
                switch (blockCoord.z)
                {
                    case(0):
                        chunkID = _terrainGen.ChunkFromPosition(new Vector3(adjustedHitCoord.x, adjustedHitCoord.y, adjustedHitCoord.z-1f));
                        regChunk = _terrainGen.GetRegionChunk(chunkID);
                        blockCoordCopy.z = RegionChunk.chunkSizeZ-1;
                        regChunk.BlocksData[blockCoordCopy.x + 1][blockCoordCopy.y][blockCoordCopy.z + 1 + 1] =
                            collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1];
                        StartCoroutine(regChunk.CalculateDrawnMesh(blockCoordCopy.x / RenderChunk.xSize,
                            blockCoordCopy.y / RenderChunk.ySize, blockCoordCopy.z / RenderChunk.zSize));
                        break;
                    case(RegionChunk.chunkSizeZ-1):
                        chunkID= _terrainGen.ChunkFromPosition(new Vector3(adjustedHitCoord.x, adjustedHitCoord.y, adjustedHitCoord.z+1f));
                        regChunk = _terrainGen.GetRegionChunk(chunkID);
                        blockCoordCopy.z = 0;
                        regChunk.BlocksData[blockCoordCopy.x + 1][blockCoordCopy.y][blockCoordCopy.z + 1 - 1] =
                            collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1];
                        StartCoroutine(regChunk.CalculateDrawnMesh(blockCoordCopy.x / RenderChunk.xSize,
                            blockCoordCopy.y / RenderChunk.ySize, blockCoordCopy.z / RenderChunk.zSize));
                        break;
                }
            }
        }
    }

    Vector3Int WorldCoordToChunkBlockCoord(Vector3 worldCoord)
    {
        Vector3Int res = new Vector3Int();
        res.x = Mathf.RoundToInt(worldCoord.x) % RegionChunk.chunkSizeX;
        res.y = Mathf.RoundToInt(worldCoord.y);
        res.z = Mathf.RoundToInt(worldCoord.z) % RegionChunk.chunkSizeZ;

        if (res.x < 0)
        {
            res.x += 16;
        }

        if (res.z < 0)
        {
            res.z += 16;
        }
        
        return res;
    }
}
