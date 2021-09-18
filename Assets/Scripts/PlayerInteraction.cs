using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private Camera _mainCamera;
    private Vector2 _cameraCenter;
    
    private TerrainGen _terrainGen;
    private PlayerInventory _playerInventory;

    private Vector3Int prevLookedBlock;
    private GameObject blockHighlight;

    private float delayTimer;
    
    private readonly Dictionary<Sides, Vector3Int> sideVector = new Dictionary<Sides, Vector3Int>()
    {
        {Sides.UP, Vector3Int.up},
        {Sides.DOWN, Vector3Int.down},
        {Sides.FRONT, Vector3Int.back},
        {Sides.BACK, Vector3Int.forward},
        {Sides.LEFT, Vector3Int.left},
        {Sides.RIGHT, Vector3Int.right}
    };

    [SerializeField] private float mouseInputDelay = 0.05f; //Delay between mouse input being processed
    [SerializeField] private float maxHitDistance = 5f;
    [SerializeField] private InventoryView _inventoryView;
    [SerializeField] private GameObject blockHighlightPrefab;

    // Start is called before the first frame update
    void Start()
    {
        _terrainGen = FindObjectOfType<TerrainGen>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
        _mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        _cameraCenter = new Vector2(_mainCamera.pixelWidth / 2, _mainCamera.pixelHeight / 2);
        delayTimer += Time.deltaTime;
        if (delayTimer >= mouseInputDelay)
        {
            ProcessMouseInput();
            //Reset delay timer only after block change
        }

        ProcessBlockHighlight();
    }

    void ProcessBlockHighlight()
    {
        RegionChunk collidedChunk = null;
        Vector3Int blockCoord = new Vector3Int();
        Vector3 adjustedHitCoord = new Vector3();
        RaycastHit hit;
        Ray ray = _mainCamera.ScreenPointToRay(_cameraCenter);
        var hitSomething = Physics.Raycast(ray, out hit);
        if (hitSomething && hit.distance < maxHitDistance)
        {
            //Make sure the coordinate to get the block is the correct block, thus the -0.5f
            adjustedHitCoord = hit.point + hit.normal * -0.5f;
            //Local within chunk (0-15) block coordinate
            blockCoord = new Vector3Int(Mathf.RoundToInt(adjustedHitCoord.x), Mathf.RoundToInt(adjustedHitCoord.y),
                Mathf.RoundToInt(adjustedHitCoord.z));

            if (blockHighlight == null)
            {
                blockHighlight = Instantiate(blockHighlightPrefab);
            }
            if (prevLookedBlock == null || blockCoord != prevLookedBlock)
            {
                blockHighlight.transform.position = blockCoord;
                blockHighlight.transform.rotation = Quaternion.identity;
                prevLookedBlock = blockCoord;
            } 
        }
        else
        {
            if (blockHighlight != null)
            {
                Destroy(blockHighlight);
            }
        }
    }

    void ProcessMouseInput()
    {
        bool blockChange = false;
        RegionChunk collidedChunk = null;
        Vector3Int blockCoord = new Vector3Int();
        Vector3 adjustedHitCoord = new Vector3();
        if ((Input.GetButton("Fire1") || Input.GetButton("Fire2")) && !_inventoryView.IsInventoryActive)
        {
            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(_cameraCenter);
            var hitSomething = Physics.Raycast(ray, out hit);
            if (hitSomething && hit.distance < maxHitDistance)
            {
                collidedChunk = hit.collider.transform.parent.GetComponent<RegionChunk>();

                if (Input.GetButton("Fire1"))
                {
                    //Make sure the coordinate to get the block is the correct block, thus the -0.5f
                    adjustedHitCoord = hit.point + hit.normal * -0.5f;
                    //Local within chunk (0-15) block coordinate
                    blockCoord = WorldCoordToChunkBlockCoord(adjustedHitCoord);
                    //Possibly keep the record of modified blocks in TerrainGen
                    collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1].BlockType = BlockTypes.AIR;
                }
                else
                {
                    if (_playerInventory.InventoryItems[_playerInventory.ActiveItemIndex].itemContained == null ||
                        _playerInventory.InventoryItems[_playerInventory.ActiveItemIndex].itemCount == 0)
                    {
                        return;
                    }
                    var placedBlockType = _playerInventory.InventoryItems[_playerInventory.ActiveItemIndex].itemContained.PlacedBlock;
                    if (placedBlockType == BlockTypes.NONE)
                    {
                        return;
                    }


                    Sides placedDirection = ConvertVectorToSide(hit.normal);
                    
                    //Make sure the coordinate to get the block is the correct block, thus the +0.5f
                    adjustedHitCoord = hit.point + hit.normal * 0.5f;
                    //Local within chunk (0-15) block coordinate
                    blockCoord = WorldCoordToChunkBlockCoord(adjustedHitCoord);
                    
                    
                    Vector3 castCenter = new Vector3(Mathf.RoundToInt(adjustedHitCoord.x), Mathf.RoundToInt(adjustedHitCoord.y),
                        Mathf.RoundToInt(adjustedHitCoord.z));
                    bool existsCollision = Physics.CheckBox(castCenter, new Vector3(0.48f, 0.48f, 0.48f));
                    if(existsCollision)
                        return;
                    
                    //Check if the block coord is in another chunk
                    if (blockCoord.x == 0 || blockCoord.x == RegionChunk.chunkSizeX - 1 || blockCoord.z == 0 ||
                        blockCoord.z == RegionChunk.chunkSizeZ - 1)
                    {
                        collidedChunk = _terrainGen.GetRegionChunk(TerrainGen.ChunkFromPosition(adjustedHitCoord));
                    }

                    //Possibly keep the record of modified blocks in TerrainGen
                    collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1].BlockType = placedBlockType;
                    collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1].BlockDirection =
                        placedDirection;
                }
                blockChange = true;
            }
        }

        if (blockChange)
        {
            //Calculate new mesh
            StartCoroutine(collidedChunk.CalculateDrawnMesh(blockCoord.x / RenderChunk.xSize,
                blockCoord.y / RenderChunk.ySize, blockCoord.z / RenderChunk.zSize));

            if (RegionChunk
                .blockTypesProperties[collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1]
                    .BlockType].isTransparent)
            {
                //Rerender the neighbouring render chunks
                switch (blockCoord.y % RenderChunk.ySize)
                {
                    case (0):
                        if (blockCoord.y / RenderChunk.ySize > 0)
                            StartCoroutine(collidedChunk.CalculateDrawnMesh(blockCoord.x / RenderChunk.xSize,
                                blockCoord.y / RenderChunk.ySize - 1, blockCoord.z / RenderChunk.zSize));
                        break;
                    case (RenderChunk.ySize - 1):
                        if (blockCoord.y / RenderChunk.ySize < RegionChunk.chunkSizeY / RenderChunk.ySize - 1)
                            StartCoroutine(collidedChunk.CalculateDrawnMesh(blockCoord.x / RenderChunk.xSize,
                                blockCoord.y / RenderChunk.ySize + 1, blockCoord.z / RenderChunk.zSize));
                        break;
                }

                //Update the render chunk of different region chunk
                _terrainGen.UpdateBorderingChunkData(collidedChunk, blockCoord, collidedChunk.BlocksData[blockCoord.x + 1][blockCoord.y][blockCoord.z + 1]);
            }

            //Queue block update at all bordering blocks
            var originalChunk = collidedChunk;
            foreach (var pair in sideVector)
            {
                collidedChunk = originalChunk;
                var newBlock = blockCoord + pair.Value;
                //If the block to add is not in the current chunk
                if (newBlock.x < 0 || newBlock.x + 1 > RegionChunk.chunkSizeX || newBlock.z < 0 ||
                    newBlock.z + 1 > RegionChunk.chunkSizeZ)
                {
                    TerrainGen _terrainGen = TerrainGen.instance;
                    var newChunkID = collidedChunk.chunkPos;
                    newChunkID.x += pair.Value.x;
                    newChunkID.y += pair.Value.z;

                    collidedChunk = _terrainGen.GetRegionChunk(newChunkID);
                    switch (pair.Key)
                    {
                        case Sides.RIGHT:
                            newBlock.x = 0;
                            break;
                        case Sides.FRONT:
                            newBlock.z = 15;
                            break;
                        case Sides.LEFT:
                            newBlock.x = 15;
                            break;
                        case Sides.BACK:
                            newBlock.z = 0;
                            break;
                        default:
                            newBlock.x = 0;
                            newBlock.z = 0;
                            Debug.LogError("Water update chunk border error");
                            break;
                    }
                }
                collidedChunk._chunkUpdater.updateNextTick.Add(newBlock);
            }
            delayTimer = 0;
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

    Sides ConvertVectorToSide(Vector3 vec)
    {
        if(vec.normalized == Vector3.up)
            return Sides.UP;
        if (vec.normalized == Vector3.right)
            return Sides.RIGHT;
        if (vec.normalized == Vector3.back)
            return Sides.FRONT;
        if (vec.normalized == Vector3.left)
            return Sides.LEFT;
        if (vec.normalized == Vector3.forward)
            return Sides.BACK;
        if (vec.normalized == Vector3.down)
            return Sides.DOWN;

        return Sides.UP;
    }
}
