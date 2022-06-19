using System;
using QTC.Modules.UI;
using UnityEngine;

public class GameMain : SingletonBehaviour<GameMain>
{
    public bool isServerEnable = false;
    
    private async void Start()
    {
        Application.targetFrameRate = 30;

        GameLogger.Instance.Init();                                 // 日志系统初始化
        await AssetManager.Instance.Init();                         // 资产管理器初始化
        await ScreenAdapterManager.Instance.Init();                 // 屏幕适配管理器
        await UIManager.Instance.InitAsync();                       // UI 管理器
        
        if (isServerEnable)
        {
            gameObject.GetOrAddComponent<NetTicker>();                  // 网络系统的初始化
            var ret = await NetClient.Instance.TryConnectToServer();    // 连接到服务器
            if (!ret)
            {
                GameLogger.Error("连接服务器失败");
                return;
            }
        }

        Main(); // 主逻辑
    }

    private void Update()
    {
        if(AssetManager.Instance.isInit)
            AssetManager.Instance.Update();
        
        if(UIManager.Instance.isInit)
            UIManager.Instance.Update();
    }

    private void Main()
    {
        // 加载主UI
        // UIManager.Instance.OpenWindow("LoginWindow", null);

        // 登录服务器
        // LoginManager.Instance.StartLogin();
    }
}