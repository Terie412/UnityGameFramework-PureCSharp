using System;
using GameProtocol;
using Google.Protobuf;
using KCPNet;


class ServerMain
{
    private static KCPServer kcpServer;

    private static void Main(string[] args)
    {
        kcpServer = new KCPServer();
        kcpServer.Start("10.90.239.80", 12000);

        kcpServer.onClientSessionCreated = OnClientSessionCreate;
        kcpServer.onKCPReceive = OnKCPReceive;
        
        LoginManager.Instance.Init();
        
        Console.ReadKey();
    }

    public static void RegisterProtocol(string protocolName, Action<object, KCPSession> callback)
    {
        ProtocolDispatcher.RegisterProtocol(protocolName, callback);        
    }
    
    public static void SendMessage<T>(T msg, KCPSession session) where T : IMessage<T>
    {
        var bytes = msg.ToByteArray();
        Protocol p = new Protocol {Id = ProtocolDispatcher.name_id[typeof(T).Name], Data = ByteString.CopyFrom(bytes)};
        kcpServer.SendMessage(p.ToByteArray(), session.sid);
    }

    #region private

    private static void OnClientSessionCreate(KCPSession session)
    {
        KCPNetLogger.Info($"与新的客户端建立会话：{session.remoteIPEndPoint}");
    }

    private static void OnKCPReceive(byte[] bytesReceived, KCPSession session)
    {
        ProtocolDispatcher.Dispatch(bytesReceived, session);
    }

    #endregion
}