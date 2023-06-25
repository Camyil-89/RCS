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
					Task.Run(() => { HandlerClient(client); });
				}
				catch (Exception e) { Console.WriteLine(e); }
			}
		}
		private void HandlerClient(TcpClient Client)
		{
			Console.WriteLine($"[SERVER] {Client.Client.RemoteEndPoint}");

			RCSTCPConnection connection = new RCSTCPConnection(Client.GetStream());
			connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
			connection.Start();
			while (Client != null && Client.Connected)
			{
				var packet = connection.SendAndWait(new Ping());
				if (packet.Type == PacketType.Ping)
				{
					Console.WriteLine($"[SERVER] ping: {(DateTime.Now - ((Ping)packet).Time).TotalMilliseconds}");
				}
				Thread.Sleep(1000);
			}
		}

		private void Connection_CallbackReceiveEvent(Packets.BasePacket packet)
		{
			Console.WriteLine($"[SERVER RX] {packet}");
		}

		public void Stop()
		{
			if (TcpListener != null)
			{
				TcpListener.Stop();
				TcpListener = null;
			}
		}
	}
}
