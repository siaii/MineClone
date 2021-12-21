using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPropertyManager : MonoBehaviour
{
    public static BlockPropertyManager Instance;
    //Separate block property and block method (?)
    public BlockPropertyPair[] blockProperty;
    public readonly Dictionary<BlockTypes, Block> blockClass = new Dictionary<BlockTypes, Block>()
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
    
    [SerializeField] private TexturePacker _texturePacker;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (_texturePacker == null)
        {
            Debug.LogError("Texture packer not assigned");
            if(!TryGetComponent(out _texturePacker))
                return;
        }
        
        _texturePacker.packTextures(blockProperty);
        _texturePacker.generateTextureDictIdx(blockProperty);
    }

    private void Start()
    {
        AssignBlockProperties();
    }

    private void AssignBlockProperties()
    {
        foreach (var propertyPair in blockProperty)
        {
            blockClass[propertyPair.BlockType].isTransparent = propertyPair.BlockProperty.isTransparent;
            blockClass[propertyPair.BlockType].isDirectional = propertyPair.BlockProperty.isDirectional;
            blockClass[propertyPair.BlockType].isLeveled = propertyPair.BlockProperty.isLeveled;
            blockClass[propertyPair.BlockType].isFluid = propertyPair.BlockProperty.isFluid;
            blockClass[propertyPair.BlockType].isDestroyable = propertyPair.BlockProperty.isDestroyable;
            //TODO split to base and tool
            blockClass[propertyPair.BlockType].destroyTime = propertyPair.BlockProperty.baseDestroyTime;
        }
    }
}
