using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int inventorySize = 21;
    private TexturePacker _texturePacker;

    private InventoryItem[] _inventoryItems;

    public InventoryItem[] InventoryItems => _inventoryItems;

    [Range(0,6)] private int _activeItemIndex = 0;

    public int ActiveItemIndex => _activeItemIndex;

    [SerializeField] private ItemBoxUI[] _itemBar = new ItemBoxUI[7];

    [SerializeField] private Texture2D temp;
    // Start is called before the first frame update
    void Start()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
        _inventoryItems = new InventoryItem[inventorySize];
        _inventoryItems[0] = new GrassBlockItem();
        _inventoryItems[1] = new DirtBlockItem();
        _inventoryItems[2] = new StoneBlockItem();
        _inventoryItems[3] = new WoodBlockItem();
        _inventoryItems[4] = new LeafBlockItem();

        for (int i = 0; i < _itemBar.Length; i++)
        {
            if (_inventoryItems[i] == null)
            {
                continue;
            }
            var textureIdx = _texturePacker.textureDictIndex[_inventoryItems[i].PlacedBlock];
            var textureUVRect = _texturePacker.blockTextureRects[textureIdx];

            textureUVRect.x *= _texturePacker.ResultTextureAtlas.width;
            textureUVRect.y *= _texturePacker.ResultTextureAtlas.height;
            textureUVRect.width *= _texturePacker.ResultTextureAtlas.width;
            textureUVRect.height *= _texturePacker.ResultTextureAtlas.height;
            _itemBar[i].ContainedItemImage.sprite = Sprite.Create((Texture2D)_texturePacker.ResultTextureAtlas,textureUVRect, Vector2.zero);
            _itemBar[i].ContainedItemImage.color = new Color(1,1,1,1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            _activeItemIndex += 1;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            _activeItemIndex -= 1;
        }

        if (_activeItemIndex > 6)
        {
            _activeItemIndex = 0;
        }

        if (_activeItemIndex < 0)
        {
            _activeItemIndex = 6;
        }

        for (int i = 0; i < _itemBar.Length; i++)
        {
            _itemBar[i].SetSelected(i==_activeItemIndex);
        }
    }
}
