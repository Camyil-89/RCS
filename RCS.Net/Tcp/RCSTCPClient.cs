using OpenGost.Security.Cryptography;
using RCS.Net.Firewall;
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
	public class ExceptionNotConnect : Exception
	{
		public ExceptionNotConnect(string message = "dont connected to RCS server")
		: base(message)
		{
		}

		public ExceptionNotConnect(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
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

		public byte[] PublicKey { get; set; } = new byte[0];
		public int TimeoutUpdateKeys { get; set; } = 10; // seconds

		public double Ping { get; private set; } = -1;

		public delegate void CallbackClientStatus(ConnectStatus connect);
		public event CallbackClientStatus CallbackClientStatusEvent;

		private string Address { get; set; } = "";
		private int Port { get; set; } = -1;
		public bool Connect(string address, int socket)
		{
			OpenGostCryptoConfig.ConfigureCryptographicServices();
			Disconnect();
			Client = new TcpClient();
			CallbackClientStatusEvent?.Invoke(ConnectStatus.Connecting);
			try
			{
				Address = address;
				Port = socket;
				Client.Connect(address, socket);
				Connection = new RCSTCPConnection(Client.GetStream(), PublicKey, null, new EmptyFirewall());
				Connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
				Connection.Start();
				CallbackClientStatusEvent?.Invoke(ConnectStatus.Connect);
				Task.Run(() =>
				{
					Stopwatch stopwatch_ping = Stopwatch.StartNew();
					Stopwatch stopwatch_update_keys = Stopwatch.StartNew();
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
							if (stopwatch_update_keys.ElapsedMilliseconds >= TimeoutUpdateKeys * 1000)
							{
								Connection.UpdateKeys();
								stopwatch_update_keys.Restart();
							}
						}
						catch (TimeoutException)
						{
							count_timeout++;
							Console.WriteLine($"Timeout error {count_timeout} \\ 5");
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
					Connection = null;
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
				Connection = null;
			}
			Client = null;
			Port = -1;
		}
		public void Send(BasePacket packet)
		{
			Connection.Send(packet);
		}
		private TcpClient GetNewTCPClient()
		{
			if (Port == -1)
				throw new ExceptionNotConnect();
			var client = new TcpClient();
			client.Connect(Address, Port);
			return client;
		}
		//public BasePacket SendAndWait(BasePacket packet)
		//{
		//	var client = GetNewTCPClient();
		//	RCSTCPConnection connection = null;
		//	BasePacket answer = null;
		//	try
		//	{
		//		connection = new RCSTCPConnection(client.GetStream());
		//		connection.Mode = ModeConnection.RequestAnswer;
		//		connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
		//		connection.Start(true);
		//
		//		answer = connection.SendAndWait(packet);
		//		connection.SendAndWait(new Packet() { Type = PacketType.Disconnect });
		//	} catch (Exception ex) { Console.WriteLine(ex); }
		//	finally
		//	{
		//		connection.Abort();
		//		client.Close();
		//		client.Dispose();
		//	}
		//	return answer;
		//}
	}
}
