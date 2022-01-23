using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Object = System.Object;

namespace GAutomatorView.Editor
{
	public class SocketClient
	{
		private Socket socket;

		private Dictionary<int, string> statusError = new Dictionary<int, string>()
		{
			{0, "SUCCESS, 成功"},
			{1, "NO_SUCH_CMD, 没有这个命令"},
			{2, "UNPACK_ERROR, 解析信息错误"},
			{3, "UN_KNOW_ERROR, 未知错误"},
			{4, "GAMEOBJ_NOT_EXIST, GameObject 不存在"},
			{5, "COMPONENT_NOT_EXIST, Component 不存在"},
			{6, "NO_SUCH_HANDLER, 没有这个接口"},
			{7, "REFLECTION_ERROR, 反射错误"},
			{8, "NO_SUCH_RESOURCE, 没有这个资源"}
		};

		public SocketClient(string address, int port)
		{
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(address), port);
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // GA transports bytes in stream based on TCP
			socket.Connect(ipEndPoint); // Actively connect to remote server
		}

		public object SendCommand(int cmd, object param)
		{
			if (param == null) 
				param = "";
			Dictionary<string, object> command = new Dictionary<string, object>();
			command.Add("cmd", cmd);
			command.Add("value", param);
			Send(command);
			object result = Receive();
			return result;
		}

		// Receive the data returned from server
		private object Receive()
		{
			string recvStr = "";

			// There is always four bytes(one int) in the beginning of the message to tell the length of message itself.
			// And the following parsing method applies to Little-End device, which means an integer equals to 37 is 00100101 00000000 00000000 00000000 in binary.
			byte[] lenByte = new byte[4];
			socket.Receive(lenByte, lenByte.Length, SocketFlags.None);
			int length = (0xff & lenByte[0]) |
			             (0xff & lenByte[1]) << 8 |
			             (0xff & lenByte[2]) << 16 |
			             (0xff & lenByte[3]) << 24;

			if (length <= 0) return "";
			byte[] recvBuffer = new byte[length];
			int receivedLength = 0;
			while (receivedLength < length)
			{
				int recvCount = socket.Receive(recvBuffer, length - receivedLength, SocketFlags.None);
				receivedLength += recvCount;
				recvStr += Encoding.ASCII.GetString(recvBuffer, 0, recvCount);
			}

			return ParseMessage(recvStr);
		}

		// Parse the data returned from server which is like byte{"cmd":106, "status":0,"data":"Login"} in json format
		private object ParseMessage(string message)
		{
			Dictionary<string, object> map = JsonConvert.DeserializeObject<Dictionary<string, Object>>(message);
			
			if (map == null)
			{
				throw new Exception($"Fail to parse receive message to hashMap: {message}");
			}
			int status = Convert.ToInt32(map["status"]); 
			if (status != 0)
			{
				statusError.TryGetValue(status, out string error);
				throw new Exception($"Returned messasge status error: state == {status}, {error}");
			}

			var data = map["data"];
			return data;
		}

		private void Send(object msg)
		{
			string serialized = JsonConvert.SerializeObject(msg);
			int length = serialized.Length;
			byte[] lenByte = new byte[4]; // GA need a byte to tell how long the message will be sent
			for (int i = 0; i < 4; i++)
			{
				lenByte[i] = (byte) ((length >> (i * 8)) & 0xff);
			}

			byte[] bytesToSend = Encoding.UTF8.GetBytes(serialized);
			socket.Send(lenByte, SocketFlags.None);
			socket.Send(bytesToSend, SocketFlags.None);
		}

		public void Dispose()
		{
			socket.Close();
		}
	}
}