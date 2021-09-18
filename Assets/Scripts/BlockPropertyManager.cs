using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockPropertyManager
{
    public static readonly Dictionary<BlockTypes, Block> blockProperties = new Dictionary<BlockTypes, Block>()
    {
        {BlockTypes.GRASS, new Grass()},
        {BlockTypes.AIR, new Air()},
        {BlockTypes.DIRT, new Dirt()},
        {BlockTypes.STONE, new Stone()},
        {BlockTypes.WOOD, new Wood()},
        {BlockTypes.LEAF, new Leaf()},
        {BlockTypes.WATER_SOURCE, new Water_Source()},
        {BlockTypes.WATER_FLOWING, new Water_Flowing()}
    };
}
