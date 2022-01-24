using QTC.Modules.UI;

public class GameMain : SingletonBehaviour<GameMain>
{
    private async void Start()
    {
        // Application.targetFrameRate = 30;

        GameLogger.Instance.Init();                                 // 日志系统初始化
        await AssetManager.Instance.Init();                         // 资产管理器初始化
        UIManager.Instance.InitAsync();                             // UI 管理器
        gameObject.GetOrAddComponent<NetTicker>();                  // 网络系统的初始化
        var ret = await NetClient.Instance.TryConnectToServer();    // 连接到服务器
        if (!ret)
        {
            GameLogger.Error("连接服务器失败");
            return;
        }

        Main(); // 主逻辑
    }

    private void Main()
    {
        // 登录服务器
        LoginManager.Instance.StartLogin();
    }
}