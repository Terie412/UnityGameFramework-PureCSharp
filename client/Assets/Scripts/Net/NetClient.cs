using System;
using System.Text;
using System.Threading.Tasks;
using KCPNet;
using GameProtocol;
using Google.Protobuf;
using UnityEngine;

// 这个网络模块只是随便写写，一个基于KCP的简易通信模块
public class NetClient: SingleTon<NetClient>
{
    public KCPClient kcpClient;

    public Action<bool> onTryConnectToServerEnd;

    public void TryConnectToServer()
    {
        kcpClient = new KCPClient();
        kcpClient.Start("127.0.0.1", 12000);

        kcpClient.onKCPReceive = OnKCPReceive;

        Task<bool> connectTask = kcpClient.TryConnectToServer();
        Task.Run(() =>
        {
            ConnectCheckAsync(connectTask);
        });
    }
    
    public bool TryLogin()
    {
        Debug.Log("尝试登录游戏");
        LoginReq loginReq = new LoginReq();
        byte[] loginReq_buffer = loginReq.ToByteArray();
        
        Protocol protocol = new Protocol();
        protocol.Id = 1;
        protocol.Data = ByteString.CopyFrom(loginReq_buffer);
        var ret = kcpClient.SendMessage(protocol.ToByteArray());
        return ret;
    }

    private void OnKCPReceive(byte[] bytesReceived)
    {
        Protocol protocol = Protocol.Parser.ParseFrom(bytesReceived);
        uint id = protocol.Id;
        ProtocolHandler.id_parse[id](protocol.Data.ToByteArray(), null);
    }

    private async void ConnectCheckAsync(Task<bool> connectTask)
    {
        Debug.Log($"ConnectCheckAsync");

        int failCount = 0;
        while (true)
        {
            Debug.Log($"尝试第 {failCount + 1} 次");
            if (connectTask != null && connectTask.IsCompleted)
            {
                if (connectTask.Result)
                {
                    onTryConnectToServerEnd?.Invoke(true);
                    break;
                }

                failCount++;
                if (failCount > 5)
                {
                    onTryConnectToServerEnd?.Invoke(false);
                    break;
                }

                connectTask = kcpClient.TryConnectToServer();
            }
                
            await Task.Delay(1000);
        }
    }
}