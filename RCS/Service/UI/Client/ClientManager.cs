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
		public static CenterCertificationsPageVM CenterCertificationsPageVM => App.Host.Services.GetRequiredService<CenterCertificationsPageVM>();
		private static bool IsAutoConnect = false;
		public static void Connect()
		{
			CertificateManager.RCSTCPClient = new Net.Tcp.RCSTCPClient();
			CertificateManager.RCSTCPClient.CallbackClientStatusEvent += Client_CallbackClientStatusEvent;
			CertificateManager.RCSTCPClient.PublicKey = Settings.Instance.Certificate.Info.PublicKey;
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			CertificateManager.RCSTCPClient.TimeoutUpdateKeys = Settings.Instance.Parametrs.Client.TimeoutUpdateKeys;
			Task.Run(AutoConnect);
			Task.Run(() =>
			{
				if (CertificateManager.RCSTCPClient.Connect(Settings.Instance.Parametrs.Client.Address, Settings.Instance.Parametrs.Client.Port))
				{
					CertificateManager.RCSTCPClient.Connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
					CertificateManager.RCSTCPClient.Connection.CallbackUpdateKeysEvent += Connection_CallbackUpdateKeysEvent;
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
			CenterCertificationsPageVM.PingText = $"{Math.Round(CertificateManager.RCSTCPClient.Ping, 2)} мс.";
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
		
		public static void Disconnect()
		{
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			Task.Run(() =>
			{
				CertificateManager.RCSTCPClient.Disconnect();
				CertificateManager.RCSTCPClient.Connection.Abort();
			});
		}
	}
}
