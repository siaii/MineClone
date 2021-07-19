using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemSlot
{
    private InventoryViewItemSlot itemViewSlot;
    private InventoryItem _itemContained;
    private int _itemCount;
    public InventoryItem itemContained
    {
        get => _itemContained;
        set => SetItemContained(value);
    }

    public int itemCount
    {
        get => _itemCount;
        set => SetItemCount(value);
    }

    private void SetItemContained(InventoryItem newItem)
    {
        _itemContained = newItem;
        itemViewSlot.UpdateItemImage(_itemContained, _itemCount);
    }

    private void SetItemCount(int newCount)
    {
        _itemCount = newCount;
        itemViewSlot.UpdateItemImage(_itemContained, _itemCount);
    }
    public void SetItemViewSlot(InventoryViewItemSlot correspondingItemViewSlot)
    {
        itemViewSlot = correspondingItemViewSlot;
    }
}
