using System;
using KCPNet;

public static class ProtocolHandler
{
	public static Action<object, KCPSession> onLoginReq;
	public static Action<object, KCPSession> onLoginAck;
	public static Action<object, KCPSession> onHeartBeatReq;
}