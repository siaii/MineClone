using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffectController : MonoBehaviour
{
    [SerializeField] private GameObject PostProcessingCollider;
    [SerializeField] private PlayerController _playerController;

    private TerrainGen _terrainGen;

    private void Start()
    {
        _terrainGen = FindObjectOfType<TerrainGen>();
    }

    private void Update()
    {
        Vector3 worldPos = transform.parent.TransformPoint(transform.localPosition);

        SetWaterTint(_terrainGen.BlockTypeFromPosition(worldPos)==BlockTypes.WATER);
    }

    public void SetWaterTint(bool isTinted)
    {
        PostProcessingCollider.SetActive(isTinted);
    }
}
