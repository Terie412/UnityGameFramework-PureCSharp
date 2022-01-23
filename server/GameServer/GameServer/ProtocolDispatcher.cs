using System;
using System.Collections.Generic;
using GameProtocol;
using KCPNet;

public class ProtocolDispatcher
{
	public static Dictionary<string, uint> name_id = new Dictionary<string, uint>()
	{
		{"LoginReq", 1},
		{"LoginAck", 2},
		{"HeartBeatReq", 3},
		{"TestNetSpeedReq", 4},
		{"TestNetSpeedAck", 5},
	};

	private static Dictionary<uint, Action<byte[], KCPSession>> id_parser = new Dictionary<uint, Action<byte[], KCPSession>>()
	{
		{1, LoginReqParser},
		{2, LoginAckParser},
		{3, HeartBeatReqParser},
		{4, TestNetSpeedReqParser},
		{5, TestNetSpeedAckParser},
	};

	public static void Dispatch(byte[] bytes, KCPSession session)
	{
		Protocol p = Protocol.Parser.ParseFrom(bytes);
		id_parser[p.Id]?.Invoke(p.Data.ToByteArray(), session);
	}

	public static void RegisterProtocol(string protocolName, Action<object, KCPSession> callback)
	{
		if (!name_id.TryGetValue(protocolName, out var id)) return;

		switch (id)
		{
			case 1: ProtocolHandler.onLoginReq += callback; break;
			case 2: ProtocolHandler.onLoginAck += callback; break;
			case 3: ProtocolHandler.onHeartBeatReq += callback; break;
			case 4: ProtocolHandler.onTestNetSpeedReq += callback; break;
			case 5: ProtocolHandler.onTestNetSpeedAck += callback; break;
		}
	}

	private static void LoginReqParser(byte[] bytes, KCPSession session) { object p = LoginReq.Parser.ParseFrom(bytes); ProtocolHandler.onLoginReq?.Invoke(p, session); }
	private static void LoginAckParser(byte[] bytes, KCPSession session) { object p = LoginAck.Parser.ParseFrom(bytes); ProtocolHandler.onLoginAck?.Invoke(p, session); }
	private static void HeartBeatReqParser(byte[] bytes, KCPSession session) { object p = HeartBeatReq.Parser.ParseFrom(bytes); ProtocolHandler.onHeartBeatReq?.Invoke(p, session); }
	private static void TestNetSpeedReqParser(byte[] bytes, KCPSession session) { object p = TestNetSpeedReq.Parser.ParseFrom(bytes); ProtocolHandler.onTestNetSpeedReq?.Invoke(p, session); }
	private static void TestNetSpeedAckParser(byte[] bytes, KCPSession session) { object p = TestNetSpeedAck.Parser.ParseFrom(bytes); ProtocolHandler.onTestNetSpeedAck?.Invoke(p, session); }
}
