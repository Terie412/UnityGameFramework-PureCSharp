﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KCPNet;
using GameProtocol;
using Google.Protobuf;
using UnityEngine;
using Object = UnityEngine.Object;

// 这个网络模块只是随便写写，一个基于KCP的简易通信模块
public class NetClient: SingleTon<NetClient>
{
    public KCPClient kcpClient;
    private Queue<byte[]> receiveQueue;
    private CancellationTokenSource connectCheckCTS;

    public enum NetClientState
    {
        None,
        Connecting,
        Connected,
        Disconnected,
    }

    public NetClientState state = NetClientState.None;
    
    public NetClient()
    {
        // 转接网络库的日志系统到Unity的日志系统
        KCPNetLogger.onInfo = (str, _) => { Debug.Log(str); };
        KCPNetLogger.onWarning = (str, _) => { Debug.LogWarning(str); };
        KCPNetLogger.onError = (str, _) => { Debug.LogError(str); };
        
        NetTicker ticker;
        if((ticker = Object.FindObjectOfType<NetTicker>()) == null)
        {
            ticker = new GameObject().AddComponent<NetTicker>();
            ticker.gameObject.name = "NetTicker";
        }

        receiveQueue = new Queue<byte[]>();

        ticker.onUpdate = Update;
        ticker.onApplicationQuit = () =>
        {
            connectCheckCTS?.Cancel();
        };
    }

    private void Update()
    {
        lock (receiveQueue)
        {
            if (receiveQueue.Count > 0)
            {
                ProtocolDispatcher.Dispatch(receiveQueue.Dequeue());
            }
        }
    }

    public async Task<bool> TryConnectToServer()
    {
        kcpClient?.Close();

        kcpClient = new KCPClient();
        kcpClient.Start("10.90.239.80", 12000);
        kcpClient.onKCPReceive = OnKCPReceive;

        state = NetClientState.Connecting;
        
        connectCheckCTS = new CancellationTokenSource();
        var result = await kcpClient.TryConnectToServer();
        return result;
    }
    
    public void RegisterProtocol(string protocalName, Action<object> callback)
    {
        ProtocolDispatcher.RegisterProtocol(protocalName, callback);
    }

    public void SendMessage<T>(T msg) where T: IMessage<T>
    {
        var bytes = msg.ToByteArray();
        Protocol p = new Protocol {Id = ProtocolDispatcher.name_id[typeof(T).Name], Data = ByteString.CopyFrom(bytes)};
        kcpClient.SendMessage(p.ToByteArray());
    }

    private void OnKCPReceive(byte[] bytesReceived)
    {
        lock (receiveQueue)
        {
            receiveQueue.Enqueue(bytesReceived);
        }
    }
}