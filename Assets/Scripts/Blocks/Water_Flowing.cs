using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Water_Flowing : Water_Source
{
    public override bool isTransparent => true;
    public override bool isDirectional => true;
    public override bool isLeveled => true;

    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight4 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.25f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, 0.25f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.25f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.25f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight3 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.0f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight2 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, -0.25f, 0.5f),
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, -0.25f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, -0.25f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.25f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, -0.25f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.25f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight1 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.25f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.25f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            }
        }
    };

    public override Vector3[] GetSideVertices(Sides reqSides, Vector3 blockPos, Sides blockDirection, int level)
    {
        Dictionary<Sides, Vector3[]> verticesDict;
        float rotationAmount = 0;
        //Get vertices according to the fluid level
        if (blockDirection != Sides.DOWN)
        {
            switch (level)
            {
                case 1:
                    verticesDict = _verticesHeight1;
                    break;
                case 2:
                    verticesDict = _verticesHeight2;
                    break;
                case 3: 
                    verticesDict = _verticesHeight3;
                    break;
                case 4:
                    verticesDict = _verticesHeight4;
                    break;
                default:
                    verticesDict = _verticesBase;
                    break;
            }    
        }
        else
        {
            verticesDict = _verticesBase;
        }
        

        switch (blockDirection)
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
        Vector3[] res = (Vector3[])verticesDict[reqSides].Clone();
        //Rotate according to the direction
        var rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
        for(int i=0; i<res.Length; i++)
        {
            var vert = res[i];
            vert = rotation * vert;
            res[i] = vert;
        }
        
        for(int i=0; i<res.Length; i++)
        {
            res[i] += blockPos;
        }
        return res;
        
    }

    public override bool BlockUpdate(RegionChunk regChunk, Vector3Int blockPos)
    {
        bool change = base.BlockUpdate(regChunk, blockPos);
        RegionChunk originalChunk = regChunk;
        BlockData curBlockData = regChunk.BlocksData[blockPos.x + 1][blockPos.y][blockPos.z + 1];
        Dictionary<Sides, Vector3Int> checkDict = 
            new Dictionary<Sides, Vector3Int>()
            {
                {Sides.UP, new Vector3Int(0, 1, 0)},
                {Sides.RIGHT, new Vector3Int(1, 0, 0)},
                {Sides.FRONT, new Vector3Int(0, 0, -1)},
                {Sides.LEFT, new Vector3Int(-1, 0, 0)},
                {Sides.BACK, new Vector3Int(0, 0, 1)}
            };

        int maxSurroundingLevel = 0;
        Sides maxDirection = Sides.DOWN;
        Sides maxSubDirection = Sides.DOWN;
        foreach (var pair in checkDict)
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

            BlockData checkData = regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1];
            if (checkData.BlockType == BlockTypes.WATER_SOURCE || checkData.BlockType == BlockTypes.WATER_FLOWING)
            {
                int level = checkData.BlockType == BlockTypes.WATER_SOURCE || checkData.BlockDirection == Sides.DOWN ? 5 : checkData.Level;
                if (level > maxSurroundingLevel)
                {
                    maxDirection = Side.ReverseHorizontalSide(pair.Key);
                    maxSubDirection = Side.ReverseHorizontalSide(pair.Key);
                    regChunk._chunkUpdater.AddToUpdateNextTick(checkBlock);
                }
                else if (level == maxSurroundingLevel)
                {
                    maxSubDirection = Side.ReverseHorizontalSide(pair.Key);
                    regChunk._chunkUpdater.AddToUpdateNextTick(checkBlock);
                }
                
                maxSurroundingLevel = math.max(maxSurroundingLevel, level);
            }
        }

        if (maxSurroundingLevel - 1 < curBlockData.Level)
        {
            curBlockData.Level--;
            if (curBlockData.Level == 0)
            {
                curBlockData.BlockType = BlockTypes.AIR;
                curBlockData.BlockDirection = maxDirection;
                curBlockData.SubDirection = maxSubDirection;
            }
            change = true;
        }

        if (change)
        {
            regChunk._chunkUpdater.AddToUpdateNextTick(blockPos);
        }

        return change;
    }

    //Dont flow to other flowing water with higher level
    protected override bool ExistingFlowWater(RegionChunk regChunk, Vector3Int checkBlock, KeyValuePair<Sides, Vector3Int> pair, BlockData curBlockData)
    {
        if (curBlockData.Level - 1 < regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level)
        {
            return false;
        }
        //else
        if (curBlockData.Level - 1 == regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level)
        {
            if (regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockDirection != pair.Key)
            {
                if (regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].SubDirection ==
                    Side.ReverseHorizontalSide(pair.Key))
                {
                    regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].SubDirection =
                        Side.ReverseHorizontalSide(regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1]
                            .BlockDirection);
                }
                else
                {
                    regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].SubDirection = pair.Key;
                }
            }
                
        }
        else if (curBlockData.Level - 1 > regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level)
        {
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].SubDirection = pair.Key; 
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].BlockDirection = pair.Key;
            regChunk.BlocksData[checkBlock.x + 1][checkBlock.y][checkBlock.z + 1].Level = curBlockData.Level - 1;
        }

        
        return true;

    }
}
