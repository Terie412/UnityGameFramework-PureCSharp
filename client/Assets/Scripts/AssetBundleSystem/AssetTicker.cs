using System;

/// 为AssetBundle管理器提供一个Unity运行时生命周期的环境
public class AssetTicker : SingletonBehaviour<AssetTicker>
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public Action onUpdate;

    private void Update()
    {
        onUpdate?.Invoke();
    }
}