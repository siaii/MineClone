using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ItemBarSlot : MonoBehaviour
{
    [SerializeField] private Sprite inactiveItemBox;
    [SerializeField] private Sprite activeItemBox;
    [SerializeField] private Image containedItemImage;
    [SerializeField] private Text itemCountText;

    public Image ContainedItemImage => containedItemImage;

    private Image _image;
    private TexturePacker _texturePacker;

    // Start is called before the first frame update
    void Start()
    {
        _image = GetComponent<Image>();
        _texturePacker = FindObjectOfType<TexturePacker>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelected(bool isSelected)
    {
        _image.sprite = isSelected ? activeItemBox : inactiveItemBox;
    }
    
    public void UpdateItemImage(InventoryItem itemContained, int itemCount)
    {
        if (itemContained == null || itemCount == 0)
        {
            ContainedItemImage.color = new Color(0, 0, 0, 0);
            itemCountText.text = "";
            return;
        }
        var textureIdx = _texturePacker.textureDictIndex[itemContained.PlacedBlock];
        var textureUVRect = _texturePacker.blockTextureRects[textureIdx];

        textureUVRect.x *= _texturePacker.ResultTextureAtlas.width;
        textureUVRect.y *= _texturePacker.ResultTextureAtlas.height;
        textureUVRect.width *= _texturePacker.ResultTextureAtlas.width;
        textureUVRect.height *= _texturePacker.ResultTextureAtlas.height;

        ContainedItemImage.sprite = Sprite.Create((Texture2D)_texturePacker.ResultTextureAtlas,textureUVRect, Vector2.zero);
        ContainedItemImage.color = new Color(1, 1, 1, 1);

        
        itemCountText.text = itemCount == 1 ? "" : itemCount.ToString();
    }
}
