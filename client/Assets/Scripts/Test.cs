using UnityEngine;
using UnityEngine.UI;

public class Test: MonoBehaviour
{
    public GameObject go;
    public int totalCount;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            var ls = go.GetComponent<LoopScrollRectBase>();
            ls.totalCount = totalCount;
            ls.onItemBuild.AddListener((obj, index) =>
            {
                obj.transform.GetChild(0).GetComponent<Text>().text = index.ToString();
            });
            ls.RefillCells();
        }
    }
}