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

		public double Ping { get; private set; } = -1;

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
				Connection.Start(true);
				CallbackClientStatusEvent?.Invoke(ConnectStatus.Connect);
				Task.Run(() =>
				{
					Stopwatch stopwatch_ping = Stopwatch.StartNew();
					int count_timeout = 0;
					while (Client != null && Client.Connected && Client.Client.Connected)
					{
						try
						{
							if (stopwatch_ping.ElapsedMilliseconds >= 1000)
							{
								stopwatch_ping.Restart();
								count_timeout = 0;
								var packet = (Ping)Connection.SendAndWait(new Ping());
								Ping = (DateTime.Now - packet.Time).TotalMilliseconds;
							}
						}
						catch (TimeoutException)
						{
							count_timeout++;
							if (count_timeout == 5)
							{
								Console.WriteLine($"[CLIENT] lost connection timeout");
								break;
							}
						}
						catch { }
						Thread.Sleep(1);
					}
					Connection.Abort();
					CallbackClientStatusEvent?.Invoke(ConnectStatus.Disconnect);
				});
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
				Connection.Abort();
			}
			Client = null;
		}
		public void Send(BasePacket packet)
		{
			Connection.Send(packet);
		}
	}
}
