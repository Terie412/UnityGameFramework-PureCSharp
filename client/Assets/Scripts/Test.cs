using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Mesh mesh;
    public Renderer renderer;
    public Collider collider;
    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        renderer = GetComponent<Renderer>();
        collider = GetComponent<Collider>();
    }

    private void OnDrawGizmos()
    {
        if (mesh == null || renderer == null || collider == null)
        {
            return;
        }

        var bounds = collider.bounds;
        var center = bounds.center;
        var extents = bounds.extents;

        var v1 = center + new Vector3(extents.x, extents.y, extents.z);
        var v2 = center + new Vector3(-extents.x, extents.y, extents.z);
        var v3 = center + new Vector3(-extents.x, extents.y, -extents.z);
        var v4 = center + new Vector3(extents.x, extents.y, -extents.z);
        var v5 = center + new Vector3(extents.x, -extents.y, extents.z);
        var v6 = center + new Vector3(-extents.x, -extents.y, extents.z);
        var v7 = center + new Vector3(-extents.x, -extents.y, -extents.z);
        var v8 = center + new Vector3(extents.x, -extents.y, -extents.z);
        
        // Handles.DrawLine(v1, v2);
        // Handles.DrawLine(v2, v3);
        // Handles.DrawLine(v3, v4);
        // Handles.DrawLine(v1, v4);
        // Handles.DrawLine(v5, v6);
        // Handles.DrawLine(v6, v7);
        // Handles.DrawLine(v7, v8);
        // Handles.DrawLine(v5, v5);
        // Handles.DrawLine(v1, v5);
        // Handles.DrawLine(v2, v6);
        // Handles.DrawLine(v3, v7);
        // Handles.DrawLine(v4, v8);
        
        Gizmos.DrawLine(v1, v2);
        Gizmos.DrawLine(v2, v3);
        Gizmos.DrawLine(v3, v4);
        Gizmos.DrawLine(v1, v4);
        Gizmos.DrawLine(v5, v6);
        Gizmos.DrawLine(v6, v7);
        Gizmos.DrawLine(v7, v8);
        Gizmos.DrawLine(v5, v5);
        Gizmos.DrawLine(v1, v5);
        Gizmos.DrawLine(v2, v6);
        Gizmos.DrawLine(v3, v7);
        Gizmos.DrawLine(v4, v8);
    }
}
