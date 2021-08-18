using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData
{
    private BlockTypes _blockTypes = BlockTypes.AIR;
    private Sides _upDirection = Sides.UP;
    private int _level = 1;
    public BlockTypes BlockType
    {
        get => _blockTypes;
        set => _blockTypes = value;
    }

    public Sides UpDirection
    {
        get => _upDirection;
        set => _upDirection = value;
    }

    public int Level
    {
        get => _level;
        set => _level = value;
    }
}
