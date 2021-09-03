using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData
{
    private BlockTypes _blockTypes = BlockTypes.AIR;
    private Sides _blockDirection = Sides.UP;
    private Sides _blockSubDirection = Sides.UP;
    private Sides _sourceDirection = Sides.UP;
    private Sides _sourceSubDirection = Sides.UP;
    private int _level = 1;
    public BlockTypes BlockType
    {
        get => _blockTypes;
        set => _blockTypes = value;
    }

    public Sides BlockDirection
    {
        get => _blockDirection;
        set => _blockDirection = value;
    }

    public int Level
    {
        get => _level;
        set => _level = value;
    }

    public Sides SubDirection
    {
        get => _blockSubDirection;
        set => _blockSubDirection = value;
    }

    public Sides SourceDirection
    {
        get => _sourceDirection;
        set => _sourceDirection = value;
    }
    public Sides SourceSubDirection
    {
        get => _sourceSubDirection;
        set => _sourceSubDirection = value;
    }
}
