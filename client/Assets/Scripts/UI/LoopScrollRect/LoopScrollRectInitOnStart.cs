using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public class LoopScrollRectInitOnStart : MonoBehaviour, LoopScrollPrefabSource, LoopScrollDataSource
{
    public GameObject item;
    public int totalCount = -1;
    private string poolName = "";

    public GameObject GetObject(int index)
    {
        var go = GameObjectPoolManager.Instance.GetGameObjectFromPool(poolName);
        go.SetActive(true);
        return go;
    }

    public void ReturnObject(Transform trans)
    {
        trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
        trans.gameObject.SetActive(false);
        trans.SetParent(transform, false);
        GameObjectPoolManager.Instance.ReturnGameObject(trans.gameObject);
    }

    public void ProvideData(Transform transform, int idx)
    {
        transform.SendMessage("ScrollCellIndex", idx, SendMessageOptions.DontRequireReceiver);
    }

    void Start()
    {
        if (item == null)
        {
            return;
        }
        
        poolName = item.GetInstanceID().ToString();
        GameObjectPoolManager.Instance.InitNewPool(poolName, item);
        
        var ls = GetComponent<LoopScrollRect>();
        ls.prefabSource = this;
        ls.dataSource = this;
        ls.totalCount = totalCount;
        ls.RefillCells();
    }
}