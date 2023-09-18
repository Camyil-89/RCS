using EasyTCP;
using Microsoft.Extensions.DependencyInjection;
using RCS.Certificates;
using RCS.Net.Packets;
using RCS.ViewModels.Pages.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenGost.Security.Cryptography;
using EasyTCP.Serialize;
using RCS.Net;

namespace RCS.Service.UI.Client
{
	public static class ClientManager
	{
		private static ISerialization Serialize = new StandardSerialize();
		private static DateTime LastUpdateKeys = DateTime.Now;
		public static CenterCertificationsPageVM CenterCertificationsPageVM => App.Host.Services.GetRequiredService<CenterCertificationsPageVM>();
		private static bool IsAutoConnect = false;
		//private static RCS.Net.Tcp.ConnectStatus LastStatus = Net.Tcp.ConnectStatus.Disconnect;
		public static void Connect()
		{
			OpenGostCryptoConfig.ConfigureCryptographicServices();
			CertificateManager.Client = new EasyTCP.Client();
			if (Settings.Instance.Certificate != null)
			{
				try
				{
					var cert = new X509Certificate2((byte[])Settings.Instance.Certificate.Info.Attributes.FirstOrDefault(i => i.Name == "RCS.SSL.CERTIFICATE.X509").Data);
					CertificateManager.Client.EnableSsl(cert, false);
				} catch (Exception ex) { MessageBoxHelper.ErrorShow($"Не удалось запусть SSL шифрование!\n\n{ex}"); }
			}
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			CertificateManager.Client.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.Ping>(1).CallbackReceiveEvent += ClientManager_CallbackReceiveEvent;
			CertificateManager.Client.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketCertificate>(2);
			CertificateManager.Client.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketRequestCertificates>(3);
			CertificateManager.Client.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketRequestCertificate>(4);
			CertificateManager.Client.PacketEntityManager.RegistrationPacket<RCS.Net.Packets.PacketRequestNetCertificates>(5);

			Task.Run(() =>
			{
				try
				{
					foreach (var i in CertificateManager.Client.ConnectWithInfo(Settings.Instance.Parametrs.Client.Address, Settings.Instance.Parametrs.Client.Port, Serialize))
					{
						switch (i.Status)
						{
							case EasyTCP.ConnectStatus.Connecting:
								{
									CenterCertificationsPageVM.ClientStatusText = $"Подключение к ЦС...";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = false;
									break;
								}
							case EasyTCP.ConnectStatus.WaitResponseFromServer:
								{
									CenterCertificationsPageVM.ClientStatusText = $"Ожидание ответа от ЦС...";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = false;
									break;
								}
							case EasyTCP.ConnectStatus.FailConnectBlockFirewall:
								CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
								CenterCertificationsPageVM.EnableDisconnectButton = false;
								CenterCertificationsPageVM.EnableConnectButton = true;
								CenterCertificationsPageVM.PingText = $"";
								CenterCertificationsPageVM.LastUpdateKeysText = $"";
								MessageBoxHelper.WarningShow($"Подключение было заблокировано браудмером сервера!\nКод: {i.Firewall.Code}\nОтвет: {i.Firewall.Answer}");
								break;
							case EasyTCP.ConnectStatus.NotFoundServer:
								{
									CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = true;
									CenterCertificationsPageVM.PingText = $"";
									CenterCertificationsPageVM.LastUpdateKeysText = $"";
									break;
								}
							case EasyTCP.ConnectStatus.OK:
								{
									CenterCertificationsPageVM.ClientStatusText = $"ЦС подключен!";
									CenterCertificationsPageVM.EnableDisconnectButton = true;
									CenterCertificationsPageVM.EnableConnectButton = false;
									Task.Run(PingServer);
									break;
								}
							case EasyTCP.ConnectStatus.TimeoutConnectToServer:
								{
									CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = true;
									CenterCertificationsPageVM.PingText = $"";
									CenterCertificationsPageVM.LastUpdateKeysText = $"";
									break;
								}
							case EasyTCP.ConnectStatus.InitConnect:
								{
									CenterCertificationsPageVM.ClientStatusText = $"Подключение к ЦС...";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = false;
									break;
								}
							case EasyTCP.ConnectStatus.InitSerialization:
								{
									CenterCertificationsPageVM.ClientStatusText = $"Подключение к ЦС...";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = false;
									break;
								}
							case EasyTCP.ConnectStatus.Fail:
								{
									CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
									CenterCertificationsPageVM.EnableDisconnectButton = false;
									CenterCertificationsPageVM.EnableConnectButton = true;
									CenterCertificationsPageVM.PingText = $"";
									CenterCertificationsPageVM.LastUpdateKeysText = $"";
									break;
								}
							default:
								break;
						}
					}
				}
				catch
				{
					CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
					CenterCertificationsPageVM.EnableDisconnectButton = false;
					CenterCertificationsPageVM.EnableConnectButton = true;
					CenterCertificationsPageVM.PingText = $"";
					CenterCertificationsPageVM.LastUpdateKeysText = $"";
				}
				if (Settings.Instance.Parametrs.Client.AutoStartClient)
				{
					Task.Run(AutoConnect);
				}
			});
		}
		private static void ClientManager_CallbackReceiveEvent(object packet, EasyTCP.Packets.Packet rawPacket)
		{
			rawPacket.Answer(rawPacket);
		}

		private static void PingServer()
		{
			while (CertificateManager.Client.CheckConnectionWithServer())
			{
				try
				{
					//CenterCertificationsPageVM.LastUpdateKeysText = $"{(int)(DateTime.Now - LastUpdateKeys).TotalSeconds} сек.";
					var ping = CertificateManager.Client.SendAndWaitResponse<RCS.Net.Packets.Ping>(new Ping(), 5000);
					CenterCertificationsPageVM.PingText = $"{Math.Round((DateTime.Now - ping.Time).TotalMilliseconds, 2)} мс.";
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
				Thread.Sleep(1000);
			}
			Disconnect();
			Console.WriteLine($"END ping");
		}
		public static void AutoConnect()
		{
			if (IsAutoConnect)
				return;
			Log.WriteLine("AutoConnect start");
			IsAutoConnect = true;
			while (Settings.Instance.Parametrs.Client.AutoStartClient && IsAutoConnect)
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
		public static void Disconnect()
		{
			CenterCertificationsPageVM.EnableDisconnectButton = false;
			CenterCertificationsPageVM.EnableConnectButton = false;
			Task.Run(() =>
			{
				try
				{
					CertificateManager.Client.Close();
				}
				catch { }
				CenterCertificationsPageVM.ClientStatusText = $"ЦС не подключен!";
				CenterCertificationsPageVM.EnableDisconnectButton = false;
				CenterCertificationsPageVM.EnableConnectButton = true;
				CenterCertificationsPageVM.PingText = $"";
				CenterCertificationsPageVM.LastUpdateKeysText = $"";
			});
		}
	}
}
