using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameProtocol;
using KCPNet;
using Newtonsoft.Json;
using QTC.Modules.UI;
using UnityEngine;

public class GameMain : SingletonBehaviour<GameMain>
{
	private IEnumerator Start()
	{
		// Application.targetFrameRate = 30;
		
		// 日志系统初始化
		GameLogger.Instance.Init();
		
		// 资产管理器初始化
		yield return AssetManager.Instance.Init();

		// UI 管理器
		UIManager.Instance.InitAsync();
		// UIManager.Instance.Init();
		
		// // 网络系统的初始化
		// KCPNetLogger.onInfo = (str, _) => { Debug.Log(str); };
		// KCPNetLogger.onWarning = (str, _) => { Debug.LogWarning(str); };
		// KCPNetLogger.onError = (str, _) => { Debug.LogError(str); };
		// gameObject.GetOrAddComponent<NetTicker>();
		//
		// // 连接到服务器
		// NetClient.Instance.TryConnectToServer();
		// while (true)
		// {
		// 	if (NetClient.Instance.state == NetClient.NetClientState.Connected) break;
		// 	if (NetClient.Instance.state == NetClient.NetClientState.Disconnected)
		// 	{
		// 		GameLogger.Error("服务器连接失败");
		// 		yield break;
		// 	}
		//
		// 	yield return null;
		// }
		//
		//
		// // 主逻辑
		// Main();
	}

	private void Main()
	{
		LoginManager.Instance.StartLogin();
	}
}
