using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Source : Block
{
    public override bool isTransparent => true;

    private readonly int[] _insideTriangle =
    {
        0, 2, 1, 0, 3, 2
    };

    public override int[] GetSideTriangles(Sides reqSides)
    {
        return _insideTriangle;
    }
}
