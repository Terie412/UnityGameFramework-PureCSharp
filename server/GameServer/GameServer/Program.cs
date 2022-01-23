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
            
            ProtocolHandler.onLoginReq = OnLoginReq;
            ProtocolHandler.onTestNetSpeedReq = OnTestNetSpeedReq;

            Console.ReadKey();
        }

        private static void OnClientSessionCreate(KCPSession session)
        {
            KCPNetLogger.Info($"与新的客户端建立会话：{session.remoteIPEndPoint}");
        }

        private static void OnKCPReceive(byte[] bytesReceived, KCPSession session)
        {
            ProtocolDispatcher.Dispatch(bytesReceived, session);
        }
        
        public static void SendMessage<T>(T msg, KCPSession session) where T: IMessage<T>
        {
            var bytes = msg.ToByteArray();
            Protocol p = new Protocol {Id = ProtocolDispatcher.name_id[typeof(T).Name], Data = ByteString.CopyFrom(bytes)};
            KCPNetLogger.Info($"发送消息: id = {p.Id}, len = {p.Data.Length}");
            kcpServer.SendMessage(p.ToByteArray(), session.sid);
        }

        private static void OnLoginReq(object obj, KCPSession session)
        {
            KCPNetLogger.Info($"客户端请求登录: {session.sid}");
            LoginAck ack = new LoginAck();
            ack.Sid = session.sid;
            ack.PlayerInfo = new PlayInfo();
            ack.PlayerInfo.Name = "qintianchen";
            ack.PlayerInfo.Uid = 15614884;
            
            SendMessage(ack, session);
        }
        
        public static long GetTimeStampFromT0()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        private static void OnTestNetSpeedReq(object obj, KCPSession session)
        {
            var ack = obj as TestNetSpeedReq;
            TestNetSpeedAck msg = new TestNetSpeedAck();
            msg.SendTimeStamp = ack.SendTimeStamp;
            msg.ReceiveTimeStamp = GetTimeStampFromT0();
            
            SendMessage(msg, session);
        }
    }
}