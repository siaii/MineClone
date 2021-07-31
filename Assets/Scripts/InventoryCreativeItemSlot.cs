using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryCreativeItemSlot : InventoryItemSlot
{
    public override int TakeItem(bool takeAll)
    {
        return takeAll ? maxItemCount : 1;
    }

    public override int PutItem(InventoryItem item, int count)
    {
        return 0;
    }
}
