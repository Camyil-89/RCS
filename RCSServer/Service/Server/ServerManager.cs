using EasyTCP;
using EasyTCP.Serialize;
using Microsoft.Extensions.DependencyInjection;
using OpenGost.Security.Cryptography;
using RCS.Certificates;
using RCS.Net;
using RCS.Net.Packets;
using RCS.Service;
using RCS.Service.UI;
using RCSServer.ViewModels.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace RCSServer.Service.Server
{
	public static class ServerManager
	{
		public static EasyTCP.Server Server = new EasyTCP.Server();
		private static MainVM MainVM => App.Host.Services.GetRequiredService<MainVM>();
		public static void Start()
		{
			Server.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.Ping>(1).CallbackReceiveEvent += ServerManager_CallbackReceiveEvent;
			Server.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketCertificate>(2).CallbackReceiveEvent += ServerManager_CallbackReceiveEvent1;
			Server.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketRequestCertificates>(3).CallbackReceiveEvent += ServerManager_CallbackReceiveEvent2;
			Server.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketRequestCertificate>(4).CallbackReceiveEvent += ServerManager_CallbackReceiveEvent3;
			Server.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketRequestNetCertificates>(5).CallbackReceiveEvent += ServerManager_CallbackReceiveEvent4;
			Server.CallbackConnectClientEvent += Server_CallbackConnectClientEvent;
			Server.CallbackDisconnectClientEvent += Server_CallbackDisconnectClientEvent;
			OpenGostCryptoConfig.ConfigureCryptographicServices();


			if (Settings.Instance.Certificate != null)
			{
				try
				{
					var cert = new X509Certificate2((byte[])Settings.Instance.Certificate.Certificate.Info.Attributes.FirstOrDefault(i => i.Name == "RCS.SSL.CERTIFICATE.X509").Data);
					Server.EnableSsl(cert);
				} catch (Exception ex) { MessageBoxHelper.ErrorShow($"Не удалось запусть SSL шифрование!\n\n{ex}"); }
			}
			Server.Start(1991,new RCS.Net.Firewall.SvarogFirewall());
			Task.Run(UpdateInfoClients);
		}

		private static void ServerManager_CallbackReceiveEvent4(object packet, EasyTCP.Packets.Packet rawPacket)
		{
			try
			{

			}
			catch { rawPacket.SafeAnswerNull(); }
		}

		private static void ServerManager_CallbackReceiveEvent3(object packet, EasyTCP.Packets.Packet rawPacket)
		{
			try
			{
				var req = (RCS.Net.Packets.PacketRequestCertificate)(packet);
				req.Certificate = RCS.Certificates.CertificateManager.Store.GetItem(req.RequestPacket).Certificate;
				Server.Answer(rawPacket, req);
			}
			catch { rawPacket.SafeAnswerNull(); }
		}

		private static void Server_CallbackDisconnectClientEvent(EasyTCP.ServerClient client)
		{
			try
			{
				var x = Settings.Instance.Clients.First(i => i.Client.IpPort == client.IpPort);
				App.Current.Dispatcher.Invoke(() => { Settings.Instance.Clients.Remove(x); });
			}
			catch { }
		}

		private static void ServerManager_CallbackReceiveEvent2(object packet, EasyTCP.Packets.Packet rawPacket)
		{
			try
			{
				List<object> certificates = new List<object>();
				foreach (var i in RCS.Certificates.CertificateManager.Store.Certificates)
				{
					certificates.Add(i.Certificate);
				}
				Server.Answer(rawPacket, new RCS.Net.Packets.PacketRequestCertificates() { Certificates = certificates });
			}
			catch
			{
				rawPacket.SafeAnswerNull();
			}
		}

		private static void ServerManager_CallbackReceiveEvent1(object packet, EasyTCP.Packets.Packet rawPacket)
		{
			try
			{
				var cert = (Certificate)(((PacketCertificate)packet).CertificateObj);
				var info = RCS.Certificates.CertificateManager.Store.FindMasterCertificate(cert);
				var pckt = new PacketCertificate() { IsValid = info != null };
				Server.Answer(rawPacket, pckt);
			}
			catch
			{
				try
				{
					rawPacket.AnswerNull();
				}
				catch { }
			}
		}

		private static void ServerManager_CallbackReceiveEvent(object packet, EasyTCP.Packets.Packet rawPacket)
		{
			rawPacket.Answer(rawPacket);
		}
		private static string FormatBytesPerSecond(double bytesPerSecond)
		{
			string[] suffixes = { "Байт", "КБайт", "МБайт", "ГБайт", "ТБайт" };
			int suffixIndex = 0;

			while (bytesPerSecond >= 1024 && suffixIndex < suffixes.Length - 1)
			{
				bytesPerSecond /= 1024;
				suffixIndex++;
			}

			return $"{bytesPerSecond:0.##} {suffixes[suffixIndex]}";
		}
		private static void UpdateInfoClients()
		{
			while (true)
			{
				foreach (var i in Settings.Instance.Clients)
				{
					try
					{
						i.Ping = Math.Round((DateTime.Now - i.Client.Client.SendAndWaitResponse<Ping>(new Ping(), 5000).Time).TotalMilliseconds, 2);
						i.RXBytes = $"{FormatBytesPerSecond(i.Client.Connection.Statistics.ReceivedBytes)}";
						i.TXBytes = $"{FormatBytesPerSecond(i.Client.Connection.Statistics.SentBytes)}";
						i.RXPackets = i.Client.Connection.Statistics.ReceivedPackets;
						i.TXPackets = i.Client.Connection.Statistics.SentPackets;
						i.TXSpeed = $"{FormatBytesPerSecond(i.Client.Connection.Statistics.AverageSentBytesSpeed)}\\C";
						i.RXSpeed = $"{FormatBytesPerSecond(i.Client.Connection.Statistics.AverageReceivedBytesSpeed)}\\C";
					}
					catch
					{

					}
				}
				Thread.Sleep(1000);
			}
		}
		private static void Server_CallbackConnectClientEvent(EasyTCP.ServerClient client)
		{
			Console.WriteLine($"[SERVER] {client.IpPort} ");
			//var sec = new SecureSerialize();
			//sec.PrivateKey = Settings.Instance.Certificate.PrivateKey;
			//sec.PublicKey = Settings.Instance.Certificate.Certificate.Info.PublicKey;
			//client.Connection.Serialization = sec;
			//client.Connection.InitSerialization();
			App.Current.Dispatcher.Invoke(() => { Settings.Instance.Clients.Add(new Models.ServerClient() { Client = client }); });
		}
		public static void Stop()
		{
			Server.Stop();
		}
	}
}
