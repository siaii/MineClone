using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Source : Block
{
    public override bool isTransparent => true;

    public override bool isDirectional => true;

    public override bool isFluid => true;
}
