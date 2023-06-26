using Microsoft.Extensions.DependencyInjection;
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
		private static RCS.Net.Tcp.RCSTCPClient Client = new Net.Tcp.RCSTCPClient();
		private static CenterCertificationsPageVM CenterCertificationsPageVM => App.Host.Services.GetRequiredService<CenterCertificationsPageVM>();
		public static void Connect()
		{
			Client = new Net.Tcp.RCSTCPClient();
			Client.CallbackClientStatusEvent += Client_CallbackClientStatusEvent;
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			Task.Run(() =>
			{
				if (Client.Connect(new System.Net.IPAddress(Settings.Instance.Parametrs.Client.IPAddress), Settings.Instance.Parametrs.Client.Port))
				{
					Client.Connection.CallbackReceiveEvent += Connection_CallbackReceiveEvent;
				}
			});
		}

		private static void Connection_CallbackReceiveEvent(Net.Packets.BasePacket packet)
		{
			CenterCertificationsPageVM.ClientStatusText = $"ЦС подключен! пинг: {Math.Round(Client.Ping, 2)} мс.";
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
			}
			else if (connect == Net.Tcp.ConnectStatus.Disconnect)
			{
				CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
				CenterCertificationsPageVM.EnableDisconnectButton = false;
				CenterCertificationsPageVM.EnableConnectButton = true;
			}
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
