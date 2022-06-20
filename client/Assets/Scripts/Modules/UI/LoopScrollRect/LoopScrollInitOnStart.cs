using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LoopScrollRect))]
[DisallowMultipleComponent]
public class LoopScrollInitOnStart : MonoBehaviour, LoopScrollPrefabSource
{
    private string poolName;

    private void Awake()
    {
        var ls = GetComponent<LoopScrollRect>();
        ls.prefabSource = this;
        if (ls.content.childCount == 0)
        {
            Debug.LogError($"LoopScrollRect 无子物体");
            return;
        }

        var child = ls.content.GetChild(0);
        poolName = child.GetInstanceID().ToString();
        GameObjectPoolManager.Instance.InitNewPool(poolName, child.gameObject);
    }

    public GameObject GetObject(int index)
    {
        var go = GameObjectPoolManager.Instance.GetGameObjectFromPool(poolName);
        return go;
    }

    public void ReturnObject(Transform trans)
    {
        GameObjectPoolManager.Instance.ReturnGameObject(trans.gameObject);
    }
}