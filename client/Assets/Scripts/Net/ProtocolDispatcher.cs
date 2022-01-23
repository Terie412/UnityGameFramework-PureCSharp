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

	private static Dictionary<uint, Action<byte[]>> id_parser = new Dictionary<uint, Action<byte[]>>()
	{
		{1, LoginReqParser},
		{2, LoginAckParser},
		{3, HeartBeatReqParser},
		{4, TestNetSpeedReqParser},
		{5, TestNetSpeedAckParser},
	};

	public static void Dispatch(byte[] bytes)
	{
		Protocol p = Protocol.Parser.ParseFrom(bytes);
		id_parser[p.Id]?.Invoke(p.Data.ToByteArray());
	}

	public static void RegisterProtocol(string protocolName, Action<object> callback)
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

	private static void LoginReqParser(byte[] bytes) { object p = LoginReq.Parser.ParseFrom(bytes); ProtocolHandler.onLoginReq?.Invoke(p); }
	private static void LoginAckParser(byte[] bytes) { object p = LoginAck.Parser.ParseFrom(bytes); ProtocolHandler.onLoginAck?.Invoke(p); }
	private static void HeartBeatReqParser(byte[] bytes) { object p = HeartBeatReq.Parser.ParseFrom(bytes); ProtocolHandler.onHeartBeatReq?.Invoke(p); }
	private static void TestNetSpeedReqParser(byte[] bytes) { object p = TestNetSpeedReq.Parser.ParseFrom(bytes); ProtocolHandler.onTestNetSpeedReq?.Invoke(p); }
	private static void TestNetSpeedAckParser(byte[] bytes) { object p = TestNetSpeedAck.Parser.ParseFrom(bytes); ProtocolHandler.onTestNetSpeedAck?.Invoke(p); }
}
