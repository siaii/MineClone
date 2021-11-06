using System.Collections.Generic;
using UnityEngine;

public class TexturePacker : MonoBehaviour
{
    public Dictionary<BlockTypes, int> textureDictIndex;

    public Rect[] blockTextureRects;
    
    [SerializeField] public BlockTexturePair[] textureDict;

    [SerializeField] private Material renderChunkMaterial;

    public Texture ResultTextureAtlas { get; private set; }

    void Awake()
    {
        
    }

    void Update()
    {
    }

    public void packTextures(BlockPropertyPair[] textureDict)
    {
        Texture2D resTexture = new Texture2D(1024,1024);
        List<Texture2D> texturesToPack = new List<Texture2D>();

        foreach (var t in textureDict)
        {
            texturesToPack.Add(t.BlockProperty.BlockTexture);
        }
        blockTextureRects = resTexture.PackTextures(texturesToPack.ToArray(), 2);
        resTexture.filterMode = FilterMode.Point;
        resTexture.wrapMode = TextureWrapMode.Clamp;
        renderChunkMaterial.SetTexture("_BaseMap", resTexture);
        ResultTextureAtlas = resTexture;
    }

    public void generateTextureDictIdx(BlockPropertyPair[] textureDict)
    {
        textureDictIndex = new Dictionary<BlockTypes, int>();

        for (int i = 0; i < textureDict.Length; i++)
        {
            textureDictIndex.Add(textureDict[i].BlockType, i);
        }
    } 
}
