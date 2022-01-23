using System.Collections;
using GameProtocol;
using KCPNet;
using UnityEngine;

public class GameMain : MonoBehaviour
{
	private IEnumerator Start()
	{
		// Application.targetFrameRate = 30;
		
		// 日志系统初始化
		GameLogger.Instance.Init();
		
		// 资产管理器初始化
		yield return AssetManager.Instance.Init();

		// 网络
		KCPNetLogger.onInfo = (str, _) => { Debug.Log(str); };
		KCPNetLogger.onWarning = (str, _) => { Debug.LogWarning(str); };
		KCPNetLogger.onError = (str, _) => { Debug.LogError(str); };

		gameObject.GetOrAddComponent<NetTicker>();
		
		NetClient.Instance.TryConnectToServer();

		while (true)
		{
			if (NetClient.Instance.state == NetClient.NetClientState.Connected) break;
			if (NetClient.Instance.state == NetClient.NetClientState.Disconnected)
			{
				GameLogger.Error("服务器连接失败");
				yield break;
			}

			yield return null;
		}
		
		// 主逻辑
		NetClient.Instance.RegisterProtocol("LoginAck", OnLoginAck);
		NetClient.Instance.RegisterProtocol("TestNetSpeedAck", OnTestNetSpeedAck);
		Main();
	}

	private void OnTestNetSpeedAck(object obj)
	{
		var ack = obj as TestNetSpeedAck;
		var now = TimeUtils.GetTimeStampFromT0();
		GameLogger.Info($"延迟:{now - ack.SendTimeStamp}, 服务器延迟: {ack.ReceiveTimeStamp - ack.SendTimeStamp}");
	}

	private void OnLoginAck(object obj)
	{
		StartCoroutine(test());
	}

	IEnumerator test()
	{
		for (int i = 0; i < 30; i++)
		{
			var msg = new TestNetSpeedReq();
			msg.SendTimeStamp = TimeUtils.GetTimeStampFromT0(); 
			NetClient.Instance.SendMessage(msg);
			yield return new WaitForSeconds(0.03f);
		}
	}

	private void Main()
	{
		LoginManager.Instance.StartLogin();
	}
}
