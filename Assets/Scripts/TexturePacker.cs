using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TexturePacker : MonoBehaviour
{
    public Dictionary<BlockTypes, int> textureDictIndex;

    public Rect[] blockTextureRects;
    
    [SerializeField] public BlockTexturePair[] textureDict;

    [SerializeField] private Material renderChunkMaterial;

    [SerializeField] private Image uiImage;
    
    void Awake()
    {
        packTextures();
        generateTextureDictIdx();
    }

    void packTextures()
    {
        Texture2D resTexture = new Texture2D(1024,1024);
        List<Texture2D> texturesToPack = new List<Texture2D>();

        for (int i = 0; i < textureDict.Length; i++)
        {
            texturesToPack.Add(textureDict[i].blockTexture);
        }
        blockTextureRects = resTexture.PackTextures(texturesToPack.ToArray(), 2);
        resTexture.filterMode = FilterMode.Point;
        resTexture.wrapMode = TextureWrapMode.Clamp;
        resTexture.Apply();
        renderChunkMaterial.mainTexture = resTexture;
    }

    void generateTextureDictIdx()
    {
        textureDictIndex = new Dictionary<BlockTypes, int>();

        for (int i = 0; i < textureDict.Length; i++)
        {
            textureDictIndex.Add(textureDict[i].blockType, i);
        }
    } 
}
