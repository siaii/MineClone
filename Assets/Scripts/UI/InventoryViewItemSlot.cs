using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewItemSlot : MonoBehaviour
{
    [SerializeField] private GameObject itemImageObject;
    [SerializeField] private GameObject itemCountObject;
    
    private TexturePacker _texturePacker;
    // Start is called before the first frame update
    void Start()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateItemImage(InventoryItem itemContained, int itemCount)
    {
        var imageComponent = itemImageObject.GetComponent<Image>();
        if (itemContained == null || itemCount == 0)
        {
            imageComponent.color = new Color(0, 0, 0, 0);
            return;
        }
        var textureIdx = _texturePacker.textureDictIndex[itemContained.PlacedBlock];
        var textureUVRect = _texturePacker.blockTextureRects[textureIdx];

        textureUVRect.x *= _texturePacker.ResultTextureAtlas.width;
        textureUVRect.y *= _texturePacker.ResultTextureAtlas.height;
        textureUVRect.width *= _texturePacker.ResultTextureAtlas.width;
        textureUVRect.height *= _texturePacker.ResultTextureAtlas.height;

        imageComponent.sprite = Sprite.Create((Texture2D)_texturePacker.ResultTextureAtlas,textureUVRect, Vector2.zero);
        imageComponent.color = new Color(1, 1, 1, 1);
    }
}
