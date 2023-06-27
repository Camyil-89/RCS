﻿using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Tcp
{
	public class RCSTCPListener
	{
		public TcpListener TcpListener { get; private set; }
		public List<TcpClient> Clients = new List<TcpClient>();

		public delegate void CallbackReceive(BasePacket packet);
		public event CallbackReceive CallbackReceiveEvent;

		public delegate void CallbackConnectClient(TcpClient client);
		public event CallbackConnectClient CallbackConnectClientEvent;

		public delegate void CallbackDisconnectClient(EndPoint endPoint);
		public event CallbackDisconnectClient CallbackDisconnectClientEvent;
		public void Start(int socket)
		{
			TcpListener = new TcpListener(IPAddress.Any, socket);
			TcpListener.Start();
			Task.Run(Listener);

		}
		private void Listener()
		{
			Console.WriteLine($"[SERVER] start");
			while (TcpListener != null)
			{
				try
				{
					var client = TcpListener.AcceptTcpClient();
					Clients.Add(client);
					Thread thread = new Thread(() => { HandlerClient(client); });
					thread.Start();

				}
				catch (Exception e) { }
			}
		}
		private void HandlerClient(TcpClient Client)
		{
			var ip_client = Client.Client.RemoteEndPoint;
			CallbackConnectClientEvent?.Invoke(Client);
			RCSTCPConnection connection = new RCSTCPConnection(Client.GetStream());
			connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
			connection.Start(false);
			while (Client != null && Client.Connected && Client.Client.Connected)
			{
				try
				{
					Thread.Sleep(1000);
					var packet = connection.SendAndWait(new Ping());
					if (packet.Type == PacketType.Ping)
					{
						Console.WriteLine($"[SERVER {Clients.Count}] ping: {(DateTime.Now - ((Ping)packet).Time).TotalMilliseconds}");
					}
				}
				catch { }
			}
			connection.Abort();
			Console.WriteLine($"[SERVER] disconnect: {ip_client}");
			Clients.Remove(Client);
			CallbackDisconnectClientEvent?.Invoke(ip_client);
		}

		private void Connection_CallbackReceiveEvent(Packets.BasePacket packet)
		{
			//Console.WriteLine($"[SERVER RX] {packet}");
			if (packet.Type == PacketType.Ping)
				packet.Answer(packet);
			else
			{
				CallbackReceiveEvent?.Invoke(packet);
			}
		}

		public void Stop()
		{
			if (TcpListener != null)
			{
				TcpListener.Stop();
				TcpListener = null;
			}
			while (Clients.Count > 0)
			{
				try
				{
					foreach (var i in Clients)
					{
						i.Close();
						i.Dispose();
					}
				}
				catch { }
				Thread.Sleep(1);
			}
		}
	}
}
