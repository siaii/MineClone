using System.Collections.Generic;
using UnityEngine;

public abstract class Block
{
    public virtual bool isTransparent => false;
    public virtual bool isDirectional => false;
    public virtual bool isLeveled => false;

    public virtual bool isFluid => false;

    protected readonly Dictionary<Sides, Vector3[]> _verticesBase = new Dictionary<Sides, Vector3[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f)
            }
            
        }, 
        {
            Sides.DOWN, new []
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            }},

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
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f)
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector3(-0.5f, 0.5f, 0.5f),
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
                new Vector3(0.5f, 0.5f, 0.5f)
            }
        }
    };
    
    protected readonly Dictionary<Sides, Dictionary<Sides, Vector2[]>> directionConverted =    
        new Dictionary<Sides, Dictionary<Sides, Vector2[]>>()
        {
            {
                Sides.FRONT, new Dictionary<Sides, Vector2[]>()
                {
                    // {Sides.UP, Sides.BACK},
                    // {Sides.DOWN, Sides.FRONT},
                    // {Sides.FRONT, Sides.UP},
                    // {Sides.BACK, Sides.DOWN},
                    // {Sides.LEFT, Sides.LEFT}, //Rotate
                    // {Sides.RIGHT, Sides.RIGHT} //Rotate
                    {
                        Sides.UP, new []
                        {
                            new Vector2(0.995f, 3/8f),
                            new Vector2(0.995f, 5/8f),
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.75f,3/8f),
                        }}, 
                    {
                        Sides.DOWN, new []
                        {
                            
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0.25f,3/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.5f, 5/8f),
                        }},

                    {
                        Sides.FRONT, new[]
                        {
                            
                            new Vector2(0.25f, 7/8f),
                            new Vector2(0.25f,5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f, 7/8f),
                        }
                    },
                    {
                        Sides.BACK, new []
                        {
                            new Vector2(0.5f, 1/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f,1/8f),
                        }
                    },
                    {
                        Sides.LEFT, new []
                        {
                            new Vector2(0f,3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0f, 5/8f),
                        }
                    },
                    {
                        Sides.RIGHT, new []
                        {
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f,3/8f),
                            new Vector2(0.75f, 3/8f),
                        }
                    }
                }
            },
            {
                Sides.BACK, new Dictionary<Sides, Vector2[]>()
                {
                    // {Sides.UP, Sides.BACK},
                    // {Sides.DOWN, Sides.FRONT},
                    // {Sides.FRONT, Sides.UP},
                    // {Sides.BACK, Sides.DOWN},
                    // {Sides.LEFT, Sides.LEFT}, //Rotate
                    // {Sides.RIGHT, Sides.RIGHT} //Rotate
                    {
                        Sides.UP, new []
                        {
                            new Vector2(0.995f, 3/8f),
                            new Vector2(0.995f, 5/8f),
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.75f,3/8f),
                        }}, 
                    {
                        Sides.DOWN, new []
                        {
                            
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0.25f,3/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.5f, 5/8f),
                        }},

                    {
                        Sides.FRONT, new[]
                        {
                            
                            new Vector2(0.25f, 7/8f),
                            new Vector2(0.25f,5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f, 7/8f),
                        }
                    },
                    {
                        Sides.BACK, new []
                        {
                            new Vector2(0.5f, 1/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f,1/8f),
                        }
                    },
                    {
                        Sides.LEFT, new []
                        {
                            new Vector2(0f,3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0f, 5/8f),
                        }
                    },
                    {
                        Sides.RIGHT, new []
                        {
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f,3/8f),
                            new Vector2(0.75f, 3/8f),
                        }
                    }
                }
            },
            {
                Sides.UP, new Dictionary<Sides, Vector2[]>()
                {
                    // {Sides.UP, Sides.UP},
                    // {Sides.DOWN, Sides.DOWN},
                    // {Sides.FRONT, Sides.FRONT},
                    // {Sides.BACK, Sides.BACK},
                    // {Sides.LEFT, Sides.LEFT},
                    // {Sides.RIGHT, Sides.RIGHT}
                    {
                        Sides.UP, new []
                        {
                            new Vector2(0.25f, 7/8f),
                            new Vector2(0.25f,5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f, 7/8f),
                        }}, 
                    {
                        Sides.DOWN, new []
                        {
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f,1/8f),
                            new Vector2(0.5f, 1/8f),
                            new Vector2(0.5f, 3/8f),
                        }},

                    {
                        Sides.FRONT, new[]
                        {
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0.25f,3/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.5f, 5/8f),
                        }
                    },
                    {
                        Sides.BACK, new []
                        {
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.75f,3/8f),
                            new Vector2(0.995f, 3/8f),
                            new Vector2(0.995f, 5/8f),
                        }
                    },
                    {
                        Sides.LEFT, new []
                        {
                            new Vector2(0f, 5/8f),
                            new Vector2(0f,3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f, 5/8f),
                        }
                    },
                    {
                        Sides.RIGHT, new []
                        {
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f,3/8f),
                            new Vector2(0.75f, 3/8f),
                            new Vector2(0.75f, 5/8f),
                        }
                    }
                }
            },
            {
                Sides.DOWN, new Dictionary<Sides, Vector2[]>()
                {
                    // {Sides.UP, Sides.UP},
                    // {Sides.DOWN, Sides.DOWN},
                    // {Sides.FRONT, Sides.FRONT},
                    // {Sides.BACK, Sides.BACK},
                    // {Sides.LEFT, Sides.LEFT},
                    // {Sides.RIGHT, Sides.RIGHT}
                    {
                        Sides.UP, new []
                        {
                            new Vector2(0.25f, 7/8f),
                            new Vector2(0.25f,5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f, 7/8f),
                        }}, 
                    {
                        Sides.DOWN, new []
                        {
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f,1/8f),
                            new Vector2(0.5f, 1/8f),
                            new Vector2(0.5f, 3/8f),
                        }},

                    {
                        Sides.FRONT, new[]
                        {
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0.25f,3/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.5f, 5/8f),
                        }
                    },
                    {
                        Sides.BACK, new []
                        {
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.75f,3/8f),
                            new Vector2(0.995f, 3/8f),
                            new Vector2(0.995f, 5/8f),
                        }
                    },
                    {
                        Sides.LEFT, new []
                        {
                            new Vector2(0f, 5/8f),
                            new Vector2(0f,3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f, 5/8f),
                        }
                    },
                    {
                        Sides.RIGHT, new []
                        {
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f,3/8f),
                            new Vector2(0.75f, 3/8f),
                            new Vector2(0.75f, 5/8f),
                        }
                    }
                }
            },
            {
                Sides.LEFT, new Dictionary<Sides, Vector2[]>()
                {
                    // {Sides.UP, Sides.LEFT},
                    // {Sides.DOWN, Sides.RIGHT},
                    // {Sides.FRONT, Sides.FRONT}, //Rotate
                    // {Sides.BACK, Sides.BACK}, //Rotate
                    // {Sides.LEFT, Sides.DOWN},
                    // {Sides.RIGHT, Sides.UP}
                    {
                        Sides.UP, new []
                        {
                            new Vector2(0f,3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0f, 5/8f),
                        }}, 
                    {
                        Sides.DOWN, new []
                        {
                            new Vector2(0.5f,3/8f),
                            new Vector2(0.75f, 3/8f),
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.5f, 5/8f),
                        }},

                    {
                        Sides.FRONT, new[]
                        {
                            
                            new Vector2(0.25f,3/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.25f, 5/8f),
                        }
                    },
                    {
                        Sides.BACK, new []
                        {
                            new Vector2(0.995f, 5/8f),
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.75f,3/8f),
                            new Vector2(0.995f, 3/8f),
                        }
                    },
                    {
                        Sides.LEFT, new []
                        {
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f,1/8f),
                            new Vector2(0.5f, 1/8f),
                        }
                    },
                    {
                        Sides.RIGHT, new []
                        {
                            new Vector2(0.25f,5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f, 7/8f),
                            new Vector2(0.25f, 7/8f),
                        }
                    }
                }
            },
            {
                Sides.RIGHT, new Dictionary<Sides, Vector2[]>()
                {
                    // {Sides.UP, Sides.LEFT},
                    // {Sides.DOWN, Sides.RIGHT},
                    // {Sides.FRONT, Sides.FRONT}, //Rotate
                    // {Sides.BACK, Sides.BACK}, //Rotate
                    // {Sides.LEFT, Sides.DOWN},
                    // {Sides.RIGHT, Sides.UP}
                    {
                        Sides.UP, new []
                        {
                            new Vector2(0f,3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f, 5/8f),
                            new Vector2(0f, 5/8f),
                        }}, 
                    {
                        Sides.DOWN, new []
                        {
                            new Vector2(0.5f,3/8f),
                            new Vector2(0.75f, 3/8f),
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.5f, 5/8f),
                        }},

                    {
                        Sides.FRONT, new[]
                        {
                            
                            new Vector2(0.25f,3/8f),
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.25f, 5/8f),
                        }
                    },
                    {
                        Sides.BACK, new []
                        {
                            new Vector2(0.995f, 5/8f),
                            new Vector2(0.75f, 5/8f),
                            new Vector2(0.75f,3/8f),
                            new Vector2(0.995f, 3/8f),
                        }
                    },
                    {
                        Sides.LEFT, new []
                        {
                            new Vector2(0.5f, 3/8f),
                            new Vector2(0.25f, 3/8f),
                            new Vector2(0.25f,1/8f),
                            new Vector2(0.5f, 1/8f),
                        }
                    },
                    {
                        Sides.RIGHT, new []
                        {
                            new Vector2(0.25f,5/8f),
                            new Vector2(0.5f, 5/8f),
                            new Vector2(0.5f, 7/8f),
                            new Vector2(0.25f, 7/8f),
                        }
                    }
                }
            }
        };
    
    protected readonly Dictionary<Sides, Vector2[]> _uvMap = new Dictionary<Sides, Vector2[]>()
    {
        {
            Sides.UP, new []
            {
                new Vector2(0.25f, 7/8f),
                new Vector2(0.25f,5/8f),
                new Vector2(0.5f, 5/8f),
                new Vector2(0.5f, 7/8f),
            }}, 
        {
            Sides.DOWN, new []
            {
                new Vector2(0.25f, 3/8f),
                new Vector2(0.25f,1/8f),
                new Vector2(0.5f, 1/8f),
                new Vector2(0.5f, 3/8f),
            }},

        {
            Sides.FRONT, new[]
            {
                new Vector2(0.25f, 5/8f),
                new Vector2(0.25f,3/8f),
                new Vector2(0.5f, 3/8f),
                new Vector2(0.5f, 5/8f),
            }
        },
        {
            Sides.BACK, new []
            {
                new Vector2(0.75f, 5/8f),
                new Vector2(0.75f,3/8f),
                new Vector2(0.995f, 3/8f),
                new Vector2(0.995f, 5/8f),
            }
        },
        {
            Sides.LEFT, new []
            {
                new Vector2(0f, 5/8f),
                new Vector2(0f,3/8f),
                new Vector2(0.25f, 3/8f),
                new Vector2(0.25f, 5/8f),
            }
        },
        {
            Sides.RIGHT, new []
            {
                new Vector2(0.5f, 5/8f),
                new Vector2(0.5f,3/8f),
                new Vector2(0.75f, 3/8f),
                new Vector2(0.75f, 5/8f),
            }
        }
    };

    private readonly int[] _triangles =
    {
        0, 2, 1, 0, 3, 2
    };
    

    public virtual Vector3[] GetSideVertices(Sides reqSides, Vector3 blockPos)
    {
        Vector3[] res = (Vector3[])_verticesBase[reqSides].Clone();
        for(int i=0; i<res.Length; i++)
        {
            res[i] += blockPos;
        }
        return res;
    }
    
    public virtual Vector3[] GetSideVertices(Sides reqSides, Vector3 blockPos, Sides blockDirection)
    {
        Vector3[] res = (Vector3[])_verticesBase[reqSides].Clone();
        for(int i=0; i<res.Length; i++)
        {
            res[i] += blockPos;
        }
        return res;
    }
    
    public virtual Vector3[] GetSideVertices(Sides reqSides, Vector3 blockPos, Sides blockDirection, int level)
    {
        Vector3[] res = (Vector3[])_verticesBase[reqSides].Clone();
        for(int i=0; i<res.Length; i++)
        {
            res[i] += blockPos;
        }
        return res;
    }

    public virtual int[] GetSideTriangles(Sides reqSides)
    {
        return _triangles;
    }

    public virtual Vector2[] GetSideUVs(Sides reqSides, Sides blockDirection = Sides.UP)
    { 
        return directionConverted[blockDirection][reqSides];
    }
}
