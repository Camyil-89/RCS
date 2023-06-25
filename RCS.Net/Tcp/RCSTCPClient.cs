using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Tcp
{
	public enum ConnectStatus : byte
	{
		Disconnect = 0,
		Connecting = 1,
		Connect = 2,
	}
	public class RCSTCPClient
	{
		public TcpClient Client { get; private set; }
		public RCSTCPConnection Connection { get; private set; }
		public int BufferSize { get; private set; } = 1024; // 1 kb

		public delegate void CallbackClientStatus(ConnectStatus connect);
		public event CallbackClientStatus CallbackClientStatusEvent;

		public bool Connect(IPAddress ip, int socket)
		{
			Disconnect();
			Client = new TcpClient();
			CallbackClientStatusEvent?.Invoke(ConnectStatus.Connecting);
			try
			{
				Client.Connect(ip, socket);
				Connection = new RCSTCPConnection(Client.GetStream());
				Connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
				Connection.Start();
				CallbackClientStatusEvent?.Invoke(ConnectStatus.Connect);
				Console.WriteLine($"[CLIENT] Connect");
				return true;
			}
			catch { }
			CallbackClientStatusEvent?.Invoke(ConnectStatus.Disconnect);
			return false;
		}
		private void Connection_CallbackReceiveEvent(BasePacket packet)
		{
			if (packet.Type == PacketType.Ping)
				packet.Answer(packet);
		}

		public void Disconnect()
		{
			if (Client != null && Client.Connected)
			{
				Client.Close();
				Client.Dispose();
				CallbackClientStatusEvent?.Invoke(ConnectStatus.Disconnect);
			}
			Client = null;
		}
		public void Send(BasePacket packet)
		{
			Connection.Send(packet);
		}
	}
}
