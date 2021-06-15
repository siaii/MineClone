using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class RenderChunk : MonoBehaviour
{
    public const int xSize = 16;
    public const int ySize = 16;
    public const int zSize = 16;

    private Mesh renderMesh;

    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshCollider _meshCollider;

    private void Start()
    {
        renderMesh = new Mesh();
    }

    public void BuildMesh(Vector3[] verts, int[] tris, Vector2[] uvs)
    {
        renderMesh = new Mesh();
        CreateMesh(verts, tris, uvs);
        
        _meshFilter.mesh = renderMesh;
        _meshCollider.sharedMesh = renderMesh;
    }

    void CreateMesh(Vector3[] verts, int[] tris, Vector2[] uvs)
    {
        renderMesh.vertices = verts;
        renderMesh.triangles = tris;
        renderMesh.uv = uvs;
        renderMesh.RecalculateNormals();
    }
}
