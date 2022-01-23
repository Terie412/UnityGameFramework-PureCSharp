using System;
using System.Net;
using GameProtocol;
using Google.Protobuf;
using KCPNet;

namespace GameServer
{
    class Program
    {
        private static KCPServer kcpServer;
        
        private static void Main(string[] args)
        {
            kcpServer = new KCPServer();
            kcpServer.Start("127.0.0.1", 12000);

            kcpServer.onClientSessionCreated = OnClientSessionCreate;
            kcpServer.onKCPReceive = OnKCPReceive;
            
            ProtocolHandler.RegisterProtocol("LoginReq", OnLoginReq);

            Console.ReadKey();
        }

        private static void OnClientSessionCreate(KCPSession session)
        {
            KCPNetLogger.Info($"与新的客户端建立会话：{session.remoteIPEndPoint}");
        }

        private static void OnKCPReceive(byte[] bytesReceived, KCPSession session)
        {
            Protocol protocol = Protocol.Parser.ParseFrom(bytesReceived);
            uint id = protocol.Id;
            KCPNetLogger.Info($"收到协议, id = {id}");
            ProtocolHandler.id_parse[id](protocol.Data.ToByteArray(), session);
        }

        private static void OnLoginReq(object obj, KCPSession session)
        {
            KCPNetLogger.Info($"客户端请求登录: {session.sid}");
            LoginAck ack = new LoginAck();
            ack.Sid = session.sid;
            ack.PlayerInfo = new PlayInfo();
            ack.PlayerInfo.Name = "qintianchen";
            ack.PlayerInfo.Uid = 15614884;
            var loginAckBytes = ack.ToByteArray();

            Protocol protocol = new Protocol();
            protocol.Id = 2;
            protocol.Data = ByteString.CopyFrom(loginAckBytes);
            var bytes = protocol.ToByteArray();
            
            kcpServer.SendMessage(bytes, session.sid);
        }
    }
}