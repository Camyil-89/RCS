using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Net.Packets;
using RCS.ViewModels.Pages.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RCS.Service.UI.Client
{
	public static class ClientManager
	{
		private static DateTime LastUpdateKeys = DateTime.Now;
		private static RCS.Net.Tcp.RCSTCPClient Client = new Net.Tcp.RCSTCPClient();
		private static CenterCertificationsPageVM CenterCertificationsPageVM => App.Host.Services.GetRequiredService<CenterCertificationsPageVM>();
		private static bool IsAutoConnect = false;
		public static void Connect()
		{
			Client = new Net.Tcp.RCSTCPClient();
			Client.CallbackClientStatusEvent += Client_CallbackClientStatusEvent;
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			Client.TimeoutUpdateKeys = Settings.Instance.Parametrs.Client.TimeoutUpdateKeys;
			Task.Run(AutoConnect);
			Task.Run(() =>
			{
				if (Client.Connect(Settings.Instance.Parametrs.Client.Address, Settings.Instance.Parametrs.Client.Port))
				{
					Client.Connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
					Client.Connection.CallbackUpdateKeysEvent += Connection_CallbackUpdateKeysEvent;
					LastUpdateKeys = DateTime.Now;
				}
			});
		}
		public static void AutoConnect()
		{
			if (IsAutoConnect)
				return;
			Log.WriteLine("AutoConnect start");
			IsAutoConnect = true;
			while (Settings.Instance.Parametrs.Client.AutoStartClient)
			{
				if (CenterCertificationsPageVM.EnableConnectButton)
				{
					Connect();
				}
				Thread.Sleep(100);
			}
			IsAutoConnect = false;
			Log.WriteLine("AutoConnect end");
		}
		private static void Connection_CallbackUpdateKeysEvent()
		{
			LastUpdateKeys = DateTime.Now;	
		}

		private static void Connection_CallbackReceiveEvent(Net.Packets.BasePacket packet)
		{
			CenterCertificationsPageVM.ClientStatusText = $"ЦС подключен!";
			CenterCertificationsPageVM.PingText = $"{Math.Round(Client.Ping, 2)} мс.";
			CenterCertificationsPageVM.LastUpdateKeysText = $"{Math.Round((DateTime.Now - LastUpdateKeys).TotalSeconds, 0)} сек. назад";
		}

		private static void Client_CallbackClientStatusEvent(Net.Tcp.ConnectStatus connect)
		{
			if (connect == Net.Tcp.ConnectStatus.Connect)
			{
				CenterCertificationsPageVM.ClientStatusText = $"ЦС подключен!";
				CenterCertificationsPageVM.EnableDisconnectButton = true;
				CenterCertificationsPageVM.EnableConnectButton = false;
			}
			else if (connect == Net.Tcp.ConnectStatus.Connecting)
			{
				CenterCertificationsPageVM.ClientStatusText = $"Подключение к ЦС...";
				CenterCertificationsPageVM.EnableDisconnectButton = false;
				CenterCertificationsPageVM.EnableConnectButton = false;

				CenterCertificationsPageVM.PingText = $"";
				CenterCertificationsPageVM.LastUpdateKeysText = $"";
			}
			else if (connect == Net.Tcp.ConnectStatus.Disconnect)
			{
				CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
				CenterCertificationsPageVM.EnableDisconnectButton = false;
				CenterCertificationsPageVM.EnableConnectButton = true;
				CenterCertificationsPageVM.PingText = $"";
				CenterCertificationsPageVM.LastUpdateKeysText = $"";
			}
		}
		public static bool CheckValidCertificate(Certificates.Certificate certificate)
		{
			Packet packet = new Packet();
			packet.Type = PacketType.ValidatingCertificate;
			packet.Data = certificate.Raw();
			return (bool)Client.Connection.SendAndWait(packet).Data;
		}
		public static Certificates.Certificate RequestCertificate(Guid guid)
		{
			Packet packet = new Packet();
			packet.Type = PacketType.RequestCertificate;
			packet.Data = guid;
			return (Certificate)Client.Connection.SendAndWait(packet).Data;
		}
		public static Certificates.Certificate[] GetLastCertificates()
		{
			Packet packet = new Packet();
			packet.Type = PacketType.RequestCertificates;
			var certs = Client.Connection.SendAndWait(packet).Data;
			List<Certificate> certificates = new List<Certificate>();
			foreach (var i in (string[])certs)
			{
				certificates.Add(new Certificate().FromRaw(i));
			}
			return certificates.ToArray();
		}
		public static void Disconnect()
		{
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			Task.Run(() =>
			{
				Client.Disconnect();
				Client.Connection.Abort();
			});
		}
	}
}
