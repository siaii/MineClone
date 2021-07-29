using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class InterfaceItemView : MonoBehaviour
{
    [SerializeField] protected GameObject itemImageObject;
    [SerializeField] protected GameObject itemCountObject;

    private int _inventoryItemSlotIndex = -1;

    public int InventoryItemSlotIndex
    {
        get => _inventoryItemSlotIndex;
        set => _inventoryItemSlotIndex = value;
    }
    
    private TexturePacker _texturePacker;

    // Start is called before the first frame update
    void Start()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
    }

    public void UpdateItemImage(InventoryItem itemContained, int itemCount)
    {
        if (!_texturePacker)
        {
            _texturePacker = FindObjectOfType<TexturePacker>();
        }
        var imageComponent = itemImageObject.GetComponent<Image>();
        if (itemContained == null || itemCount == 0)
        {
            imageComponent.color = new Color(0, 0, 0, 0);
            itemCountObject.GetComponent<Text>().text = "";
            return;
        }
        
        //TODO Change the item textures both here and item bar
        var textureIdx = _texturePacker.textureDictIndex[itemContained.PlacedBlock];
        var textureUVRect = _texturePacker.blockTextureRects[textureIdx];

        textureUVRect.x *= _texturePacker.ResultTextureAtlas.width;
        textureUVRect.y *= _texturePacker.ResultTextureAtlas.height;
        textureUVRect.width *= _texturePacker.ResultTextureAtlas.width;
        textureUVRect.height *= _texturePacker.ResultTextureAtlas.height;

        imageComponent.sprite = Sprite.Create((Texture2D)_texturePacker.ResultTextureAtlas,textureUVRect, Vector2.zero);
        imageComponent.color = new Color(1, 1, 1, 1);

        
        itemCountObject.GetComponent<Text>().text = itemCount == 1 ? "" : itemCount.ToString();
    }

    public void SetSlotIndex(int idx)
    {
        InventoryItemSlotIndex = idx;
    }
}
