using System.Collections.Generic;
using UnityEngine;

public class TestCoordTranslation : MonoBehaviour
{
    public Matrix4x4 M;
    public Matrix4x4 V;
    public Matrix4x4 P;

    public Camera cam;
    
    private void Start()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                return;
            }
        }
        
        Application.targetFrameRate = 30;
        M = transform.localToWorldMatrix;
        V = cam.worldToCameraMatrix;
        P = cam.projectionMatrix;
        

        Debug.Log($"M = \n{M}");
        Debug.Log($"V = \n{V}");
        Debug.Log($"P = \n{P}");

        GetMatrix_V();
        GetMatrix_P_Orthographic();
        GetMatrix_P_Perspective();
    }

    private Matrix4x4 GetMatrix_P_Perspective()
    {
        float n = cam.nearClipPlane;
        float f = cam.farClipPlane;
        float aspect = cam.aspect;
        float fov = cam.fieldOfView * Mathf.Deg2Rad;
        float size = n * Mathf.Tan(fov / 2);
        Debug.Log($"n = {n}, size = {size}, aspect = {aspect}");
        
        Matrix4x4 mt = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, (f + n) / 2, 1)
        );

        Matrix4x4 mr = new Matrix4x4(
            new Vector4(size * aspect, 0, 0, 0),
            new Vector4(0, size, 0, 0),
            new Vector4(0, 0, (f-n) / 2, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 mp2o = new Matrix4x4(
            new Vector4(n, 0, 0, 0),
            new Vector4(0, n, 0, 0),
            new Vector4(0, 0, f + n, 1),
            new Vector4(0, 0, -n * f, 0)
        );

        Matrix4x4 m_z_reflect = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 mp = mr.inverse * mt.inverse * mp2o * m_z_reflect;
        Debug.Log($"mp2o = \n{mp2o}");
        Debug.Log($"mp_persp = \n{mp}");

        Vector4 testPoint = new Vector4(4, 4, 4, 1);
        Matrix4x4 testMatrix = new Matrix4x4(
            new Vector4(20, 242, 222, 1),
            Vector4.zero,
            Vector4.zero,
            Vector4.zero
        );
        Matrix4x4 v1 = mp * testMatrix;
        Matrix4x4 v2 = P * testMatrix;

        Debug.Log($"v1=\n{v1}");
        Debug.Log($"v2=\n{v2}");
        
        Debug.Log($"v1=\n{mp.MultiplyPoint(testPoint)}");
        Debug.Log($"v1=\n{P.MultiplyPoint(testPoint)}");

        return mp;
    }
    
    private void GetMatrix_P_Orthographic()
    {
        float n = cam.nearClipPlane;
        float f = cam.farClipPlane;
        float aspect = cam.aspect;
        float size = cam.orthographicSize;

        Matrix4x4 mt = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, (f + n) / 2, 1)
        );

        Matrix4x4 mr = new Matrix4x4(
            new Vector4(size * aspect, 0, 0, 0),
            new Vector4(0, size, 0, 0),
            new Vector4(0, 0, (f - n) / 2, 0),
            new Vector4(0, 0, 0, 1)
        );
        
        Matrix4x4 m_z_reflect = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 mp = mr.inverse * mt.inverse * m_z_reflect;
        Debug.Log($"mp_ortho = \n{mp}");
    }

    private void GetMatrix_V()
    {
        float x = cam.transform.eulerAngles.x * Mathf.Deg2Rad;
        float y = cam.transform.eulerAngles.y * Mathf.Deg2Rad;
        float z = cam.transform.eulerAngles.z * Mathf.Deg2Rad;
        Matrix4x4 ry = new Matrix4x4(
            new Vector4(Mathf.Cos(y), 0, -Mathf.Sin(y), 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(Mathf.Sin(y), 0, Mathf.Cos(y), 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 rx = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, Mathf.Cos(x), Mathf.Sin(x), 0),
            new Vector4(0, -Mathf.Sin(x), Mathf.Cos(x), 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 rz = new Matrix4x4(
            new Vector4(Mathf.Cos(z), Mathf.Sin(z), 0, 0),
            new Vector4(-Mathf.Sin(z), Mathf.Cos(z), 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 mr = ry * rx * rz;
        Matrix4x4 mt = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z, 1)
        );
        Matrix4x4 m_z_reflect = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );
        
        Matrix4x4 mv = m_z_reflect * mr.inverse * mt.inverse;
        Debug.Log($"mv = \n{mv}");
    }

    private void GetMatrix_M()
    {
        float x = 30 * Mathf.Deg2Rad ;
        float y = 45 * Mathf.Deg2Rad;
        float z = 60 * Mathf.Deg2Rad;
        Matrix4x4 ry = new Matrix4x4(
            new Vector4(Mathf.Cos(y), 0, -Mathf.Sin(y), 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(Mathf.Sin(y), 0, Mathf.Cos(y), 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 rx = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, Mathf.Cos(x), Mathf.Sin(x), 0),
            new Vector4(0, -Mathf.Sin(x), Mathf.Cos(x), 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 rz = new Matrix4x4(
            new Vector4(Mathf.Cos(z), Mathf.Sin(z), 0, 0),
            new Vector4(-Mathf.Sin(z), Mathf.Cos(z), 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1)
        );

        Debug.Log($"rx = \n{rx}");
        Debug.Log($"ry = \n{ry}");
        Debug.Log($"rz = \n{rz}");

        Debug.Log($"ry * rx * rz = \n{ry * rx * rz}");
        Debug.Log($"rz * rx * ry = \n{rz * rx * ry}");
    }

    private float lastTime = 0;
    private void Update()
    {
        // if (Time.realtimeSinceStartup - lastTime > 0.3f)
        // {
        //     M = transform.localToWorldMatrix;
        //     V = Camera.main.worldToCameraMatrix;
        //     P = GetMatrix_P_Perspective();
        //     
        //     lastTime = Time.realtimeSinceStartup;
        //     GenCube();
        // }
    }

    void GenCube()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = {
            new(-0.5f, -0.5f, -0.5f),
            new(0.5f, -0.5f, -0.5f),
            new(-0.5f, -0.5f, 0.5f),
            new(0.5f, -0.5f, 0.5f),
            
            new(-0.5f, 0.5f, -0.5f),
            new(0.5f, 0.5f, -0.5f),
            new(-0.5f, 0.5f, 0.5f),
            new(0.5f, 0.5f, 0.5f)
        };
        
        List<Vector3> projectionVertices = new();
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 localVertex = vertices[i];
            Vector4 worldVertex = M * new Vector4(localVertex.x, localVertex.y, localVertex.z, 1);
            Vector4 viewVertex = V * worldVertex;
            Vector4 projectionVertex = P * viewVertex;
            
            var x = projectionVertex.x / projectionVertex.w;
            var y = projectionVertex.y / projectionVertex.w;
            var z = projectionVertex.z / projectionVertex.w;
            Vector3 v = new Vector3(x, y, z);
            
            Debug.Log($"vertex {localVertex} -> {worldVertex} -> {projectionVertex} -> {v}");

            
            projectionVertices.Add(v);
        }

        Vector3[] vs2 = projectionVertices.ToArray();
        mesh.vertices = vs2;
        mesh.triangles = new[]
        {
            2, 0, 3,
            1, 3, 0,
            4, 6, 5,
            7, 5, 6,
            0, 4, 1,
            5, 1, 4,
            1, 5, 3,
            7, 3, 5,
            3, 7, 2,
            6, 2, 7,
            2, 6, 0,
            4, 0, 6,
        };

        GetComponent<MeshFilter>().mesh = mesh;
    }
}