using System.Collections;
using System.Collections.Generic;

public class InventoryItemSlot
{
    protected InterfaceItemView itemViewSlot;
    private InventoryItem _itemContained;
    private int _itemCount = 0;
    public const int maxItemCount = 64;
    public virtual InventoryItem itemContained
    {
        get => _itemContained;
        set => SetItemContained(value);
    }

    public virtual int itemCount
    {
        get => _itemCount;
        set => SetItemCount(value);
    }
    
    private void SetItemContained(InventoryItem newItem)
    {
        _itemContained = newItem;
        itemViewSlot.UpdateItemImage(itemContained, itemCount);
    }

    private void SetItemCount(int newCount)
    {
        _itemCount = newCount;
        itemViewSlot.UpdateItemImage(itemContained, itemCount);
    }
    public void SetItemViewSlot(InterfaceItemView correspondingItemViewSlot)
    {
        itemViewSlot = correspondingItemViewSlot;
    }

    public virtual int TakeItem(bool takeAll)
    {
        if (itemContained == null || itemCount == 0)
        {
            return -1;
        }
        int takeAmount;
        if (takeAll)
        {
            takeAmount = itemCount;
            itemCount = 0;
            itemContained = null;
        }
        else
        {
            takeAmount = itemCount / 2;
            itemCount -= takeAmount;
        }
        itemViewSlot.UpdateItemImage(itemContained, itemCount);
        return takeAmount;
    }

    public virtual int PutItem(InventoryItem item, int count)
    {
        // if (item != itemContained || itemCount >= maxItemCount)
        // {
        //     return -1;
        // }

        if (itemContained == null)
        {
            itemContained = item;
            itemCount = count;
            itemViewSlot.UpdateItemImage(itemContained, itemCount);
            return 0;
        }
        
        int excess = count;
        if (itemContained == item)
        {
            itemCount += count;
        
            if (itemCount > maxItemCount) 
            { 
                excess = itemCount - maxItemCount; 
                itemCount = maxItemCount;
            }
            else
            {
                excess = 0;
            }
        
            itemViewSlot.UpdateItemImage(itemContained, itemCount); 
        }
        return excess;
    }
}
