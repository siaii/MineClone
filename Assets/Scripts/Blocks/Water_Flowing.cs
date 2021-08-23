using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Flowing : Block
{
    public override bool isTransparent => true;
    public override bool isDirectional => true;
    public override bool isLeveled => true;

    public override bool isFluid => true;

    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight4 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.25f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, 0.25f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.25f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.25f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight3 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, 0.25f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.25f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight2 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, -0.25f, 0.5f),
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, -0.25f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, -0.25f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.25f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, -0.25f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.25f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Vector3[]> _verticesHeight1 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            }
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }
        },
        {
            Sides.FRONT, new[]
            {
                new Vector3(-0.5f, -0.25f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.25f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.25f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, -0.25f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f)
            }
        }
    };

    public override Vector3[] GetSideVertices(Sides reqSides, Vector3 blockPos, Sides blockDirection, int level)
    {
        Dictionary<Sides, Vector3[]> verticesDict;
        float rotationAmount = 0;
        //Get vertices according to the fluid level
        if (blockDirection != Sides.DOWN)
        {
            switch (level)
            {
                case 1:
                    verticesDict = _verticesHeight1;
                    break;
                case 2:
                    verticesDict = _verticesHeight2;
                    break;
                case 3: 
                    verticesDict = _verticesHeight3;
                    break;
                case 4:
                    verticesDict = _verticesHeight4;
                    break;
                default:
                    verticesDict = _verticesBase;
                    break;
            }    
        }
        else
        {
            verticesDict = _verticesBase;
        }
        

        switch (blockDirection)
        {
            case Sides.BACK:
                rotationAmount = 0;
                break;
            case Sides.RIGHT:
                rotationAmount = 90;
                break;
            case Sides.FRONT:
                rotationAmount = 180;
                break;
            case Sides.LEFT:
                rotationAmount = 270;
                break;
        }
        Vector3[] res = (Vector3[])verticesDict[reqSides].Clone();
        //Rotate according to the direction
        var rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
        for(int i=0; i<res.Length; i++)
        {
            var vert = res[i];
            vert = rotation * vert;
            res[i] = vert;
        }
        
        for(int i=0; i<res.Length; i++)
        {
            res[i] += blockPos;
        }
        return res;
        
    }
}
