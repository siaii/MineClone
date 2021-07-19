using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInventory : MonoBehaviour
{
    private const int inventorySize = 28;
    private TexturePacker _texturePacker;

    private InventoryItemSlot[] _inventoryItems = new InventoryItemSlot[inventorySize];

    public InventoryItemSlot[] InventoryItems => _inventoryItems;

    [Range(0,6)] private int _activeItemIndex = 0;

    public int ActiveItemIndex => _activeItemIndex;

    [FormerlySerializedAs("_itemBar")] [SerializeField] private ItemBarSlot[] _itemBarSlots = new ItemBarSlot[7];

    [SerializeField] private InventoryViewItemSlot[] _inventoryViewItemSlots = new InventoryViewItemSlot[inventorySize];
    // Start is called before the first frame update
    void Start()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
        for (int i = 0; i < _inventoryItems.Length; i++)
        {
            _inventoryItems[i] = new InventoryItemSlot();
            _inventoryItems[i].SetItemViewSlot(_inventoryViewItemSlots[i]);
        }
        _inventoryItems[0].itemContained = new GrassBlockItem();
        _inventoryItems[0].itemCount = 1;
        _inventoryItems[1].itemContained = new DirtBlockItem();
        _inventoryItems[1].itemCount = 1;
        _inventoryItems[2].itemContained = new StoneBlockItem();
        _inventoryItems[2].itemCount = 1;
        _inventoryItems[3].itemContained = new WoodBlockItem();
        _inventoryItems[3].itemCount = 1;
        _inventoryItems[4].itemContained = new LeafBlockItem();
        _inventoryItems[4].itemCount = 1;
        for (int i = 0; i < _itemBarSlots.Length; i++)
        {
            if (_inventoryItems[i].itemContained == null)
            {
                continue;
            }
            var textureIdx = _texturePacker.textureDictIndex[_inventoryItems[i].itemContained.PlacedBlock];
            var textureUVRect = _texturePacker.blockTextureRects[textureIdx];

            textureUVRect.x *= _texturePacker.ResultTextureAtlas.width;
            textureUVRect.y *= _texturePacker.ResultTextureAtlas.height;
            textureUVRect.width *= _texturePacker.ResultTextureAtlas.width;
            textureUVRect.height *= _texturePacker.ResultTextureAtlas.height;
            _itemBarSlots[i].ContainedItemImage.sprite = Sprite.Create((Texture2D)_texturePacker.ResultTextureAtlas,textureUVRect, Vector2.zero);
            _itemBarSlots[i].ContainedItemImage.color = new Color(1,1,1,1);
        }
        _itemBarSlots[ActiveItemIndex].SetSelected(true);
    }

    // Update is called once per frame
    void Update()
    {
        //If Mouse scroll wheel is scrolled
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > Mathf.Epsilon)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                _activeItemIndex -= 1;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                _activeItemIndex += 1;
            }

            //Loop back if overflowed
            if (_activeItemIndex > 6)
            {
                _activeItemIndex = 0;
            }

            if (_activeItemIndex < 0)
            {
                _activeItemIndex = 6;
            }

            for (int i = 0; i < _itemBarSlots.Length; i++)
            {
                _itemBarSlots[i].SetSelected(i==_activeItemIndex);
            }
        }
    }
}
