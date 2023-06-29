using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Service;
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
		public static void Start()
		{
			Server.Start(1991);
			Server.CallbackReceiveEvent += Server_CallbackReceiveEvent;
			Server.CallbackConnectClientEvent += Server_CallbackConnectClientEvent; ;
			Server.CallbackDisconnectClientEvent += Server_CallbackDisconnectClientEvent; ;
		}

		private static void Server_CallbackDisconnectClientEvent(RCS.Net.Tcp.RCSClient client)
		{
			Settings.Instance.Clients.Remove(client);
			Log.WriteLine($"Disconnect: {client.EndPoint}");
		}

		private static void Server_CallbackConnectClientEvent(RCS.Net.Tcp.RCSClient client)
		{
			Settings.Instance.Clients.Add(client);
			Log.WriteLine($"Connect: {client.EndPoint}");
		}

		private static void Server_CallbackReceiveEvent(RCS.Net.Packets.BasePacket packet)
		{
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
			else if (packet.Type == RCS.Net.Packets.PacketType.ValidatingCertificate)
			{
				packet.Type = RCS.Net.Packets.PacketType.RSTStopwatch;
				packet.Answer(packet);
				try
				{
					var cert = (Certificate)packet.Data;
					packet.Data = Settings.Instance.CertificatesStore.FindMasterCertificate(cert).Certificate != null;
					packet.Answer(packet);
					packet.Type = RCS.Net.Packets.PacketType.ValidatingCertificate;
					packet.Answer(packet);
				}
				catch (Exception ex) { Console.WriteLine(ex); }
				Console.WriteLine("ASDASDAS");
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
