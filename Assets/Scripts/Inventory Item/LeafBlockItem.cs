using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafBlockItem : InventoryItem
{
    public override bool IsPlaceable => true;
    public override BlockTypes PlacedBlock => BlockTypes.LEAF;
}
