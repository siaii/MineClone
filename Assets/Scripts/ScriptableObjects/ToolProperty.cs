using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Tool Property", menuName = "Scriptables/Tool Property", order = 3)]
public class ToolProperty : ScriptableObject
{
    [Header("Fields")] 
    public string toolName;
    public ToolTypes toolType;
    public int maxDurability;
    public float speedModifier;
    public float attackDamage;
}
