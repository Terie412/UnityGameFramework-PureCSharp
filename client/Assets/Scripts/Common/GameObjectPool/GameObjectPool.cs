using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用来标记一个 GameObject 从属于哪一个对象缓存池
/// </summary>
[DisallowMultipleComponent]
public class PoolObject : MonoBehaviour
{
    public string poolName;
    [HideInInspector] public bool isPooled;
}

public enum PoolInflationType
{
    /// When a dynamic pool inflates, add one to the pool.
    INCREMENT,

    /// When a dynamic pool inflates, double the size of the pool
    DOUBLE
}

class GameObjectPool
{
    private string poolName;
    private Stack<PoolObject> poolObjectStack = new();
    private GameObject root;
    private string sourceName;
    private PoolInflationType inflationType;
    private int objectsInUse;

    public GameObjectPool(string poolName, GameObject source, Transform poolParent, int initialCount, PoolInflationType type)
    {
        if (string.IsNullOrEmpty(poolName) || source == null || poolParent == null)
        {
            return;
        }

        this.poolName = poolName;
        inflationType = type;
        root = new GameObject(poolName + "Pool");
        root.transform.SetParent(poolParent, false);

        sourceName = source.name;
        PoolObject poolObj = source.GetComponent<PoolObject>();
        if (poolObj == null)
        {
            poolObj = source.AddComponent<PoolObject>();
        }

        poolObj.poolName = poolName;
        AddObjectToPool(poolObj);
        PopulatePool(Mathf.Max(initialCount - 1, 0));
    }

    private void AddObjectToPool(PoolObject poolObj)
    {
        var gameObject = poolObj.gameObject;
        gameObject.SetActive(false);
        gameObject.name = sourceName;
        poolObjectStack.Push(poolObj);
        poolObj.isPooled = true;
        poolObj.gameObject.transform.SetParent(root.transform, false);
    }

    private void PopulatePool(int initialCount)
    {
        for (int index = 0; index < initialCount; index++)
        {
            PoolObject po = Object.Instantiate(poolObjectStack.Peek());
            AddObjectToPool(po);
        }
    }

    public GameObject GetGameObject()
    {
        PoolObject poolObj;
        if (poolObjectStack.Count > 1)
        {
            poolObj = poolObjectStack.Pop();
        }
        else
        {
            int increaseSize = inflationType switch
            {
                PoolInflationType.INCREMENT => 1,
                PoolInflationType.DOUBLE => Mathf.Max(objectsInUse, 1),
                _ => 1
            };

            PopulatePool(increaseSize);
            poolObj = poolObjectStack.Pop();
        }

        // 对于通过外部手段销毁 PoolObject 后，当前的 Stack 的Count不会改变，但是Pop出来的是 null
        if (poolObj == null)
        {
            return null;
        }

        objectsInUse++;
        poolObj.isPooled = false;
        GameObject result = poolObj.gameObject;
        result.SetActive(true);
        return result;
    }

    public void ReturnObjectToPool(PoolObject po)
    {
        if (!poolName.Equals(po.poolName) || po.isPooled) return;
        
        objectsInUse--;
        AddObjectToPool(po);
    }

    public void Destroy()
    {
        Object.Destroy(root);
        poolObjectStack.Clear();
        poolObjectStack = null;
        root = null;
    }
}