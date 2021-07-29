using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(InventoryView))]
public class PlayerInventory : MonoBehaviour
{
    private const int inventorySize = 28;
    private TexturePacker _texturePacker;
    private EventSystem _eventSystem;
    private InventoryView _inventoryView;
    private InventoryItemSlot _holdItem = new InventoryItemSlot();

    private InventoryItemSlot[] _inventoryItems = new InventoryItemSlot[inventorySize];
    private InventoryCreativeItemSlot[] _creativeItemSlots = new InventoryCreativeItemSlot[5];
    public InventoryItemSlot[] InventoryItems => _inventoryItems;

    [Range(0,6)] private int _activeItemIndex = 0;

    public int ActiveItemIndex => _activeItemIndex;

    [FormerlySerializedAs("_itemBar")] [SerializeField] private ItemBarSlot[] _itemBarSlots = new ItemBarSlot[7];
    
    [SerializeField] private InventoryViewItemSlot[] _inventoryViewItemSlots = new InventoryViewItemSlot[inventorySize];
    [SerializeField] private InventoryViewItemSlot[] _creativeViewItemSlots = new InventoryViewItemSlot[5];
    [SerializeField] private HoldItemView holdItemView;
    
    [SerializeField] private GraphicRaycaster canvasRaycaster;


    // Start is called before the first frame update
    void Start()
    {
        _texturePacker = FindObjectOfType<TexturePacker>();
        _eventSystem = FindObjectOfType<EventSystem>();
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

        _creativeItemSlots[0] = new InventoryCreativeItemSlot();
        _creativeItemSlots[1] = new InventoryCreativeItemSlot();
        _creativeItemSlots[2] = new InventoryCreativeItemSlot();
        _creativeItemSlots[3] = new InventoryCreativeItemSlot();
        _creativeItemSlots[4] = new InventoryCreativeItemSlot();
        
        for (int i = 0; i < _creativeItemSlots.Length; i++)
        {
            _creativeItemSlots[i].SetItemViewSlot(_creativeViewItemSlots[i]);
            _creativeViewItemSlots[i].SetSlotIndex(i);
        }
        
        _creativeItemSlots[0].itemContained = new GrassBlockItem();
        _creativeItemSlots[0].itemCount = 1;
        _creativeItemSlots[1].itemContained = new DirtBlockItem();
        _creativeItemSlots[1].itemCount = 1;
        _creativeItemSlots[2].itemContained = new StoneBlockItem();
        _creativeItemSlots[2].itemCount = 1;
        _creativeItemSlots[3].itemContained = new WoodBlockItem();
        _creativeItemSlots[3].itemCount = 1;
        _creativeItemSlots[4].itemContained = new LeafBlockItem();
        _creativeItemSlots[4].itemCount = 1;


        
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
        if (_inventoryView.IsInventoryActive)
        {
            int itemDestinationIdx = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                itemDestinationIdx = 0;
            }else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                itemDestinationIdx = 1;
            }else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                itemDestinationIdx = 2;
            }else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                itemDestinationIdx = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                itemDestinationIdx = 4;
            }else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                itemDestinationIdx = 5;
            }else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                itemDestinationIdx = 6;
            }

            if (itemDestinationIdx!=-1 && itemDestinationIdx<7)
            {
                PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
                pointerEventData.position = Input.mousePosition;

                List<RaycastResult> results = new List<RaycastResult>();
                
                canvasRaycaster.Raycast(pointerEventData, results);

                foreach (var res in results)
                {
                    if (res.gameObject.tag == "InvItemSlot")
                    {
                        int itemSlotIdx = res.gameObject.GetComponent<InventoryViewItemSlot>().InventoryItemSlotIndex;
                        if (_inventoryItems[itemSlotIdx].itemContained != null)
                        {
                            bool swap = false;
                            InventoryItem tempItem = new InventoryItem();
                            int tempCount = -1;
                            //Move or swap item to the idx
                            if (_inventoryItems[itemDestinationIdx].itemContained != null)
                            {
                                tempItem = _inventoryItems[itemDestinationIdx].itemContained;
                                tempCount = _inventoryItems[itemDestinationIdx].TakeItem(true);;
                                swap = true;
                            }

                            _inventoryItems[itemDestinationIdx].PutItem(_inventoryItems[itemSlotIdx].itemContained,
                                _inventoryItems[itemSlotIdx].TakeItem(true));

                            if (swap)
                            {
                                if(tempCount>0)
                                    _inventoryItems[itemSlotIdx].PutItem(tempItem, tempCount);
                            }
                            
                            _itemBarSlots[itemDestinationIdx].UpdateItemImage(_inventoryItems[itemDestinationIdx].itemContained,
                                _inventoryItems[itemDestinationIdx].itemCount);
                            
                            _itemBarSlots[itemSlotIdx].UpdateItemImage(_inventoryItems[itemSlotIdx].itemContained,
                                _inventoryItems[itemSlotIdx].itemCount);
                            
                            break;    
                        }
                    }
                }

                
            } 
        }
        else
        {
            int prevActive = _activeItemIndex;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _activeItemIndex = 0;
            }else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _activeItemIndex = 1;
            }else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _activeItemIndex = 2;
            }else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _activeItemIndex = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _activeItemIndex = 4;
            }else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _activeItemIndex = 5;
            }else if (Input.GetKeyDown(KeyCode.Alpha7))
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
    }

    public void InventoryInteract(PointerEventData.InputButton mouseClickButton, int itemSlotIndex, bool isCreative = false)
    {
        //Take item
        if (_holdItem.itemContained == null)
        {
            if ((_inventoryItems[itemSlotIndex].itemContained != null || isCreative) && !Input.GetKey(KeyCode.LeftShift))
            {
                if (isCreative)
                {
                    _holdItem.itemContained = _creativeItemSlots[itemSlotIndex].itemContained;                    
                }
                else
                {
                    _holdItem.itemContained = _inventoryItems[itemSlotIndex].itemContained;    
                }
            }

            int resCount = -1;
            switch (mouseClickButton)
            {
                case PointerEventData.InputButton.Left:
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                    {
                        int swapIdx = -1;
                        if (itemSlotIndex < 7)
                        {
                            for (int i = 7; i < inventorySize; i++)
                            {
                                //Maybe can put a case for when the active inventory is creative inventory
                                if (_inventoryItems[i].itemContained == null)
                                {
                                    swapIdx = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 7; i++)
                            {
                                if (_inventoryItems[i].itemContained == null)
                                {
                                    swapIdx = i;
                                    break;
                                }
                            }
                        }

                        if (swapIdx != -1)
                        {
                            _inventoryItems[swapIdx].PutItem(_inventoryItems[itemSlotIndex].itemContained,
                                _inventoryItems[itemSlotIndex].itemCount);
                                    
                            _inventoryItems[itemSlotIndex].TakeItem(true);
                        }

                        if (swapIdx < 7)
                        {
                            _itemBarSlots[swapIdx].UpdateItemImage(_inventoryItems[swapIdx].itemContained, _inventoryItems[swapIdx].itemCount);
                        }
                        else if (itemSlotIndex < 7)
                        {
                            _itemBarSlots[itemSlotIndex].UpdateItemImage(_inventoryItems[itemSlotIndex].itemContained, _inventoryItems[itemSlotIndex].itemCount);
                        }
                    }
                    else
                    {
                        if (isCreative)
                        {
                            resCount = _creativeItemSlots[itemSlotIndex].TakeItem(true);
                        }
                        else
                        {
                            resCount = _inventoryItems[itemSlotIndex].TakeItem(true);                            
                        }
                    }
                    break;
                }
                case PointerEventData.InputButton.Right:
                    if (isCreative)
                    {
                        resCount = _creativeItemSlots[itemSlotIndex].TakeItem(false);
                    }
                    else
                    {
                        resCount = _inventoryItems[itemSlotIndex].TakeItem(false);                        
                    }
                    break;
            }
            
            if (resCount > 0)
            {
                print(_holdItem.itemContained);
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
            if (_inventoryItems[itemSlotIndex].itemContained != null)
            {
                if(_inventoryItems[itemSlotIndex].itemContained!=_holdItem.itemContained) return;
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
                    excessCount += _holdItem.itemCount - 1;
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
