using System.Collections;
using GameProtocol;
using KCPNet;
using Newtonsoft.Json;
using UnityEngine;

public class GameMain : MonoBehaviour
{
	private IEnumerator Start()
	{
		Application.targetFrameRate = 30;
		
		// 日志系统初始化
		GameLogger.Instance.Init();
		
		// 资产管理器初始化
		yield return AssetManager.Instance.Init();

		{
			// Lua 的逻辑演示
			
			// ToLua 初始化
			// 执行主逻辑
			// LuaMain.Instance.Init(this);
			// LuaMain.Instance.lua.DoFile("Main");
		}

		// 网络
		KCPNetLogger.onInfo = (str, color) =>
		{
			Debug.Log(str);
		};
		
		KCPNetLogger.onWarning = (str, color) =>
		{
			Debug.LogWarning(str);
		};
		
		KCPNetLogger.onError = (str, color) =>
		{
			Debug.LogError(str);
		};
		
		ProtocolHandler.RegisterProtocol("LoginAck", OnLoginAck);
		NetClient.Instance.onTryConnectToServerEnd = OnTryConnectToServerEnd;
		NetClient.Instance.TryConnectToServer();
	}

	private void OnTryConnectToServerEnd(bool isSuccess)
	{
		if (!isSuccess)
		{
			Debug.Log("连接到服务器失败");
			return;
		}

		Debug.Log("连接到服务器成功！");
		NetClient.Instance.TryLogin();
	}

	// 现在依然是另一个线程在调用，所以希望网络的回包放在一个线程安全队列里面让主线程每帧去取
	private void OnLoginAck(object obj, KCPSession session)
	{
		LoginAck loginAck = obj as LoginAck;
		Debug.Log($"登录成功: {JsonConvert.SerializeObject(loginAck)}");
	}
}
