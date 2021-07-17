using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public virtual bool IsPlaceable => true;

    public virtual BlockTypes PlacedBlock => BlockTypes.NONE;
    public virtual void RightClickFunction(){}
}
