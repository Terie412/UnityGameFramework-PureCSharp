using System;
using KCPNet;

namespace Framework
{
	public static class ProtocolHandler
	{
		public static Action<object> onLoginReq;
		public static Action<object> onLoginAck;
		public static Action<object> onHeartBeatReq;
	}
}