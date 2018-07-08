using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于统一MeshRenderer和SkinnedMeshRenderer的碰撞体设定用的
/// </summary>
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(Renderer))]
public class MeshBaker : MonoBehaviour {
    private MeshCollider mc;
    private Renderer renderer;
    private Mesh mesh;
    private bool isStatic;
	// Use this for initialization
	void Start ()
    {
        mc = GetComponent<MeshCollider>();
        renderer = GetComponent<Renderer>();
        mesh = new Mesh();
        isStatic = !(renderer.GetType().Equals(typeof(SkinnedMeshRenderer)));
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (isStatic == false && renderer != null)
        {
            ((SkinnedMeshRenderer)renderer).BakeMesh(mesh);
            mc.sharedMesh = mesh;
        }
	}
}
