using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Air : Block
{
    public override bool isTransparent => true;

    private readonly Dictionary<Sides, Vector3[]> _verticesBase = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new Vector3[]
                { }
        }, 
        {
            Sides.DOWN, new Vector3[]
                { }
        },
        {
            Sides.FRONT, new Vector3[]
                { }
        },
        {
            Sides.BACK, new Vector3[]
            { }
        },
        {
            Sides.LEFT, new Vector3[]
            { }
        },
        {
            Sides.RIGHT, new Vector3[]
            { }
        }
    };
    
    private readonly int[] _triangles =
    {};

    private readonly Dictionary<Sides, Vector2[]> _uvMap = new Dictionary<Sides, Vector2[]>()
    {
        {
            Sides.UP, new Vector2[]
                { }
        }, 
        {
            Sides.DOWN, new Vector2[]
                { }
        },
        {
            Sides.FRONT, new Vector2[]
                { }
        },
        {
            Sides.BACK, new Vector2[]
                { }
        },
        {
            Sides.LEFT, new Vector2[]
                { }
        },
        {
            Sides.RIGHT, new Vector2[]
                { }
        }
    };
    
    public override Vector3[] GetSideVertices(Sides reqSides, Vector3 blockPos)
    {
        Vector3[] res = (Vector3[])_verticesBase[reqSides].Clone();
        for(int i=0; i<res.Length; i++)
        {
            res[i] += blockPos;
        }
        return res;
    }
    
    public override int[] GetSideTriangles()
    {
        return _triangles;
    }
    
    public override Vector2[] GetSideUVs(Sides reqSides)
    {
        return _uvMap[reqSides];
    }
}
