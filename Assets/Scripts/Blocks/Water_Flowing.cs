using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Flowing : Block
{
    public override bool isTransparent => true;

    public override bool isDirectional => true;

    private readonly Dictionary<Sides, Vector3[]> _verticesHeight4 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.375f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.375f, 0.5f)
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
                new Vector3(0.5f, 0.375f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.375f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.375f, 0.5f),
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
                new Vector3(0.5f, 0.375f, 0.5f)
            }
        }
    };
    
    private readonly Dictionary<Sides, Vector3[]> _verticesHeight3 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, 0.375f, -0.5f),
                new Vector3(0.5f, 0.375f, -0.5f),
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
                new Vector3(-0.5f, 0.375f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.375f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, 0.375f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.375f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.25f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.375f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0.375f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.25f, 0.5f)
            }
        }
    };
    
    private readonly Dictionary<Sides, Vector3[]> _verticesHeight2 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.125f, 0.5f),
                new Vector3(-0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.25f, -0.5f),
                new Vector3(0.5f, 0.125f, 0.5f)
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
                new Vector3(0.5f, 0.125f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.125f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.125f, 0.5f),
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
                new Vector3(0.5f, 0.125f, 0.5f)
            }
        }
    };
    
    private readonly Dictionary<Sides, Vector3[]> _verticesHeight1 = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.0f, 0.5f),
                new Vector3(-0.5f, 0.125f, -0.5f),
                new Vector3(0.5f, 0.125f, -0.5f),
                new Vector3(0.5f, 0.0f, 0.5f)
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
                new Vector3(-0.5f, 0.125f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, 0.125f, -0.5f)
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector3(0.5f, 0.0f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.0f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.0f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.125f, -0.5f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector3(0.5f, 0.125f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.0f, 0.5f)
            }
        }
    };
}
