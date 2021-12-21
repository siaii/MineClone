using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Block Property", menuName = "Scriptables/Block Property", order = 2)]
public class BlockProperty : ScriptableObject
{
    [Header("Assets")]
    public Texture2D BlockTexture;

    [Header("Fields")] 
    public string blockName;
    public bool isTransparent;
    public bool isDirectional;
    public bool isLeveled;
    public bool isFluid;
    public bool isDestroyable;
    public float baseDestroyTime;
    public ToolCategories ProperToolCategory;
}
