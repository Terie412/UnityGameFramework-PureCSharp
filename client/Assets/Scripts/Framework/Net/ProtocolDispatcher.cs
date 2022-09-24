using System;
using System.Collections.Generic;
using GameProtocol;
using KCPNet;

namespace Framework
{
	public class ProtocolDispatcher
	{
		public static Dictionary<string, uint> name_id = new Dictionary<string, uint>()
		{
			{ "LoginReq", 1 },
			{ "LoginAck", 2 },
			{ "HeartBeatReq", 3 },
		};

		private static Dictionary<uint, Action<byte[]>> id_parser = new Dictionary<uint, Action<byte[]>>()
		{
			{ 1, LoginReqParser },
			{ 2, LoginAckParser },
			{ 3, HeartBeatReqParser },
		};

		public static void Dispatch(byte[] bytes)
		{
			try
			{
				Protocol p = Protocol.Parser.ParseFrom(bytes);
				id_parser[p.Id]?.Invoke(p.Data.ToByteArray());
			}
			catch (Exception e)
			{
				KCPNetLogger.Error(e.ToString());
			}
		}

		public static void RegisterProtocol(string protocolName, Action<object> callback)
		{
			if (!name_id.TryGetValue(protocolName, out var id)) return;

			switch (id)
			{
				case 1:
					ProtocolHandler.onLoginReq += callback;
					break;
				case 2:
					ProtocolHandler.onLoginAck += callback;
					break;
				case 3:
					ProtocolHandler.onHeartBeatReq += callback;
					break;
			}
		}

		private static void LoginReqParser(byte[] bytes)
		{
			object p = LoginReq.Parser.ParseFrom(bytes);
			ProtocolHandler.onLoginReq?.Invoke(p);
		}

		private static void LoginAckParser(byte[] bytes)
		{
			object p = LoginAck.Parser.ParseFrom(bytes);
			ProtocolHandler.onLoginAck?.Invoke(p);
		}

		private static void HeartBeatReqParser(byte[] bytes)
		{
			object p = HeartBeatReq.Parser.ParseFrom(bytes);
			ProtocolHandler.onHeartBeatReq?.Invoke(p);
		}
	}
}