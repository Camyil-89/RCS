using RCS.Net.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace RCS.Net.Tcp
{
	public class RCSClient: Base.ViewModel.BaseViewModel
	{

		#region Statistics: Description
		/// <summary>Description</summary>
		private ConnectionStatistics _Statistics = new ConnectionStatistics();
		/// <summary>Description</summary>
		public ConnectionStatistics Statistics { get => _Statistics; set => Set(ref _Statistics, value); }
		#endregion

		#region Client: Description
		/// <summary>Description</summary>
		private TcpClient _Client;
		/// <summary>Description</summary>
		public TcpClient Client { get => _Client; set => Set(ref _Client, value); }
		#endregion

		#region Connection: Description
		/// <summary>Description</summary>
		private RCSTCPConnection _Connection;
		/// <summary>Description</summary>
		public RCSTCPConnection Connection { get => _Connection; set => Set(ref _Connection, value); }
		#endregion

		#region EndPoint: Description
		/// <summary>Description</summary>
		private EndPoint _EndPoint;
		/// <summary>Description</summary>
		public EndPoint  EndPoint { get => _EndPoint; set => Set(ref _EndPoint, value); }
		#endregion


		#region Ping: Description
		/// <summary>Description</summary>
		private double _Ping = -1;
		/// <summary>Description</summary>
		public double Ping { get => _Ping; set => Set(ref _Ping, value); }
		#endregion

		public void CallbackReceiveEvent(Packets.BasePacket packet)
		{
			if (packet.Type == PacketType.Disconnect)
			{
				packet.Answer(packet);
				Connection.Abort();
				Client.Close();
				Client.Dispose();
			}
		}
	}
	public class RCSTCPListener
	{
		public TcpListener TcpListener { get; private set; }
		public List<TcpClient> Clients = new List<TcpClient>();

		public delegate void CallbackReceive(BasePacket packet);
		public event CallbackReceive CallbackReceiveEvent;

		public delegate void CallbackConnectClient(RCSClient client);
		public event CallbackConnectClient CallbackConnectClientEvent;

		public delegate void CallbackDisconnectClient(RCSClient client);
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
			var rcs_client = new RCSClient();

			RCSTCPConnection connection = new RCSTCPConnection(Client.GetStream());
			connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
			connection.CallbackReceiveEvent += rcs_client.CallbackReceiveEvent;
			connection.Statistics = rcs_client.Statistics;
			connection.Start(false);

			rcs_client.Client = Client;
			rcs_client.EndPoint = Client.Client.RemoteEndPoint;
			rcs_client.Connection = connection;
			CallbackConnectClientEvent?.Invoke(rcs_client);
			
			while (Client != null && Client.Connected && Client.Client.Connected)
			{
				try
				{
					Thread.Sleep(1000);
					var packet = connection.SendAndWait(new Ping());
					if (packet.Type == PacketType.Ping)
					{
						rcs_client.Ping = (DateTime.Now - ((Ping)packet).Time).TotalMilliseconds;
						Console.WriteLine($"[SERVER {Clients.Count}] ping: {rcs_client.Ping}");
					}
				}
				catch { }
			}
			connection.Abort();
			Clients.Remove(Client);
			CallbackDisconnectClientEvent?.Invoke(rcs_client);
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
