using UnityEngine;

public class Test: MonoBehaviour
{
    public GameObject cube;
    public GameObject sphere;
    public GameObject capsule;

    private void Start()
    {
        Application.targetFrameRate = 30;
    }

    private void Update()
    {
        var v1 = sphere.transform.position - cube.transform.position;
        var v2 = capsule.transform.position - cube.transform.position;

        var v = Vector3.Cross(v1, v2);
        Debug.Log($"{v.y}");
    }
}