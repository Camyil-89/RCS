using RCS.Net.Packets;
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
		private List<TcpClient> Clients = new List<TcpClient>();
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
				catch (Exception e) { Console.WriteLine(e); }
			}
		}
		private void HandlerClient(TcpClient Client)
		{
			var ip_client = Client.Client.RemoteEndPoint;
			Console.WriteLine($"[SERVER] {ip_client}");

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
				} catch { }
			}
			connection.Abort();
			Console.WriteLine($"[SERVER] disconnect: {ip_client}");
			Clients.Remove(Client);
		}

		private void Connection_CallbackReceiveEvent(Packets.BasePacket packet)
		{
			//Console.WriteLine($"[SERVER RX] {packet}");
			if (packet.Type == PacketType.Ping)
				packet.Answer(packet);
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
				} catch { }
				Thread.Sleep(1);
			}
		}
	}
}
