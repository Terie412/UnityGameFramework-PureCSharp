using System;
using Framework.ScreenAdapter;
using Modules.UI;
using UnityEngine;

public class GameMain : SingletonBehaviour<GameMain>
{
    public bool isServerEnable;
    [SerializeField] private int m_FrameRate = 30;
    public int frameRate
    {
        set {
            m_FrameRate = value;
            Application.targetFrameRate = m_FrameRate;
        }
    }
    
    private async void Start()
    {
        frameRate = 30;
        
        GameLogger.Instance.Init();                                 // 日志系统初始化
        await AssetManager.Instance.Init();                         // 资产管理器初始化
        ScreenAdapterManager.Instance.Init();
        await UIManager.Instance.InitAsync();                       // UI 管理器
        
        if (isServerEnable)
        {
            var ret = await NetClient.Instance.TryConnectToServer();    // 连接到服务器
            if (!ret)
            {
                GameLogger.LogError("连接服务器失败");
                return;
            }
        }

        Main(); // 主逻辑
    }

    private void Update()
    {
        if(AssetManager.Instance.isInit)
            AssetManager.Instance.Update();

        if (ScreenAdapterManager.Instance.isInit)
            ScreenAdapterManager.Instance.Update();
        
        if(UIManager.Instance.isInit)
            UIManager.Instance.Update();
        
        if(GameLogger.Instance.isInit)
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

        // 登录服务器
        // LoginManager.Instance.StartLogin();
    }
}