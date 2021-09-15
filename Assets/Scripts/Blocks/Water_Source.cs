using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Source : Block
{
    public override bool isTransparent => true;

    public override bool isFluid => true;

    public override bool isLeveled => false;

    public override bool BlockUpdate(RegionChunk regChunk, Vector3Int blockPos)
    {
        bool change = false;
        RegionChunk originalChunk = regChunk;
        BlockData curBlockData = regChunk.BlocksData[blockPos.x + 1][blockPos.y][blockPos.z + 1];
        Dictionary<Sides, Vector3Int> updateDict = 
            new Dictionary<Sides, Vector3Int>()
            {
                {Sides.DOWN, new Vector3Int(0, -1, 0)},
                {Sides.RIGHT, new Vector3Int(1, 0, 0)},
                {Sides.FRONT, new Vector3Int(0, 0, -1)},
                {Sides.LEFT, new Vector3Int(-1, 0, 0)},
                {Sides.BACK, new Vector3Int(0, 0, 1)}
            };
        //Check blocks in each direction the water can spread
        foreach (var pair in updateDict)
        {
            regChunk = originalChunk;
            var checkBlock = blockPos + pair.Value;
            if (checkBlock.y < 0)
            {
                //If new block is in y<0 don't process
                continue;
            }
            
            //If the block to add is not in the current chunk
            if (checkBlock.x < 0 || checkBlock.x >= RegionChunk.chunkSizeX || checkBlock.z < 0 ||
                checkBlock.z >= RegionChunk.chunkSizeZ)
            {
                TerrainGen _terrainGen = TerrainGen.instance;
                var newChunkID = regChunk.chunkPos;
                newChunkID.x += pair.Value.x;
                newChunkID.y += pair.Value.z;

                //TODO chunk not found error
                regChunk = _terrainGen.GetRegionChunk(newChunkID);
                switch (pair.Key)
                {
                    case Sides.RIGHT:
                        checkBlock.x = 0;
                        break;
                    case Sides.FRONT:
                        checkBlock.z = 15;
                        break;
                    case Sides.LEFT:
                        checkBlock.x = 15;
                        break;
                    case Sides.BACK:
                        checkBlock.z = 0;
                        break;
                    default:
                        checkBlock.x = 0;
                        checkBlock.z = 0;
                        Debug.LogError("Water update chunk border error");
                        break;
                }
            }
            
            change = SpreadWater(curBlockData, regChunk, checkBlock, change, pair);
            if (change)
            {
                regChunk._chunkUpdater.updateNextTick.Enqueue(checkBlock);
                regChunk._chunkUpdater.renderChunkToReDraw.Add(new Vector3Int(checkBlock.x / RenderChunk.xSize,
                    checkBlock.y / RenderChunk.ySize, checkBlock.z / RenderChunk.zSize));
                TerrainGen.instance.UpdateBorderingChunkData(regChunk, checkBlock,
                    regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1]);
                if(pair.Key==Sides.DOWN)
                    break;
            }
        }

        return change;
    }

    private bool SpreadWater(BlockData curBlockData, RegionChunk regChunk, Vector3Int checkBlock, bool change,
        KeyValuePair<Sides, Vector3Int> pair)
    {
        // var curBlockData = regChunk.BlocksData[blockPos.x + 1][blockPos.y][blockPos.z + 1];
        if (regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockType == BlockTypes.AIR)
        {
            NewFlowWater(regChunk, checkBlock, pair, curBlockData);
            change = true;
        }
        else if (regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockType ==
                 BlockTypes.WATER_FLOWING)
        {
            change = ExistingFlowWater(regChunk, checkBlock, pair, curBlockData);
        }

        return change;
    }

    protected virtual bool ExistingFlowWater(RegionChunk regChunk, Vector3Int checkBlock, KeyValuePair<Sides, Vector3Int> pair, BlockData curBlockData)
    {
        //If level 4 then make infinite water source
        if (regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level == 4 
            && regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockDirection != pair.Key)
        {
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockType = BlockTypes.WATER_SOURCE;
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockDirection = pair.Key;
        }
        else
        {
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level = 4;
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockDirection = pair.Key;
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].SubDirection = pair.Key;
        }
        return true;
    }

    private void NewFlowWater(RegionChunk regChunk, Vector3Int checkBlock, KeyValuePair<Sides, Vector3Int> pair, BlockData curBlockData)
    {
        var curLevel = curBlockData.BlockType == BlockTypes.WATER_SOURCE ? 5 : curBlockData.Level;
        if (curLevel > 1 || pair.Key == Sides.DOWN)
        {
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockType =
                BlockTypes.WATER_FLOWING;
            
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockDirection = pair.Key;
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].SubDirection = pair.Key;

            if (curBlockData.BlockDirection == Sides.DOWN)
            {
                regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level = 4;
            }  
            else
            {
                regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level = pair.Key == Sides.DOWN ? 4 : curLevel - 1;
            }
        }
    }
}
