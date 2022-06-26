using System;
using UnityEngine;

public class NetTicker : SingletonBehaviour<NetTicker>
{
    public Action onUpdate;
    public Action onApplicationQuit;
    
    private void Update()
    {
        onUpdate?.Invoke();
    }

    private void OnApplicationQuit()
    {
        onApplicationQuit?.Invoke();
    }
}