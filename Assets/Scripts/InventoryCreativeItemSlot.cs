using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryCreativeItemSlot : InventoryItemSlot
{
    public override int TakeItem(bool takeAll)
    {
        return Input.GetKeyDown(KeyCode.LeftShift) ? maxItemCount : 1;
    }

    public override int PutItem(InventoryItem item, int count)
    {
        return 0;
    }
}
