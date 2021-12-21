using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryEquipment : InventoryItem
{
    public override bool IsPlaceable => false;

    public override bool IsStackable => false;

    private int curDurability;
}
