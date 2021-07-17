using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtBlockItem : InventoryItem
{
    public override bool IsPlaceable => true;
    public override BlockTypes PlacedBlock => BlockTypes.DIRT;
}
