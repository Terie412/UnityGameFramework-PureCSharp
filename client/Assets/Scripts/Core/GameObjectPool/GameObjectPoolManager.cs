using UnityEngine;
using System.Collections.Generic;
using Framework;

public class GameObjectPoolManager : SingletonBehaviour<GameObjectPoolManager>
{
    private Dictionary<string, GameObjectPool> poolName_pool = new();

    /// <summary>
    /// 初始化一个新的对象缓存池
    /// </summary>
    /// <param name="poolName">缓存池的名称</param>
    /// <param name="go"></param>
    /// <param name="size">缓存池的大小</param>
    /// <param name="type">缓存池自动扩张大小的方式</param>
    public void InitNewPool(string poolName, GameObject go, int size = 5, PoolInflationType type = PoolInflationType.DOUBLE)
    {
        if (string.IsNullOrEmpty(poolName) || go == null)
        {
            Debug.LogError($"InitNewPool failed! poolName = {poolName}, go = {go}");
            return;
        }

        if (poolName_pool.ContainsKey(poolName))
        {
            Debug.LogWarning($"InitNewPool failed for pool:{poolName} already exists");
            return;
        }

        poolName_pool[poolName] = new GameObjectPool(poolName, go, transform, size, type);
    }

    public void DestroyPool(string poolName)
    {
        if (poolName == null)
        {
            return;
        }

        if (!poolName_pool.TryGetValue(poolName, out var pool)) return;
        pool.Destroy();
        poolName_pool.Remove(poolName);
    }

    public GameObject GetGameObjectFromPool(string poolName)
    {
        if (string.IsNullOrEmpty(poolName) || !poolName_pool.ContainsKey(poolName))
        {
            Debug.LogError($"GetObjectFromPool failed! poolName = {poolName}");
            return null;
        }

        var pool = poolName_pool[poolName];
        return pool.GetGameObject();
    }

    public void ReturnGameObject(GameObject go)
    {
        if (go == null)
        {
            return;
        }
        
        PoolObject poolObj = go.GetComponent<PoolObject>();
        if (poolObj == null)
        {
            Debug.LogWarning("Specified object is not a pooled instance: " + go.name);
            return;
        }

        if (!poolName_pool.TryGetValue(poolObj.poolName, out var pool))
        {
            Debug.LogWarning("No valid Pool with poolName: " + poolObj.poolName);
            return;
        }

        pool.ReturnObjectToPool(poolObj);
    }
}