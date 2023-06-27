using RCS.Certificates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.Service.Server
{
	public static class ServerManager
	{
		public static RCS.Net.Tcp.RCSTCPListener Server = new RCS.Net.Tcp.RCSTCPListener();
		public static void Connect()
		{
			Server.Start(1991);
			Server.CallbackReceiveEvent += Server_CallbackReceiveEvent;
			Server.CallbackConnectClientEvent += Server_CallbackConnectClientEvent;
		}

		private static void Server_CallbackConnectClientEvent(System.Net.Sockets.TcpClient client)
		{
			Console.WriteLine($"[SERVER] Connect: {client.Client.RemoteEndPoint}");
		}

		private static void Server_CallbackReceiveEvent(RCS.Net.Packets.BasePacket packet)
		{
			Console.WriteLine($">>>>>{packet}");
			if (packet.Type == RCS.Net.Packets.PacketType.RequestCertificates)
			{
				List<Certificate> certificates = new List<Certificate>();
				foreach (var i in Settings.Instance.CertificatesStore.Certificates)
				{
					certificates.Add(i.Certificate);
				}
				packet.Data = certificates.ToArray();
				packet.Answer(packet);
			}
		}

		public static void Stop()
		{
			Server.Stop();
		}
	}
}
