using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneBlockItem : InventoryItem
{
    public override bool IsPlaceable => true;
    public override BlockTypes PlacedBlock => BlockTypes.STONE;
}
