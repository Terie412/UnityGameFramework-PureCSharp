using System;
using Core;
using Framework;
using UnityEngine;

public class GameMain : SingletonBehaviour<GameMain>
{
    [SerializeField] private int m_FrameRate = 30;

    public int frameRate
    {
        set
        {
            m_FrameRate                 = value;
            Application.targetFrameRate = m_FrameRate;
        }
    }

    private void Awake()
    {
        // 初始化所有的单例模式
        GameLogger.InitSingleTonOnLoad();
        AssetManager.InitSingleTonOnLoad();
        ScreenAdapterManager.InitSingleTonOnLoad();
        UIManager.InitSingleTonOnLoad();
        NetClient.InitSingleTonOnLoad();

        GC.Collect();
    }

    private async void Start()
    {
        frameRate = 30;

        GameLogger.Instance.Init();              // 日志系统初始化
        await AssetManager.Instance.InitAsync(); // 资产管理器初始化
        ScreenAdapterManager.Instance.Init();    // 屏幕适配
        await UIManager.Instance.InitAsync();    // UI 管理器

        Main(); // 主逻辑
    }

    private void Update()
    {
        if (AssetManager.Instance.isInit)
            AssetManager.Instance.Update();

        if (ScreenAdapterManager.Instance.isInit)
            ScreenAdapterManager.Instance.Update();

        if (UIManager.Instance.isInit)
            UIManager.Instance.Update();

        if (GameLogger.Instance.isInit)
            GameLogger.Instance.Update();

        NetClient.Instance.Update();
    }

    private void OnApplicationQuit()
    {
        if (GameLogger.Instance.isInit)
            GameLogger.Instance.OnApplicationQuit();

        NetClient.Instance.OnApplicationQuit();
    }

    private void Main()
    {
        // 加载主UI
        UIManager.Instance.OpenWindow("LoginWindow", null);
    }
}