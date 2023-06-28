using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCSServer.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace RCSServer.Service.Server
{
	public static class ServerManager
	{
		public static RCS.Net.Tcp.RCSTCPListener Server = new RCS.Net.Tcp.RCSTCPListener();
		private static MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();
		public static void Connect()
		{
			Server.Start(1991);
			Server.CallbackReceiveEvent += Server_CallbackReceiveEvent;
			Server.CallbackConnectClientEvent += Server_CallbackConnectClientEvent;
			Server.CallbackDisconnectClientEvent += Server_CallbackDisconnectClientEvent;
		}

		private static void Server_CallbackDisconnectClientEvent(System.Net.EndPoint endPoint)
		{
			Console.WriteLine($"[SERVER] Disconnect: {endPoint}");
			MainVM.Test = $"{Server.Clients.Count}";
		}

		private static void Server_CallbackConnectClientEvent(System.Net.Sockets.TcpClient client)
		{
			Console.WriteLine($"[SERVER] Connect: {client.Client.RemoteEndPoint}");
			MainVM.Test = $"{Server.Clients.Count}";
		}

		private static void Server_CallbackReceiveEvent(RCS.Net.Packets.BasePacket packet)
		{
			if (packet.Type == RCS.Net.Packets.PacketType.RequestCertificates)
			{
				List<string> certificates = new List<string>();
				foreach (var i in Settings.Instance.CertificatesStore.Certificates)
				{
					certificates.Add(i.Certificate.Raw());
				}
				packet.Data = certificates.ToArray();
				packet.Answer(packet);
			}
			else if (packet.Type == RCS.Net.Packets.PacketType.ValidatingCertificate)
			{
				var cert = new Certificate().FromRaw((string)packet.Data);
				packet.Data = Settings.Instance.CertificatesStore.FindMasterCertificate(cert).Certificate != null;
				packet.Answer(packet);
			}
			else if (packet.Type == RCS.Net.Packets.PacketType.RequestCertificate)
			{
				packet.Data = Settings.Instance.CertificatesStore.GetItem((Guid)packet.Data).Certificate;
				packet.Answer(packet);
			}
		}

		public static void Stop()
		{
			Server.Stop();
		}
	}
}
