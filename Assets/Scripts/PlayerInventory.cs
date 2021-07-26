using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[RequireComponent(typeof(InventoryView))]
public class PlayerInventory : MonoBehaviour
{
    private const int inventorySize = 28;
    private TexturePacker _texturePacker;
    private InventoryView _inventoryView;
    private InventoryItemSlot _holdItem = new InventoryItemSlot();

    private InventoryItemSlot[] _inventoryItems = new InventoryItemSlot[inventorySize];

    public InventoryItemSlot[] InventoryItems => _inventoryItems;

    [Range(0,6)] private int _activeItemIndex = 0;

    public int ActiveItemIndex => _activeItemIndex;

    [FormerlySerializedAs("_itemBar")] [SerializeField] private ItemBarSlot[] _itemBarSlots = new ItemBarSlot[7];

    [SerializeField] private InventoryViewItemSlot[] _inventoryViewItemSlots = new InventoryViewItemSlot[inventorySize];
    [SerializeField] private HoldItemView holdItemView;
    // Start is called before the first frame update
    void Start()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
        _inventoryView = GetComponent<InventoryView>();
        _holdItem.SetItemViewSlot(holdItemView);
        _holdItem.itemCount = 0; //Set default value to clear the placeholder value
        for (int i = 0; i < _inventoryItems.Length; i++)
        {
            _inventoryItems[i] = new InventoryItemSlot();
            _inventoryItems[i].SetItemViewSlot(_inventoryViewItemSlots[i]);
            _inventoryViewItemSlots[i].SetSlotIndex(i);
            _inventoryItems[i].itemCount = 0;
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
            _itemBarSlots[i].UpdateItemImage(_inventoryItems[i].itemContained, _inventoryItems[i].itemCount);
        }
        _itemBarSlots[ActiveItemIndex].SetSelected(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_holdItem.itemContained != null)
        {
            holdItemView.transform.position = Input.mousePosition;
        }
        ProcessScrollwheel();
        ProcessNumberButton();
    }

    private void ProcessScrollwheel()
    {
        //If Mouse scroll wheel is scrolled
        if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > Mathf.Epsilon && !_inventoryView.IsInventoryActive)
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
                _itemBarSlots[i].SetSelected(i==ActiveItemIndex);
            }
        }
    }

    private void ProcessNumberButton()
    {
        int prevActive = _activeItemIndex;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            _activeItemIndex = 0;
        }else if (Input.GetKey(KeyCode.Alpha2))
        {
            _activeItemIndex = 1;
        }else if (Input.GetKey(KeyCode.Alpha3))
        {
            _activeItemIndex = 2;
        }else if (Input.GetKey(KeyCode.Alpha4))
        {
            _activeItemIndex = 3;
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            _activeItemIndex = 4;
        }else if (Input.GetKey(KeyCode.Alpha6))
        {
            _activeItemIndex = 5;
        }else if (Input.GetKey(KeyCode.Alpha7))
        {
            _activeItemIndex = 6;
        }

        if (_activeItemIndex != prevActive)
        {
            for (int i = 0; i < _itemBarSlots.Length; i++)
            {
                _itemBarSlots[i].SetSelected(i==ActiveItemIndex);
            }
        }
    }

    public void InventoryInteract(PointerEventData.InputButton mouseClickButton, int itemSlotIndex)
    {
        //Take item
        if (_holdItem.itemContained == null)
        {
            if (_inventoryItems[itemSlotIndex].itemContained != null)
            {
                _holdItem.itemContained = _inventoryItems[itemSlotIndex].itemContained;
            }

            int resCount = -1;
            switch (mouseClickButton)
            {
                case PointerEventData.InputButton.Left:
                    resCount = _inventoryItems[itemSlotIndex].TakeItem(true);
                    break;
                case PointerEventData.InputButton.Right:
                    resCount = _inventoryItems[itemSlotIndex].TakeItem(false);
                    break;
                default:
                    _holdItem.itemContained = null;
                    return;
            }
            
            if (resCount > 0)
            {
                _holdItem.itemCount = resCount;
            }
            else
            {
                _holdItem.itemContained = null;
            }
        }
        //Put item
        else
        {
            print(itemSlotIndex);
            print(_inventoryItems[itemSlotIndex].itemContained == null);
            if (_inventoryItems[itemSlotIndex].itemContained != null)
            {
                return;
            }

            int excessCount = 0;
            switch (mouseClickButton)
            {
                case PointerEventData.InputButton.Left:
                {
                    //Try to put all
                    excessCount = _inventoryItems[itemSlotIndex].PutItem(_holdItem.itemContained, _holdItem.itemCount);
                    break;
                }
                case PointerEventData.InputButton.Right:
                {
                    //Try to put 1
                    excessCount = _inventoryItems[itemSlotIndex].PutItem(_holdItem.itemContained, 1);
                    break;
                }
                default:
                    return;
            }
            
            _holdItem.itemCount = excessCount;
                    
            if (_holdItem.itemCount == 0)
            {
                _holdItem.itemContained = null;
            }
            //Update image (?)
        }

        if (itemSlotIndex < 7)
        {
            _itemBarSlots[itemSlotIndex].UpdateItemImage(_inventoryItems[itemSlotIndex].itemContained, _inventoryItems[itemSlotIndex].itemCount);
        }
    }
}
