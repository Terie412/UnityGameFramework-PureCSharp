using System;
using UnityEngine;

public class NetTicker : MonoBehaviour
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